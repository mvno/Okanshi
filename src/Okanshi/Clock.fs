namespace Okanshi

open System

/// Interface for defining the clock used
type IClock =
    /// Gets the current time
    abstract member Now : unit -> DateTime
    /// Gets the current ticks
    abstract member NowTicks : unit -> int64

/// Wrapper around the system time
type SystemClock() =
    static let instance = new SystemClock()

    /// Gets the current time
    member __.Now() = DateTime.UtcNow
    
    /// Gets the current ticks
    member __.NowTicks() = DateTime.UtcNow.Ticks

    static member Instance = instance

    interface IClock with
        /// Gets the current time
        member self.Now() = self.Now()
        /// Gets the current ticks
        member self.NowTicks() = self.NowTicks()

/// Manual clock, should only be used in tests
type ManualClock() =
    let mutable currentTime = DateTime.UtcNow

    /// Gets the current time
    member __.Now() = currentTime

    /// Gets the current ticks
    member __.NowTicks() = currentTime.Ticks

    /// Sets time current time
    member __.Set(time : DateTime) =
        currentTime <- time

    /// Advance the time
    member __.Advance(span : TimeSpan) =
        currentTime <- currentTime.Add(span)

    interface IClock with
        /// Gets the current time
        member self.Now() = self.Now()
        /// Gets the current ticks
        member self.NowTicks() = self.NowTicks()