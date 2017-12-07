// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Core
open Fake.Core.TargetOperators
open Fake.Core.Globbing.Operators
open Fake.Tools
open Fake.ReleaseNotesHelper
open Fake.IO
open Fake.DotNet
open System.IO
open System

// --------------------------------------------------------------------------------------
// START TODO: Provide project-specific details below
// --------------------------------------------------------------------------------------

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docs/tools/generate.fsx"

// The name of the project
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "Okanshi"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "In-process monitoring solution"

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "Monitor individual services in-process and collect data somewhere else"

// List of author names (for NuGet package)
let authors = [ "Telenor MVNO" ]

// Tags for your project (for NuGet package)
let tags = "monitoring, microservices"

// Pattern specifying assemblies to be tested using NUnit
let testProjects = "tests/**/*.Tests.??proj"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted
let gitOwner = "mvno" 
let gitHome = "https://github.com/" + gitOwner

// The name of the project on GitHub
let gitName = "Okanshi"

// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps
// --------------------------------------------------------------------------------------

// Read additional information from the release notes document
let release = LoadReleaseNotes "RELEASE_NOTES.md"

// Generate assembly info files with the right version & up-to-date information
Target.Create "AssemblyInfo" (fun _ ->
    !! "src/**/*.??proj"
    |> Shell.RegexReplaceInFilesWithEncoding @"\<VersionPrefix\>.*\</VersionPrefix>"
                                       (sprintf "<VersionPrefix>%O</VersionPrefix>" release.SemVer)
                                       System.Text.Encoding.UTF8
)

// Copies binaries from default VS location to expected bin folder
// But keeps a subdirectory structure for each project in the 
// src folder to support multiple project outputs
Target.Create "CopyBinaries" (fun _ ->
    !! "src/**/*.??proj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) @@ "bin/Release", "bin" @@ (System.IO.Path.GetFileNameWithoutExtension f)))
    |>  Seq.iter (fun (fromDir, toDir) -> Shell.CopyDir toDir fromDir (fun _ -> true))
)

// --------------------------------------------------------------------------------------
// Clean build results

Target.Create "Clean" (fun _ ->
    Shell.CleanDirs ["bin"; "temp"]
)

