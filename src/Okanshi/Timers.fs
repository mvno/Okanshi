namespace Okanshi

open System
open System.Diagnostics
open Okanshi.Helpers

type IStopwatch =
    abstract Elapsed : TimeSpan
    abstract IsRunning : bool
    abstract Start : unit -> unit
    abstract Stop : unit -> unit
    abstract Time : Func<'T> -> ('T * TimeSpan)
    abstract Time : Action -> TimeSpan

type SystemStopwatch internal() =
    let stopwatch = new Stopwatch()

    let time' f =
        stopwatch.Start()
        let result = f()
        stopwatch.Stop()
        (result, stopwatch.Elapsed)

    member __.Elapsed = stopwatch.Elapsed

    member __.IsRunning = stopwatch.IsRunning

    member __.Start() = stopwatch.Start()
    
    member __.Stop() = stopwatch.Stop()
    
    member __.Time(f: Func<'T>) = time' (fun () -> f.Invoke())

    member __.Time(f: Action) =
        let (_, elapsed) = time' (fun () -> f.Invoke())
        elapsed

    interface IStopwatch with
        member self.Elapsed = self.Elapsed
        member self.IsRunning = self.IsRunning
        member self.Start() = self.Start()
        member self.Stop() = self.Stop()
        member self.Time(f: Func<'T>) : ('T * TimeSpan) = self.Time(f)
        member self.Time(f: Action) = self.Time(f)

/// Timer that is started and stopped manually
type OkanshiTimer(onStop : Action<TimeSpan>, stopwatchFactory: Func<IStopwatch>) = 
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
        onStop.Invoke(stopwatch.Value.Elapsed)
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

    /// Manually register a timing, should used when you can't call Record since you at the call time do not know the timer to use.
    ///
    /// You should stop the stopwatch before passing so you do not incur the overhead of Okanshi. But it is not a requirement. 
    /// The stopwatch is not stopped by Okanshi.
    abstract RegisterElapsed : Stopwatch -> unit

    /// Manually register a timing, should used when you can't call Record since you at the call time do not know the timer to use
    abstract Register : TimeSpan -> unit

/// A timer providing the "total time", "count", "min" and "max" for the recordings
type Timer(config : MonitorConfig, stopwatchFactory : Func<IStopwatch>) as self = 
    let max = new MaxGauge(config)
    let min = new MinGauge(config)
    let count = new Counter(config)
    let total = new Counter(config)
    let avg = new AverageGauge(config)
    let syncRoot = new obj()

    let updateStatistics' (elapsed : TimeSpan) =
        let elapsedMillis = int64(elapsed.TotalMilliseconds)
        count.Increment() |> ignore
        total.Increment(elapsedMillis)
        max.Set(elapsedMillis)
        min.Set(elapsedMillis)
        avg.Set(float elapsedMillis)
    
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
    member __.Config = config
    
    /// Start a manually controlled timinig
    member __.Start() =
        OkanshiTimer((fun x -> updateStatistics x), (fun () -> stopwatchFactory.Invoke()))

    /// Manually register a timing, should used when you can't call Record since you at the call time do not know the timer to use.
    ///
    /// You should stop the stopwatch before passing so you do not incur the overhead of Okanshi. But it is not a requirement. 
    /// The stopwatch is not stopped by Okanshi.
    member __.RegisterElapsed(stopwatch : Stopwatch) = updateStatistics stopwatch.Elapsed 

    /// Manually register a timing, should used when you can't call Record since you at the call time do not know the timer to use
    member __.Register(elapsed : TimeSpan) = updateStatistics elapsed

    /// Gets the value and resets the monitor
    member __.GetValuesAndReset() = Lock.lock syncRoot getValuesAndReset'
    
    interface ITimer with
        member self.Record(f : Func<'T>) = self.Record(f)
        member self.Record(f : Action) = self.Record(f)
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.Start() = self.Start()
        member self.RegisterElapsed(elapsed : Stopwatch) = self.RegisterElapsed(elapsed)
        member self.Register(elapsed : TimeSpan) = self.Register(elapsed)
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast

/// Apdex (Application Performance Index) is an open standard developed by an alliance of companies that defines a standardized method to report, benchmark, and track application performance.
/// Apdex operates with three thresholds estimating end user satisfaction: satisfied, tolerating and frustrating.
/// 
/// Satisfied: Response time less than or equal to T seconds.
/// Tolerating: Response time between T seconds and 4T seconds.
/// Frustrating: Response time greater than 4 T seconds.
/// 
/// The Apdex score between 0 and 1 is calculated using the following:
/// 
/// ( Satisfied + (Tolerating/2) ) / Total number of requests
///
/// see http://www.apdex.org/overview.html 
type ApdexTimer(config : MonitorConfig, stopwatchFactory : Func<IStopwatch>, toleratableThreshold: TimeSpan) as self = 
    [<Literal>]
    let StatisticKey = "statistic"
    
    let timer = new Timer(config.WithTag(StatisticKey, ""), stopwatchFactory)
    let mutable satisfiedTimingCount = 0
    let mutable tolerableTimingCount = 0
    let syncRoot = new obj()

    let updateStatistics' (elapsed : TimeSpan) =
        let threshold = toleratableThreshold.TotalMilliseconds
        let elapsedMillis = elapsed.TotalMilliseconds
        satisfiedTimingCount <- if elapsedMillis < threshold 
                                    then satisfiedTimingCount+1 
                                    else satisfiedTimingCount
        tolerableTimingCount <- if elapsedMillis >= threshold && elapsedMillis < threshold * 4.0
                                    then tolerableTimingCount+1 
                                    else tolerableTimingCount
        elapsed |> timer.Register 
    
    let updateStatistics elapsed = 
        lockWithArg syncRoot elapsed updateStatistics'
    
    let calcApdex'() : float =  
        match timer.GetCount().Value with
        | 0L -> 0.0
        | _ -> 
            let index = (float(satisfiedTimingCount) + (float(tolerableTimingCount)/2.0)) / float(timer.GetCount().Value)
            let rounded = System.Math.Round(index, 2, MidpointRounding.AwayFromZero)
            rounded

    let getValues' () =
        seq {
            yield! timer.GetValues() 
            yield! [| new Measurement<float>("apdex", calcApdex'()) :> IMeasurement |]
        }

    let reset'() =
        timer.GetValuesAndReset() |> ignore
        satisfiedTimingCount <- 0
        tolerableTimingCount <- 0

    let getValuesAndReset'() =
        let result = self.GetValues() |> Seq.toList
        reset'()
        result |> List.toSeq

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
    member __.GetCount() = lock syncRoot (fun() -> timer.GetCount())
    
    /// Gets the average calls time within the specified step
    member __.GetValues() = lock syncRoot getValues'
    
    /// Get the maximum value of all calls
    member __.GetMax() = lock syncRoot (fun () -> timer.GetMax())
    
    /// Get the manimum value of all calls
    member __.GetMin() = lock syncRoot (fun () -> timer.GetMin())
    
    /// Gets the the total time for all calls within the specified step
    member __.GetTotalTime() = lock syncRoot (fun () -> timer.GetTotalTime())
    
    /// Calculate the apdex score
    member __.GetApDex() = lock syncRoot calcApdex'

    /// Gets the monitor config
    member __.Config = config
    
    /// Start a manually controlled timinig
    member __.Start() =
        OkanshiTimer((fun x -> updateStatistics x), (fun () -> stopwatchFactory.Invoke()))

    /// Manually register a timing, should used when you can't call Record since you at the call time do not know the timer to use.
    ///
    /// You should stop the stopwatch before passing so you do not incur the overhead of Okanshi. But it is not a requirement. 
    /// The stopwatch is not stopped by Okanshi.
    member __.RegisterElapsed(stopwatch : Stopwatch) = updateStatistics stopwatch.Elapsed

    /// Manually register a timing, should used when you can't call Record since you at the call time do not know the timer to use
    member __.Register(elapsed : TimeSpan) =  updateStatistics elapsed

    /// Gets the value and resets the monitor
    member __.GetValuesAndReset() = Lock.lock syncRoot getValuesAndReset'

    interface ITimer with
        member self.Record(f : Func<'T>) = self.Record(f)
        member self.Record(f : Action) = self.Record(f)
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.Start() = self.Start()
        member self.RegisterElapsed(elapsed : Stopwatch) = self.RegisterElapsed(elapsed)
        member self.Register(elapsed : TimeSpan) = self.Register(elapsed)
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast