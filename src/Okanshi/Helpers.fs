namespace Okanshi.Helpers

[<AutoOpen>]
module Lock = 
    let lock = lock
    
    let inline lockWithArg (lockObj : 'T when 'T : not struct) arg f = 
        let mutable lockTaken = false
        try 
            System.Threading.Monitor.Enter(lockObj, ref lockTaken)
            f arg
        finally
            if lockTaken then System.Threading.Monitor.Exit(lockObj)