Target.Create "CleanDocs" (fun _ ->
    Shell.CleanDirs ["docs/output"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target.Create "Build" (fun _ ->
    DotNetCli.Build
        (fun p ->
            { p with
                Configuration = "Release"
                AdditionalArgs = [ "--no-incremental" ] })
)

Target.Create "Restore" (fun _ ->
    !! "src/**/*.??proj"
    ++ "tests/**/*.??proj"
    |> Seq.iter (fun d ->
        DotNetCli.Restore (fun p ->
            { p with
                Project = d }))
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target.Create "RunTests" (fun _ ->
    !! testProjects
    |> Seq.map (fun x -> Path.GetDirectoryName(x))
    |> Seq.iter(fun x ->
        Trace.trace (sprintf "Running test for %s" x)
        DotNetCli.Test
            (fun p ->
                { p with
                    WorkingDir = x })
    )
)

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target.Create "NuGet" (fun _ ->
    !! "src/**/*.??proj"
    |> Seq.map (fun x -> Path.GetDirectoryName(x))
    |> Seq.iter (fun x ->
        DotNetCli.Pack
            (fun p ->
                { p with
                    WorkingDir = x;
                    OutputPath = "../../bin";
                    AdditionalArgs = [ "--include-symbols" ] })
    )
)


Target.Create "PublishNuget" (fun _ ->
    !! "bin/*.nupkg"
    |> Seq.filter (fun x -> x.Contains(".symbols.nupkg") |> not)
    |> Seq.map (fun x -> Path.GetFileName(x))
    |> Seq.iter (fun x ->
        Paket.PushFiles (fun p -> { p with WorkingDir = "bin" }) [x]
    )
)

// --------------------------------------------------------------------------------------
// Generate the documentation

let fakePath = "packages" </> "build" </> "FAKE" </> "tools" </> "FAKE.exe"
let fakeStartInfo script workingDirectory args fsiargs environmentVars =
    (fun (info: System.Diagnostics.ProcessStartInfo) ->
        info.FileName <- System.IO.Path.GetFullPath fakePath
        info.Arguments <- sprintf "%s --fsiargs -d:FAKE %s \"%s\"" args fsiargs script
        info.WorkingDirectory <- workingDirectory
        let setVar k v =
            info.EnvironmentVariables.[k] <- v
        for (k, v) in environmentVars do
            setVar k v
        setVar "MSBuild" msBuildExe
        setVar "GIT" Git.CommandHelper.gitPath
        setVar "FSI" fsiPath)

/// Run the given buildscript with FAKE.exe
let executeFAKEWithOutput workingDirectory script fsiargs envArgs =
    let exitCode =
        ExecProcessWithLambdas
            (fakeStartInfo script workingDirectory "" fsiargs envArgs)
            TimeSpan.MaxValue false ignore ignore
    System.Threading.Thread.Sleep 1000
    exitCode

// Documentation
let buildDocumentationTarget fsiargs target =
    Trace.trace (sprintf "Building documentation (%s), this could take some time, please wait..." target)
    let exit = executeFAKEWithOutput "docs/tools" "generate.fsx" fsiargs ["target", target]
    if exit <> 0 then
        failwith "generating reference documentation failed"
    ()

Target.Create "GenerateReferenceDocs" (fun _ ->
    buildDocumentationTarget "-d:RELEASE -d:REFERENCE" "Default"
)

let generateHelp' fail debug =
    let args =
        if debug then "--define:HELP"
        else "--define:RELEASE --define:HELP"
    try
        buildDocumentationTarget args "Default"
        Trace.traceImportant "Help generated"
    with
    | e when not fail ->
        Trace.traceImportant "generating help documentation failed"

let generateHelp fail =
    generateHelp' fail false

Target.Create "GenerateHelp" (fun _ ->
    File.delete "docs/content/release-notes.md"
    File.Copy("docs/content/", "RELEASE_NOTES.md")
    Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"

    File.delete "docs/content/license.md"
    File.Copy("docs/content/", "LICENSE.txt")
    Rename "docs/content/license.md" "docs/content/LICENSE.txt"

    generateHelp true
)

Target.Create "GenerateHelpDebug" (fun _ ->
    File.delete "docs/content/release-notes.md"
    File.Copy("docs/content/", "RELEASE_NOTES.md")
    Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"

    File.delete "docs/content/license.md"
    File.Copy("docs/content/", "LICENSE.txt")
    Rename "docs/content/license.md" "docs/content/LICENSE.txt"

    generateHelp' true true
)

Target.Create "KeepRunning" (fun _ ->    
    use watcher = new FileSystemWatcher(DirectoryInfo("docs/content").FullName,"*.*")
    watcher.EnableRaisingEvents <- true
    watcher.Changed.Add(fun e -> generateHelp false)
    watcher.Created.Add(fun e -> generateHelp false)
    watcher.Renamed.Add(fun e -> generateHelp false)
    watcher.Deleted.Add(fun e -> generateHelp false)

    Trace.traceImportant "Waiting for help edits. Press any key to stop."

    System.Console.ReadKey() |> ignore

    watcher.EnableRaisingEvents <- false
    watcher.Dispose()
)

Target.Create "GenerateDocs" ignore

let createIndexFsx lang =
    let content = """(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../../bin"

(**
F# Project Scaffold ({0})
=========================
*)
"""
    let targetDir = "docs/content" @@ lang
    let targetFile = targetDir @@ "index.fsx"
    Directory.ensure targetDir
    System.IO.File.WriteAllText(targetFile, System.String.Format(content, lang))

Target.Create "AddLangDocs" (fun _ ->
    let args = System.Environment.GetCommandLineArgs()
    if args.Length < 4 then
        failwith "Language not specified."

    args.[3..]
    |> Seq.iter (fun lang ->
        if lang.Length <> 2 && lang.Length <> 3 then
            failwithf "Language must be 2 or 3 characters (ex. 'de', 'fr', 'ja', 'gsw', etc.): %s" lang

        let templateFileName = "template.cshtml"
        let templateDir = "docs/tools/templates"
        let langTemplateDir = templateDir @@ lang
        let langTemplateFileName = langTemplateDir @@ templateFileName

        if System.IO.File.Exists(langTemplateFileName) then
            failwithf "Documents for specified language '%s' have already been added." lang

        Directory.ensure langTemplateDir
        Copy langTemplateDir [ templateDir @@ templateFileName ]

        createIndexFsx lang)
)

// --------------------------------------------------------------------------------------
// Release Scripts

Target.Create "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    CleanDir tempDocsDir
    Git.Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir

    CopyRecursive "docs/output" tempDocsDir true |> Trace.tracefn "%A"
    Git.Staging.StageAll tempDocsDir
    Git.Commit.Commit tempDocsDir (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Git.Branches.push tempDocsDir
)

#load "paket-files/build/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit

Target.Create "Release" (fun _ ->
    Git.Staging.StageAll ""
    Git.Commit.Commit "" (sprintf "Bump version to %s" release.NugetVersion)
    Git.Branches.push ""

    Git.Branches.tag "" release.NugetVersion
    Git.Branches.pushTag "" "origin" release.NugetVersion
    
    // release on github
    createClient (getBuildParamOrDefault "github-user" "") (getBuildParamOrDefault "github-pw" "")
    |> createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes 
    // TODO: |> uploadFile "PATH_TO_FILE"    
    |> releaseDraft
    |> Async.RunSynchronously
)

Target.Create "BuildPackage" ignore
// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target.Create "All" ignore

"Clean"
  ==> "AssemblyInfo"
  ==> "Restore"
  ==> "Build"
  ==> "CopyBinaries"
  ==> "RunTests"
  ==> "GenerateReferenceDocs"
  ==> "GenerateDocs"
  ==> "All"
  =?> ("ReleaseDocs",isLocalBuild)

"All"
  ==> "NuGet"
  ==> "BuildPackage"

"CleanDocs"
  ==> "GenerateHelp"
  ==> "GenerateReferenceDocs"
  ==> "GenerateDocs"

"CleanDocs"
  ==> "GenerateHelpDebug"

"GenerateHelp"
  ==> "KeepRunning"
    
"ReleaseDocs"
  ==> "Release"

"BuildPackage"
  ==> "PublishNuget"
  ==> "Release"

Target.RunOrDefault "All"
