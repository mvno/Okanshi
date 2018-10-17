namespace Okanshi.Helpers

open System.Collections.Generic

[<AutoOpen>]
module DictionaryHelp =
    let dic (keyValues : seq<string>) : Dictionary<string, string> =
        let result = new Dictionary<string, string>()
        Seq.iter2 (fun a b -> result.Add(a,b)) keyValues (Seq.skip 1 keyValues )
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
