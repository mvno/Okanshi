namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Okanshi")>]
[<assembly: AssemblyProductAttribute("Okanshi")>]
[<assembly: AssemblyDescriptionAttribute("In-process monitoring solution")>]
[<assembly: AssemblyFileVersionAttribute("3.0.0")>]
[<assembly: AssemblyVersionAttribute("3.0.0")>]
[<assembly: AssemblyInformationalVersionAttribute("3.0.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "3.0.0"
    let [<Literal>] InformationalVersion = "3.0.0"
