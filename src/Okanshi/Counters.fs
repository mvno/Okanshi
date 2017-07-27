namespace Okanshi

open System
open Okanshi.Helpers

/// Tracks how often some event occurs
type ICounter<'T> = 
    inherit IMonitor
    
    /// Increment the counter by one
    abstract Increment : unit -> unit
    
    /// Increment the counter by the specified amount
    abstract Increment : 'T -> unit

/// A simple counter backed by a StepLong. The value is the rate for the previous interval as defined by the step.
type StepCounter(config : MonitorConfig, step : TimeSpan, clock : IClock) = 
    let value = new StepLong(step, clock)
    let stepMilliseconds = double step.TotalMilliseconds
    let stepsPerSecond = double 1000 / stepMilliseconds

    new(config, step) = new StepCounter(config, step, new SystemClock())
    
    /// Increment the counter by one
    member __.Increment() = value.Increment(1L) |> ignore
    
    /// Increment the counter by the specified amount
    member __.Increment(amount) = 
        if amount > 0L then value.Increment(amount) |> ignore
    
    /// Gets rate of events per second
    member __.GetValue() : double = 
        let datapoint = value.Poll()
        if datapoint = Datapoint.Empty then Double.NaN
        else double datapoint.Value / stepsPerSecond
    
    /// Gets the monitor config
    member __.Config = config.WithTag(DataSourceType.Rate)
    
    interface ICounter<int64> with
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config

/// Counter tracking the maximum count
type PeakCounter(config : MonitorConfig) = 
    let mutable peakRate = 0L
    let mutable current = 0L
    let syncRoot = new obj()

    let getValue' () = peakRate
    let increment' amount =
        current <- current + amount
        if current > peakRate then peakRate <- current

    let reset'() =
        peakRate <- 0L
        current <- 0L

    let getValueAndReset'() =
        let result = getValue'()
        reset'()
        result
    
    /// Gets the maximum count
    member __.GetValue() = Lock.lock syncRoot getValue'
    
    /// Increment the value by one
    member self.Increment() = self.Increment(1L)
    
    /// Increment the value by the specified amount
    member __.Increment(amount) = lockWithArg syncRoot amount increment'
    
    /// Gets the configuration
    member __.Config = config.WithTag(DataSourceType.Counter)
    
    /// Gets the value and resets the monitor
    member __.GetValueAndReset() = Lock.lock syncRoot getValueAndReset'
    
    interface ICounter<int64> with
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config

/// A simple double counter.
type DoubleCounter(config : MonitorConfig) = 
    let mutable count = new AtomicDouble()

    let rec increment' amount = 
        let originalValue = count.Get()
        let newValue = originalValue + amount
        if count.CompareAndSet(newValue, originalValue) <> originalValue then increment' amount
    
    /// Increment the value by the specified amount
    member __.Increment(amount : double) = 
        if amount > 0.0 then increment' amount
    
    /// Increment the value by one
    member self.Increment() = self.Increment(1.0)
    
    /// Gets the maximum count
    member __.GetValue() = count.Get()
    
    /// Gets the configuration
    member __.Config = config.WithTag(DataSourceType.Rate)
    
    /// Gets the value and resets the monitor
    member __.GetValueAndReset() = count.GetAndSet(0.0)
    
    interface ICounter<double> with
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config

/// A simple counter backed by an AtomicLong. The value is the total count for the life of the counter.
type BasicCounter(config : MonitorConfig) = 
    let value = new AtomicLong()
    
    /// Increment the value by one
    member __.Increment() = value.Increment() |> ignore
    
    /// Increment the value by the specified amount
    member __.Increment(amount) = value.Increment(amount) |> ignore
    
    /// Gets the value
    member __.GetValue() = value.Get()
    
    /// Gets the configuration
    member __.Config = config.WithTag(DataSourceType.Counter)
    
    /// Gets the value and resets the monitor
    member __.GetValueAndReset() = value.Get()
    
    interface ICounter<int64> with
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config
