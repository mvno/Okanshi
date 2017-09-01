namespace Okanshi

open System.Diagnostics
open System

/// Logging class
[<AbstractClass; Sealed>]
type Logger private () =
    static let msgFormatter level msg =
        String.Format("{0} {1}: {2}", DateTime.Now.ToShortDateString, level, msg)

    /// Logs a trace message
    static member val Trace : Action<string> = new Action<_>(fun x -> Debug.WriteLine(msgFormatter "TRACE" x)) with get, set

    /// Logs a debug message
    static member val Debug : Action<string> = new Action<_>(fun x -> Debug.WriteLine(msgFormatter "DEBUG" x)) with get, set

    /// Logs an information message
    static member val Info : Action<string> = new Action<_>(fun x -> Console.WriteLine(msgFormatter "INFO" x)) with get, set

    /// Logs a warning
    static member val Warn : Action<string> = new Action<_>(fun x -> Console.WriteLine(msgFormatter "WARN" x)) with get, set

    /// Logs an error
    static member val Error : Action<string, exn> = new Action<_, _>(fun x e -> Console.Error.WriteLine("{0}:\n{1}", msgFormatter "ERROR" x, e)) with get, set

    /// Logs a fatal error
    static member val Fatal : Action<string, exn> = new Action<_, _>(fun x e -> Console.Error.WriteLine("{0}:\n{1}", msgFormatter "FATAL" x, e)) with get, set
