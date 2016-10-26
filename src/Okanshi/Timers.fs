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

/// A monitor for tracking a longer operation that might last for many minutes or hours. For tracking
/// frequent calls that last less than the polling interval, use the BasicTimer instead.
/// This timer can track multiple operations, each started by calling the Record method.
/// This monitor will create two gauges:
/// * A duration which reports the current duration in seconds. The duration is the sum of all active tasks
/// * Number of active tasks
/// The names of the monitors will be the base name passed in the config to the constructor, suffixed by
/// .duration and .activetasks respectively.
type DurationTimer(registry : IMonitorRegistry, config : MonitorConfig) =
    let nextTaskId = new AtomicLong()
    let tasks = new System.Collections.Concurrent.ConcurrentDictionary<int64, int64>()
    let activeTasks = new BasicGauge<int>({ config with Name = sprintf "%s.activetasks" config.Name }, fun () -> tasks.Count)

    let getDurationInSeconds () =
        let now = DateTime.Now.Ticks
        let durationInTicks = tasks.Values |> Seq.sumBy (fun x -> now - x)
        TimeSpan.FromTicks(durationInTicks).TotalSeconds

    let totalDurationInSeconds = new BasicGauge<float>({ config with Name = sprintf "%s.duration" config.Name }, fun () -> getDurationInSeconds())
    
    let record (f : unit -> 'a)  =
        let id = nextTaskId.Increment()
        tasks.TryAdd(id, DateTime.Now.Ticks) |> ignore
        try
            f()
        finally
            tasks.TryRemove(id) |> ignore

    do
        registry.Register(activeTasks)
        registry.Register(totalDurationInSeconds)

    new (config) = DurationTimer(DefaultMonitorRegistry.Instance, config)

    /// Time a System.Func call and return the value
    member __.Record(f : Func<'T>) = record (fun () -> f.Invoke())
    /// Time a System.Action call
    member __.Record(f : Action) = record (fun () -> f.Invoke())
    /// Get the number of running tasks
    member __.GetNumberOfActiveTasks() = activeTasks.GetValue()
    /// Get the duration in seconds. Duration is the sum of all active tasks duration.
    member __.GetDurationInSeconds() = totalDurationInSeconds.GetValue()
    /// Get the duration in seconds. Duration is the sum of all active tasks duration.
    member self.GetValue() = self.GetDurationInSeconds()
    /// Gets the monitor config
    member __.Config = totalDurationInSeconds.Config

    interface ITimer with
        member self.Record(f : Func<'T>) = self.Record(f)
        member self.Record(f : Action) = self.Record(f)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config

