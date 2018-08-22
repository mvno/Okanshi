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

type SystemStopwatch internal() =
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

    /// Manually register a timing, should used when you can't call Record since you at the call time do not know the timer to use
    abstract Register : Stopwatch -> unit

    /// Manually register a timing, should used when you can't call Record since you at the call time do not know the timer to use
    abstract Register : TimeSpan -> unit

/// A simple timer providing the total time, count, min and max for the times that have been recorded
type Timer(config : MonitorConfig, stopwatchFactory : Func<IStopwatch>) as self = 
    
    [<Literal>]
    let StatisticKey = "statistic"
    
    let max = new MaxGauge(config.WithTag(StatisticKey, "max"))
    let min = new MinGauge(config.WithTag(StatisticKey, "min"))
    let count = new Counter(config.WithTag(StatisticKey, "count"))
    let total = new Counter(config.WithTag(StatisticKey, "totalTime"))
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

    new(config: MonitorConfig) = Timer(config, fun () -> SystemStopwatch() :> IStopwatch)
    
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
    member __.Register(elapsed : int64) = elapsed |> updateStatistics

    /// Manually register a timing, should used when you can't call Record since you at the call time do not know the timer to use
    member __.Register(stopwatch : Stopwatch) = self.Register(stopwatch.ElapsedMilliseconds)

    /// Manually register a timing, should used when you can't call Record since you at the call time do not know the timer to use
    member __.Register(elapsed : TimeSpan) = self.Register(int64(elapsed.TotalMilliseconds))

    /// Gets the value and resets the monitor
    member __.GetValuesAndReset() = Lock.lock syncRoot getValuesAndReset'
    
    interface ITimer with
        member self.Record(f : Func<'T>) = self.Record(f)
        member self.Record(f : Action) = self.Record(f)
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.Start() = self.Start()
        member self.Register(elapsed : int64) = self.Register(elapsed)
        member self.Register(elapsed : Stopwatch) = self.Register(elapsed)
        member self.Register(elapsed : TimeSpan) = self.Register(elapsed)
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast
