namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Okanshi")>]
[<assembly: AssemblyProductAttribute("Okanshi")>]
[<assembly: AssemblyDescriptionAttribute("In-process monitoring solution")>]
[<assembly: AssemblyVersionAttribute("1.0.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0.0"
