namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Okanshi")>]
[<assembly: AssemblyProductAttribute("Okanshi")>]
[<assembly: AssemblyDescriptionAttribute("In-process monitoring solution")>]
[<assembly: AssemblyFileVersionAttribute("4.0.0")>]
[<assembly: AssemblyVersionAttribute("4.0.0")>]
[<assembly: AssemblyInformationalVersionAttribute("4.0.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "4.0.0"
    let [<Literal>] InformationalVersion = "4.0.0"
