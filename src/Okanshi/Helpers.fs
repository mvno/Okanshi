namespace Okanshi.Helpers

open System.Collections.Generic

[<AutoOpen>]
module DictionaryHelp =
    let dic (keyValues : seq<string>) : Dictionary<string, string> =
        let x = Array.ofSeq keyValues
        let result = new Dictionary<string, string>()
        for i in [0..2..x.Length-1] do
            result.Add(x.[i], x.[i+1])
        result

[<AutoOpen>]
module Lock = 
    let lock = lock
    
    let inline lockWithArg (lockObj : 'T when 'T : not struct) arg f = 
        try 
            System.Threading.Monitor.Enter(lockObj)
            f arg
        finally
            System.Threading.Monitor.Exit(lockObj)
