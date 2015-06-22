namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Okanshi")>]
[<assembly: AssemblyProductAttribute("Okanshi")>]
[<assembly: AssemblyDescriptionAttribute("In-process monitoring solution")>]
[<assembly: AssemblyFileVersionAttribute("1.0.0")>]
[<assembly: AssemblyVersionAttribute("1.0.0")>]
[<assembly: AssemblyInformationalVersionAttribute("1.0.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0.0"
