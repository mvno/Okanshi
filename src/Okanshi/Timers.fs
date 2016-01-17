namespace Okanshi

open System
open System.Diagnostics

/// Extenstions for the System.Diagnostics.StopWatch class
[<AutoOpen>]
module StopwatchExtensions =
    type Stopwatch with
        /// Time a function and return the result and elapsed milliseconds as a tuple
        static member Time f =
            let stopwatch = Stopwatch.StartNew()
            let result = f()
            stopwatch.Stop()
            (result, stopwatch.ElapsedMilliseconds)

/// Times System.Action and Systen.Func calls
type ITimer =
    inherit IMonitor
    /// Time a System.Func call and return the value
    abstract Record : Func<'T> -> 'T
    /// Time a System.Action call
    abstract Record : Action -> unit

/// A simple timer providing the total time, count, min and max for the times that have been recorded
type BasicTimer(registry : IMonitorRegistry, config : MonitorConfig, step) =
    [<Literal>]
    let StatisticKey = "statistic"

    let max = new MaxGauge(config.WithTag(StatisticKey, "max"))
    let min = new MinGauge(config.WithTag(StatisticKey, "min"))
    let count = new StepCounter(config.WithTag(StatisticKey, "count"), step)
    let total = new StepCounter(config.WithTag(StatisticKey, "totalTime"), step)

    let record f =
        let (result, elapsed) = Stopwatch.Time(fun () -> f())
        count.Increment() |> ignore
        total.Increment(elapsed)
        max.Set(elapsed)
        min.Set(elapsed)
        result

    do
        registry.Register(max)
        registry.Register(min)
        registry.Register(count)
        registry.Register(total)

    new (config, step) = BasicTimer(DefaultMonitorRegistry.Instance, config, step)

    /// Time a System.Func call and return the value
    member __.Record(f: Func<'T>) = record (fun () -> f.Invoke())
    /// Time a System.Action call
    member __.Record(f: Action) = record (fun () -> f.Invoke())
    /// Gets the rate of calls timed within the specified step
    member __.GetCount() = count.GetValue()
    /// Gets the average calls time within the specified step
    member self.GetValue() : double =
        let count = self.GetCount()
        if count = 0.0 then 0.0
        else self.GetTotalTime() / count
    /// Get the maximum value of all calls
    member __.GetMax() = max.GetValue()
    /// Get the manimum value of all calls
    member __.GetMin() = min.GetValue()
    /// Gets the rate of the total time for all calls within the specified step
    member __.GetTotalTime() = total.GetValue()
    /// Gets the monitor config
    member __.Config = config.WithTag(StatisticKey, "avg").WithTag(DataSourceType.Rate)

    interface ITimer with
        member self.Record(f : Func<'T>) = self.Record(f)
        member self.Record(f : Action) = self.Record(f)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config
