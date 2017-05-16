namespace Okanshi

open System

/// Interface for defining the clock used
type IClock = 
    
    /// Gets the current time
    abstract Now : unit -> DateTime
    
    /// Gets the current ticks
    abstract NowTicks : unit -> int64
    
    /// Freezes the time. Used to make sure composite metrics go into the same
    /// buckets across atomics
    abstract Freeze : unit -> unit
    
    /// Unfreezes the time
    abstract Unfreeze : unit -> unit

/// Wrapper around the system time
type SystemClock() = 
    let mutable frozenAt : DateTime option = None
    let freeze() = frozenAt <- Some(DateTime.UtcNow)
    let unfreeze() = frozenAt <- None
    
    let now() = 
        match frozenAt with
        | Some(x) -> x
        | None -> DateTime.UtcNow
    
    /// Gets the current time
    member __.Now() = 
        match frozenAt with
        | Some(x) -> x
        | None -> DateTime.UtcNow
    
    /// Gets the current ticks
    member self.NowTicks() = self.Now().Ticks
    
    /// Freezes the time. Used to make sure composite metrics go into the same
    /// buckets across atomics
    member __.Freeze() = frozenAt <- Some(DateTime.UtcNow)
    
    /// Unfreezes the time
    member __.Unfreeze() = frozenAt <- None
    
    interface IClock with
        
        /// Gets the current time
        member self.Now() = self.Now()
        
        /// Gets the current ticks
        member self.NowTicks() = self.NowTicks()
        
        /// Freezes the time. Used to make sure composite metrics go into the same
        /// buckets across atomics
        member self.Freeze() = ()
        
        /// Unfreezes the time
        member self.Unfreeze() = ()

/// Manual clock, should only be used in tests
type ManualClock() = 
    let mutable currentTime = DateTime.UtcNow
    
    /// Gets the current time
    member __.Now() = currentTime
    
    /// Gets the current ticks
    member __.NowTicks() = currentTime.Ticks
    
    /// Sets time current time
    member __.Set(time : DateTime) = currentTime <- time
    
    /// Advance the time
    member __.Advance(span : TimeSpan) = currentTime <- currentTime.Add(span)
    
    interface IClock with
        
        /// Gets the current time
        member self.Now() = self.Now()
        
        /// Gets the current ticks
        member self.NowTicks() = self.NowTicks()
        
        /// Does nothing in ManualClock, as time is always frozen
        member self.Freeze() = ()
        
        /// Does nothing in ManualClock, as time is always frozen
        member self.Unfreeze() = ()
