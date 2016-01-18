namespace Okanshi

open System
open System.Runtime.Caching

/// Helper extensions for the System.Runtime.Caching.MemoryCache
[<AutoOpen>]
module MemoryCacheExtensions =
    type MemoryCache with
        /// Return an option instead for null if the cache item doesn't exists
        member cache.TryGet<'T>(key) =
            match cache.Get(key) with
            | null -> None
            | x -> Some (x :?> 'T)

/// Observer that can receive updates about metrics
type IMetricObserver =
    inherit IDisposable
    /// Update the observer with the specified metrics
    abstract Update : Metric array -> unit
    /// Get the observations observed
    abstract GetObservations : unit -> Metric array array

/// Metric observer storing the specified number of observations in memory
type MemoryMetricObserver(poller : IMetricPoller, numberToStore) as self =
    let observations = new System.Collections.Concurrent.ConcurrentQueue<_>()
    let addObservation x =
        if observations.Count = numberToStore then observations.TryDequeue() |> ignore
        observations.Enqueue(x)
    let innerPoller = poller.MetricsPolled.Subscribe(fun x -> self.Update(x.Metrics))
    
    new (poller) = new MemoryMetricObserver(poller, 100)

    /// Update the observer with the specified metrics
    member __.Update(metrics : Metric array) = metrics |> addObservation
    /// Get the observations observed
    member __.GetObservations() = observations |> Seq.toArray
    /// Dispose the observer
    member __.Dispose() = innerPoller.Dispose()

    interface IMetricObserver with
        member self.Update(metrics) = self.Update(metrics)
        member self.GetObservations() = self.GetObservations()
        member self.Dispose() = self.Dispose()

/// Convert counter metrics to rate per second. The rate is calculated by comparing two samples of the given metric
/// and looking at the delta. This means that two samples are required to compute the rate, and that no value will be
/// sent to the wrapped observer until a second sample arrives. If the metric is not updated within the heartbeat interval
/// the previous cached value will be dropped, and new sample coming in will be treated as the first sample for that metric.
type CounterToRateMetricTransformObserver(observer : IMetricObserver, heartbeat : TimeSpan) =
    let cache = MemoryCache.Default

    let (|Counter|_|) (metric : Metric) =
        match metric.Tags |> Seq.tryFind (fun x -> x = DataSourceType.Counter) with
        | Some _ -> Some metric
        | None -> None

    let toRateMetric metric =
        let tags = DataSourceType.Rate :: (metric.Tags |> Seq.filter (fun x -> x <> DataSourceType.Counter) |> Seq.toList) |> Seq.toArray
        { Name = metric.Name; Tags = tags; Timestamp = metric.Timestamp; Value = metric.Value }

    let computeRate previous current : double =
        let currentValue = current.Value :?> double
        let previousValue = previous.Value :?> double
        let millisecondsPerSecond = 1000.0
        let duration = (current.Timestamp - previous.Timestamp).TotalMilliseconds / millisecondsPerSecond
        let delta = currentValue - previousValue
        if duration <= 0.0 || delta <= 0.0 then 0.0 else delta / duration

    let convert metric =
        match metric with
        | Counter currentMetric ->
            let metric = currentMetric |> toRateMetric
            let previousMetric = cache.TryGet(metric.Name)
            match previousMetric with
            | Some x ->
                let rate = metric |> computeRate x
                Some { metric with Value = rate }
            | None ->
                cache.Add(metric.Name, metric, DateTimeOffset.UtcNow.Add(heartbeat)) |> ignore
                None
        | _ -> Some metric

    let rec convertMetrics list metrics =
        match metrics with
        | head :: tail ->
            let metric = head |> convert
            match metric with
            | Some x -> tail |> convertMetrics (x :: list)
            | None -> tail |> convertMetrics list
        | [] -> list

    /// Update the stored metrics
    member __.Update(metrics : Metric array) =
        let newMetrics = metrics |> Seq.toList |> convertMetrics [] |> Seq.toArray
        observer.Update(newMetrics)

    /// Get the stored observations. As this observer transforms the metrics and forwards them to another observer,
    /// this will always return an empty array
    member __.GetObservations() = [||]

    interface IMetricObserver with
        member self.Update(metrics) = self.Update(metrics)
        member self.GetObservations() = self.GetObservations()
        member __.Dispose() = ()
