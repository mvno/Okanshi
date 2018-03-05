namespace Okanshi

open System
open System.Diagnostics
open Okanshi.Helpers

type IStopwatch =
    abstract ElapsedMilliseconds : int64
    abstract IsRunning : bool
    abstract Start : unit -> unit
    abstract Stop : unit -> unit
    abstract Time : Func<'T> -> ('T * int64)
    abstract Time : Action -> int64

type SystemStopwatch() =
    let stopwatch = new Stopwatch()

    let time' f =
        stopwatch.Start()
        let result = f()
        stopwatch.Stop()
        (result, stopwatch.ElapsedMilliseconds)

    member __.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
    
    member __.IsRunning = stopwatch.IsRunning

    member __.Start() = stopwatch.Start()
    
    member __.Stop() = stopwatch.Stop()
    
    member __.Time(f: Func<'T>) = time' (fun () -> f.Invoke())

    member __.Time(f: Action) =
        let (_, elapsed) = time' (fun () -> f.Invoke())
        elapsed

    interface IStopwatch with
        member self.ElapsedMilliseconds = self.ElapsedMilliseconds
        member self.IsRunning = self.IsRunning
        member self.Start() = self.Start()
        member self.Stop() = self.Stop()
        member self.Time(f: Func<'T>) : ('T * int64) = self.Time(f)
        member self.Time(f: Action) = self.Time(f)

/// Timer that is started and stopped manually
type OkanshiTimer(onStop : Action<int64>, stopwatchFactory: Func<IStopwatch>) = 
    let mutable stopwatch = Some <| stopwatchFactory.Invoke()

    new(onStop) = OkanshiTimer(onStop, fun () -> SystemStopwatch() :> IStopwatch)
    
    /// Start the timer
    member __.Start() = 
        if stopwatch.IsNone then invalidOp "Timer cannot be used multiple times"
        if stopwatch.Value.IsRunning then invalidOp "Timer already started"
        stopwatch.Value.Start()
    
    /// Stop the timer
    member __.Stop() = 
        if stopwatch.IsNone then invalidOp "Timer cannot be used multiple times"
        if not stopwatch.Value.IsRunning then invalidOp "Timer not started"
        stopwatch.Value.Stop()
        onStop.Invoke(stopwatch.Value.ElapsedMilliseconds)
        stopwatch <- None

/// Times System.Action and Systen.Func calls
type ITimer = 
    inherit IMonitor
    
    /// Time a System.Func call and return the value
    abstract Record : Func<'T> -> 'T
    
    /// Time a System.Action call
    abstract Record : Action -> unit
    
    /// Start a manually controlled timinig
    abstract Start : unit -> OkanshiTimer

    /// Manually register a timing, should only be used in special case
    abstract Register : int64 -> unit

/// A simple timer providing the total time, count, min and max for the times that have been recorded
type BasicTimer(config : MonitorConfig, stopwatchFactory : Func<IStopwatch>) as self = 
    
    [<Literal>]
    let StatisticKey = "statistic"
    
    let max = new MaxGauge(config.WithTag(StatisticKey, "max"))
    let min = new MinGauge(config.WithTag(StatisticKey, "min"))
    let count = new PeakCounter(config.WithTag(StatisticKey, "count"))
    let total = new PeakCounter(config.WithTag(StatisticKey, "totalTime"))
    let avg = new AverageGauge(config)
    let syncRoot = new obj()

    let updateStatistics' elapsed =
        count.Increment() |> ignore
        total.Increment(elapsed)
        max.Set(elapsed)
        min.Set(elapsed)
        avg.Set(float elapsed)
    
    let updateStatistics elapsed = 
        lockWithArg syncRoot elapsed updateStatistics'
    
    let record f = 
        let stopwatch = stopwatchFactory.Invoke()
        let (result, elapsed) = stopwatch.Time(fun () -> f())
        elapsed |> updateStatistics
        result

    let getValues' () =
        seq {
            yield! avg.GetValues() |> Seq.map (fun x -> Measurement("value", x.Value)) |> Seq.cast<IMeasurement>
            yield! total.GetValues() |> Seq.map (fun x -> Measurement("totalTime", x.Value)) |> Seq.cast<IMeasurement>
            yield! count.GetValues() |> Seq.map (fun x -> Measurement("count", x.Value)) |> Seq.cast<IMeasurement>
            yield! max.GetValues() |> Seq.map (fun x -> Measurement("max", x.Value)) |> Seq.cast<IMeasurement>
            yield! min.GetValues() |> Seq.map (fun x -> Measurement("min", x.Value)) |> Seq.cast<IMeasurement>
        }

    let reset'() =
        max.Reset()
        min.Reset()
        count.GetValuesAndReset() |> ignore
        total.GetValuesAndReset() |> ignore
        avg.GetValuesAndReset() |> ignore

    let getValuesAndReset'() =
        let result = self.GetValues() |> Seq.toList
        reset'()
        result |> List.toSeq

    new(config: MonitorConfig) = BasicTimer(config, fun () -> SystemStopwatch() :> IStopwatch)
    
    /// Time a System.Func call and return the value
    member __.Record(f : Func<'T>) =
        let stopwatch = stopwatchFactory.Invoke()
        let (result, elapsed) = stopwatch.Time(f)
        elapsed |> updateStatistics
        result
    
    /// Time a System.Action call
    member __.Record(f : Action) =
        let stopwatch = stopwatchFactory.Invoke()
        let elapsed = stopwatch.Time(f)
        elapsed |> updateStatistics
    
    /// Gets the rate of calls timed within the specified step
    member __.GetCount() = lock syncRoot (fun() -> count.GetValues() |> Seq.head)
    
    /// Gets the average calls time within the specified step
    member __.GetValues() = lock syncRoot getValues'
    
    /// Get the maximum value of all calls
    member __.GetMax() = lock syncRoot (fun () -> max.GetValues() |> Seq.head)
    
    /// Get the manimum value of all calls
    member __.GetMin() = lock syncRoot (fun () -> min.GetValues() |> Seq.head)
    
    /// Gets the the total time for all calls within the specified step
    member __.GetTotalTime() = lock syncRoot (fun () -> total.GetValues() |> Seq.head)
    
    /// Gets the monitor config
    member __.Config = config.WithTag(StatisticKey, "avg").WithTag(DataSourceType.Rate)
    
    /// Start a manually controlled timinig
    member __.Start() =
        OkanshiTimer((fun x -> updateStatistics x), (fun () -> stopwatchFactory.Invoke()))

    /// Manually register a timing, should only be used in special case
    member __.Register(elapsed) = elapsed |> updateStatistics

    /// Gets the value and resets the monitor
    member __.GetValuesAndReset() = Lock.lock syncRoot getValuesAndReset'
    
    interface ITimer with
        member self.Record(f : Func<'T>) = self.Record(f)
        member self.Record(f : Action) = self.Record(f)
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.Start() = self.Start()
        member self.Register(elapsed) = self.Register(elapsed)
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast

/// A monitor for tracking a longer operation that might last for many minutes or hours. For tracking
/// frequent calls that last less than the polling interval, use the BasicTimer instead.
/// This timer can track multiple operations, each started by calling the Record method.
/// This monitor will create two gauges:
/// * A duration which reports the current duration in seconds. The duration is the sum of all active tasks
/// * Number of active tasks
/// The names of the monitors will be the base name passed in the config to the constructor, suffixed by
/// .duration and .activetasks respectively.
type LongTaskTimer(config : MonitorConfig, stopwatchFactory : Func<IStopwatch>) = 
    let nextTaskId = new AtomicLong()
    let tasks = new System.Collections.Concurrent.ConcurrentDictionary<int64, int64>()
    let activeTasks = 
        new BasicGauge<int>({ config with Name = sprintf "%s.activetasks" config.Name }, fun () -> tasks.Count)
    
    let getDurationInSeconds() = 
        let now = DateTime.UtcNow.Ticks
        let durationInTicks = tasks.Values |> Seq.sumBy (fun x -> now - x)
        TimeSpan.FromTicks(durationInTicks).TotalSeconds
    
    let totalDurationInSeconds = 
        new BasicGauge<float>({ config with Name = sprintf "%s.duration" config.Name }, fun () -> getDurationInSeconds())
    let getNextId() = nextTaskId.Increment()
    let markAsStarted id = tasks.TryAdd(id, DateTime.UtcNow.Ticks) |> ignore
    let markAsCompleted id = tasks.TryRemove(id) |> ignore
    
    let record (f : unit -> 'a) = 
        let id = getNextId()
        id |> markAsStarted
        try 
            f()
        finally
            id |> markAsCompleted
    
    new(config) = LongTaskTimer(config)
    
    /// Time a System.Func call and return the value
    member __.Record(f : Func<'T>) = record (fun () -> f.Invoke())
    
    /// Time a System.Action call
    member __.Record(f : Action) = record (fun () -> f.Invoke())
    
    /// Get the number of running tasks
    member __.GetNumberOfActiveTasks() = activeTasks.GetValues() |> Seq.head
    
    /// Get the duration in seconds. Duration is the sum of all active tasks duration.
    member __.GetDurationInSeconds() = totalDurationInSeconds.GetValues() |> Seq.head
    
    /// Get the duration in seconds. Duration is the sum of all active tasks duration.
    member self.GetValues() =
        seq {
            yield Measurement("duration", self.GetDurationInSeconds().Value)
            yield Measurement("activeTasks", self.GetDurationInSeconds().Value)
        }
    
    /// Gets the monitor config
    member __.Config = totalDurationInSeconds.Config
    
    /// Start a manually controlled timinig
    member __.Start() = 
        let id = getNextId()
        id |> markAsStarted
        OkanshiTimer((fun _ -> id |> markAsCompleted), (fun () -> stopwatchFactory.Invoke()))

    /// Gets the value and resets the monitor
    member self.GetValuesAndReset() = self.GetValues()

    /// Manually register a timing, should only be used in special case
    member __.Register(elapsed : int64) : unit = raise (NotSupportedException("LongTaskTimer does not support manually registering timings"))

    interface ITimer with
        member self.Record(f : Func<'T>) = self.Record(f)
        member self.Record(f : Action) = self.Record(f)
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.Start() = self.Start()
        member self.Register(elapsed) = self.Register(elapsed)
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast
