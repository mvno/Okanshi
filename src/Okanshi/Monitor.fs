namespace Okanshi

open System
open Metric

/// Dependecy information
type Dependency =
    {
        /// Name of the dependency
        name : string;
        /// The dependency version
        version : string;
    }

/// Monitor messages
type MonitorMessage =
    /// Increment the success counter
    | IncrementSuccess of string
    /// Increment the failed counter
    | IncrementFailed of string
    /// Add timinig information
    | Time of string * int64
    /// Reset counters
    | ResetCounters
    /// Stop monitoring
    | Stop of AsyncReplyChannel<unit>

/// Used to communicate with thrird-party systems when metric is updated
type MetricUpdated =
    {
        /// The added message
        Added : MonitorMessage;
        /// The updated metric
        Metric : Metric;
        /// The update timestamp
        Timestamp : DateTimeOffset;
    }

/// Monitor options
type MonitorOptions() =
    
    /// Gets or sets the maximum number of measurement windows to keep in memory.
    ///
    /// Default value is 100.
    member val MaxNumberOfMeasurements = 100 with get, set

    /// Gets or sets the windows size in milliseconds.
    ///
    /// Default value is 1 minute.
    member val WindowSize = float 100000 with get, set

    /// Gets or sets an action called when metrics are updated.
    ///
    /// This allows you to send metrics to third party systems in near realtime.
    member val OnMetricUpdated = Action<MetricUpdated> (fun _ -> ()) with get, set


/// The monitor
module Monitor =
    open System.Collections.Concurrent
    open System.Diagnostics

    let defaultOptions = new MonitorOptions()
    let private hashSet = new ConcurrentDictionary<string, Metric>()
    let mutable private isStarted = false

    /// [omit]
    type Monitor = MailboxProcessor<MonitorMessage>

    /// Start monitoring with specified options
    let start (options : MonitorOptions) : Monitor =
        if isStarted then invalidOp "Monitor is already started"
        isStarted <- true
        MailboxProcessor.Start(fun inbox ->
            let getOrCreateMetricIfMissing (dictionary : ConcurrentDictionary<string, Metric>) key =
                if dictionary.ContainsKey(key) then
                    dictionary.[key]
                else
                    let metric = options.WindowSize |> Metric.createEmpty options.MaxNumberOfMeasurements
                    dictionary.[key] <- metric
                    metric

            let incrementSuccessCounter dictionary name =
                let metric = name |> getOrCreateMetricIfMissing dictionary
                let newMetric = metric |> addSuccess
                async {
                    { Added = IncrementSuccess name; Metric = newMetric; Timestamp = DateTimeOffset.Now }
                    |> options.OnMetricUpdated.Invoke
                } |> Async.Start
                dictionary.[name] <- newMetric

            let incrementFailedCounter dictionary name =
                let metric = name |> getOrCreateMetricIfMissing dictionary
                let newMetric = metric |> addFailed
                async {
                    { Added = IncrementFailed name; Metric = newMetric; Timestamp = DateTimeOffset.Now }
                    |> options.OnMetricUpdated.Invoke
                } |> Async.Start
                dictionary.[name] <- newMetric

            let recordTiming dictionary (name, milliseconds) =
                let metric = name |> getOrCreateMetricIfMissing dictionary
                let newMetric = metric |> addTiming milliseconds
                async {
                    { Added = Time (name, milliseconds); Metric = newMetric; Timestamp = DateTimeOffset.Now }
                    |> options.OnMetricUpdated.Invoke
                } |> Async.Start
                dictionary.[name] <- newMetric

            let rec loop () =
                async {
                    let! msg = inbox.Receive()
                    match msg with
                        | IncrementSuccess name -> name |> incrementSuccessCounter hashSet
                        | IncrementFailed name -> name |> incrementFailedCounter hashSet
                        | Time (name, milliseconds) -> (name, milliseconds) |> recordTiming hashSet
                        | ResetCounters -> hashSet.Clear() |> ignore
                        | Stop reply -> reply.Reply(); return ()

                    do! loop()
                }
            loop ())

    /// Stop monitoring
    let stop (monitor : Monitor) =
        monitor.PostAndReply(Stop)
        hashSet.Clear()
        isStarted <- false

    /// Get dependencies
    let getDependencies() =
        List.ofSeq([for dep in AppDomain.CurrentDomain.GetAssemblies() -> { new Dependency with name = dep.GetName().Name and version = dep.GetName().Version.ToString() }])
        |> List.toArray

    /// Get metrics
    let getMetrics() = hashSet |> Seq.map (|KeyValue|) |> dict

    /// Run health checks
    let runHealthChecks() = HealthChecks.RunAll()

    /// Increment the success counter for the provided key
    let success name (monitor : Monitor) = monitor.Post(IncrementSuccess name)

    /// Increment the failed counter for the provided key
    let failed name (monitor : Monitor) = monitor.Post(IncrementFailed name)

    /// Time a function call, identified by the provided key.
    ///
    /// If the function succeeds, the success counter is incremented and the timinig information saved.
    ///
    /// If the function throws an exception, the failure counter is incremented and the exception is rethrown.
    let time name f (monitor : Monitor) =
        let stopWatch = new Stopwatch()
        stopWatch.Start()
        try
            let result = f()
            stopWatch.Stop()
            monitor.Post(IncrementSuccess name)
            monitor.Post(Time (name, stopWatch.ElapsedMilliseconds))
            result
        with
            | _ ->
                stopWatch.Stop()
                monitor.Post(IncrementFailed name)
                monitor.Post(Time (name, stopWatch.ElapsedMilliseconds))
                reraise()

    /// Reset all counters
    let resetCounters (monitor : Monitor) = monitor.Post(ResetCounters)

namespace Okanshi.CSharp
    open Okanshi
    open System

    /// The monitor
    type Monitor private() =
        static let mutable instance : Monitor.Monitor option = None
        static let isStarted() = if instance.IsNone then false else true
        
        /// Start monitoring with default options
        static member Start() =
            instance <- Some <| Monitor.start Monitor.defaultOptions
        
        /// Start monitoring with specified options
        static member Start(options) =
            instance <- Some <| Monitor.start options
        
        /// Stop monitoring
        static member Stop() =
            if isStarted() then
                Monitor.stop instance.Value
                instance <- None

        /// Increment the success counter for the provided key
        static member Success(name) =
            if isStarted() then
                Monitor.success name instance.Value

        /// Increment the failure counter for the provided key
        static member Failed(name) =
            if isStarted() then
                Monitor.failed name instance.Value
        
        /// Time a function call, identified by the provided key.
        ///
        /// If the function succeeds, the success counter is incremented and the timinig information saved.
        ///
        /// If the function throws an exception, the failure counter is incremented and the exception is rethrown.
        static member Time(name, func : Func<'T>) =
            if isStarted() then
                Monitor.time name func.Invoke instance.Value
            else
                func.Invoke()

        /// Time a function call, identified by the provided key.
        ///
        /// If the function succeeds, the success counter is incremented and the timinig information saved.
        ///
        /// If the function throws an exception, the failure counter is incremented and the exception is rethrown.
        static member Time(name, action : Action) =
            if isStarted() then
                Monitor.time name action.Invoke instance.Value
            else
                action.Invoke()
        
        /// Reset all counters
        static member ResetCounters() =
            if isStarted() then
                Monitor.resetCounters instance.Value

        /// Run health checks
        static member RunHealthChecks() =
            Monitor.runHealthChecks()

        /// Get dependencies
        static member GetDependencies() =
            Monitor.getDependencies()

        /// Get metrics
        static member GetMetrics() =
            Monitor.getMetrics()

    
    /// [omit]
    /// MonitorMessage extensions for better C# interoperability
    [<System.Runtime.CompilerServices.Extension>]
    module MonitorMessageExtensions =
        /// [omit]
        /// Get value of IncrementSuccess
        [<System.Runtime.CompilerServices.Extension>]
        let GetIncrementSuccess(message) =
            match message with
            | IncrementSuccess x -> x
            | _ -> failwith "Not an increment success"

        /// [omit]
        /// Get value of IncrementFailed
        [<System.Runtime.CompilerServices.Extension>]
        let GetIncrementFailed(message) =
            match message with
            | IncrementFailed x -> x
            | _ -> failwith "Not an increment failed"

        /// [omit]
        /// Get value of Time
        [<System.Runtime.CompilerServices.Extension>]
        let GetTime(message) =
            match message with
            | Time (x, y) -> (x, y)
            | _ -> failwith "Not an time message"
