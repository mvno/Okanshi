namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Okanshi")>]
[<assembly: AssemblyProductAttribute("Okanshi")>]
[<assembly: AssemblyDescriptionAttribute("In-process monitoring solution")>]
[<assembly: AssemblyFileVersionAttribute("2.1.0")>]
[<assembly: AssemblyVersionAttribute("2.0.0")>]
[<assembly: AssemblyInformationalVersionAttribute("2.1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "2.0.0"
    let [<Literal>] InformationalVersion = "2.1.0"
