namespace Okanshi

open System

/// Utility class used to describe step intervals
[<System.Diagnostics.DebuggerDisplay("Timestamp = {Timestamp}; Value = {Value}")>]
type Datapoint =
    { Timestamp : Nullable<DateTime>; Value : int64 }
    static member Empty = { Timestamp = new System.Nullable<_>(); Value = -1L }

/// Utility class for managing a set of AtomicLong instances mapped to a particular step interval.
type StepLong(initialValue, step : TimeSpan) =
    let step = int64 step.Ticks
    [<Literal>]
    let CurrentIndex = 0
    [<Literal>]
    let PreviousIndex = 1
    
    let data = Array.init 2 (fun _ -> new AtomicLong(initialValue))
    let lastPollTime = new AtomicLong()
    let lastInitPosition = new AtomicLong()
    let rollCount now =
        let stepTime = now / step
        let lastInit = lastInitPosition.Get()
        if lastInit < stepTime && lastInitPosition.CompareAndSet(stepTime, lastInit) = lastInit then
            data.[CurrentIndex].GetAndSet(initialValue) |> data.[PreviousIndex].Set

    new (interval) = new StepLong(0L, interval)

    // Gets the current count
    member __.GetCurrent() =
        rollCount(DateTime.Now.Ticks)
        data.[CurrentIndex]

    /// Gets the value of the previous measurement
    member __.Poll() =
        let now = DateTime.Now
        let nowTicks = now.Ticks
        rollCount(nowTicks)
        let value = data.[PreviousIndex].Get()
        let last = lastPollTime.GetAndSet(nowTicks)
        let missed = (nowTicks - last) / step - 1L
        let stepStartWithWholeSeconds = new Nullable<_>(now.AddMilliseconds(float <| -now.Millisecond))
        if last > 0L && missed > 0L then Datapoint.Empty
        else { Timestamp = stepStartWithWholeSeconds; Value = value }

    /// Increment the current value by the specified amount
    member self.Increment(amount) =
        self.GetCurrent().Increment(amount)

/// Tracks how often some event occurs
type ICounter<'T> =
    inherit IMonitor
    /// Increment the counter by one
    abstract Increment : unit -> unit
    /// Increment the counter by the specified amount
    abstract Increment : 'T -> unit

/// A simple counter backed by a StepLong. The value is the rate for the previous interval as defined by the step.
type StepCounter(config : MonitorConfig, step : TimeSpan) =
    let value = new StepLong(step)
    let stepMilliseconds = double step.TotalMilliseconds
    
    /// Increment the counter by one
    member __.Increment() = value.Increment(1L) |> ignore
    /// Increment the counter by the specified amount
    member __.Increment(amount) = if amount > 0L then value.Increment(amount) |> ignore
    /// Gets rate of events per second
    member __.GetValue() : double =
        let datapoint = value.Poll()
        if datapoint = Datapoint.Empty then Double.NaN
        else double datapoint.Value / stepMilliseconds
    /// Gets the monitor config
    member __.Config = config.WithTag(DataSourceType.Rate)

    interface ICounter<int64> with
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config

/// Counter tracking the maximum count per second within a specified interval
type PeakRateCounter(config : MonitorConfig, step) =
    let currentSecond = new AtomicLong()
    let currentCount = new AtomicLong()
    let peakRate = new StepLong(step)
    let updatePeak value =
        let current = peakRate.GetCurrent()
        let rec updateValue () =
            let originalValue = current.Get()
            if current.CompareAndSet(value, originalValue) <> originalValue then updateValue()
        updateValue()

    /// Gets the peak rate within the specified interval
    member __.GetValue() = peakRate.GetCurrent().Get()
    /// Increment the value by one
    member self.Increment() = self.Increment(1L)
    /// Increment the value by the specified amount
    member __.Increment(amount) =
        let now = DateTimeOffset.UtcNow.Ticks / TimeSpan.TicksPerSecond
        if now <> currentSecond.Get() then
            currentCount.Set(0L)
            currentSecond.Set(now)
        currentCount.Increment(amount) |> ignore
        currentCount.Get() |> updatePeak
    /// Gets the configuration
    member __.Config = config.WithTag(DataSourceType.Rate)

    interface ICounter<int64> with
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config

/// A simple double counter backed by a StepLong but using doubles. The value is the rate for the previous interval as defined by the step.
type DoubleCounter(config : MonitorConfig, step : TimeSpan) =
    let stepSeconds = double step.TotalSeconds
    let count = new StepLong(step)
    let add (amount : double) =
        let current = count.GetCurrent()
        let rec loop () =
            let originalValue = current.Get()
            let originalDouble = BitConverter.Int64BitsToDouble(originalValue)
            let nextDouble = BitConverter.DoubleToInt64Bits(originalDouble + amount)
            if current.CompareAndSet(nextDouble, originalValue) <> originalValue then loop()
        loop()

    /// Increment the value by the specified amount
    member __.Increment(amount : double) =
        if amount > 0.0 then add amount
    /// Increment the value by one
    member self.Increment() = self.Increment(1.0)
    /// Gets the rate per second
    member __.GetValue() =
        let datapoint = count.Poll()
        if datapoint = Datapoint.Empty then Double.NaN
        else (datapoint.Value |> BitConverter.Int64BitsToDouble) / stepSeconds
    /// Gets the configuration
    member __.Config = config.WithTag(DataSourceType.Rate)

    interface ICounter<double> with
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config

/// A simple counter backed by an AtomicLong. The value is the total count for the life of the counter.
/// Observers are responsible for converting to a rate and handling overflows if they occur.
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

    interface ICounter<int64> with
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config
