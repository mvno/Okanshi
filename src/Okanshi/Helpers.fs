namespace Okanshi.Helpers

[<AutoOpen>]
module Lock = 
    let lock = lock
    
    let inline lockWithArg (lockObj : 'T when 'T : not struct) arg f = 
        try 
            System.Threading.Monitor.Enter(lockObj)
            f arg
        finally
            System.Threading.Monitor.Exit(lockObj)
