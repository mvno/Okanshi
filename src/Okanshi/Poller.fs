namespace Okanshi

open System
open System.Threading
open System.Threading.Tasks

/// The metric type
type Metric =
    {
        /// Name of the metric
        Name : string;
        /// The timestamp where the metric was observed
        Timestamp : DateTimeOffset;
        /// The metric tags
        Tags : Tag array;
        /// The value
        Value : obj
        /// The sub metrics
        SubMetrics : Metric array
    }

/// A poller that can be used to fetch the current values for a list of metrics
type IMetricPoller =
    inherit IDisposable
    /// Stop polling for new metrics
    abstract Stop : unit -> unit
    /// Force a poll of metrics
    abstract PollMetrics : unit -> Task
    /// Register an observer
    abstract RegisterObserver : Func<Metric seq, Task> -> unit
    /// Unregister an observer
    abstract UnregisterObserver : Func<Metric seq, Task> -> unit

/// Poller for fetching metrics from a monitor registry
type MetricMonitorRegistryPoller(registry : IMonitorRegistry, interval : TimeSpan, pollOnExit : bool) as self =
    let cancellationTokenSource = new CancellationTokenSource()
    let cancellationToken = cancellationTokenSource.Token
    let observers = new Collections.Generic.List<Func<Metric seq, Task>>()

    let rec convertMonitorToMetric (monitor : IMonitor) =
        let submetrics =
            monitor.GetAllMonitors()
            |> Seq.except (seq { yield monitor })
            |> Seq.map convertMonitorToMetric
            |> Seq.toArray
        {
            Name = monitor.Config.Name
            Timestamp = DateTimeOffset.UtcNow
            Tags = monitor.Config.Tags
            Value = monitor.GetValueAndReset()
            SubMetrics = submetrics
        }

    let pollMetrics () =
        let metrics =
            registry.GetRegisteredMonitors()
            |> Seq.map convertMonitorToMetric
            |> Seq.toArray
        observers
        |> Seq.map (fun x -> x.Invoke(metrics) |> Async.AwaitTask)
        |> Async.Parallel

    let onExitSubscriber =
        if pollOnExit then
            let currentDomain = AppDomain.CurrentDomain
            let pollMetrics = fun _ ->
                Logger.Debug.Invoke("Polling metrics because of process exit or appdomain unload")
                pollMetrics() |> Async.RunSynchronously |> ignore
            if currentDomain.IsDefaultAppDomain() then
                currentDomain.ProcessExit.Subscribe(pollMetrics)
            else
                currentDomain.DomainUnload.Subscribe(pollMetrics)
        else { new IDisposable with member __.Dispose() = () }

    let rec poll () =
        async {
            do! Async.Sleep(int interval.TotalMilliseconds)
            Logger.Debug.Invoke("Polling metrics...")
            pollMetrics() |> Async.RunSynchronously |> ignore
            return! poll()
        }

    do
        Async.Start(poll(), cancellationToken)

    new (registry) = new MetricMonitorRegistryPoller(registry, TimeSpan.FromMinutes(float 1))
    new (registry, interval) = new MetricMonitorRegistryPoller(registry, interval, false)

    /// Force poll the monitors from the registry
    member __.PollMetrics() = Task.Run(fun () -> pollMetrics() |> Async.RunSynchronously |> ignore)
    
    /// Stop polling for new metrics
    member __.Stop() =
        cancellationTokenSource.Cancel()
        onExitSubscriber.Dispose()

    /// Disposes the poller, stopping metrics collection
    member self.Dispose() = self.Stop()

    /// Register an observer
    member __.RegisterObserver(observer: Func<Metric seq, Task>) = observers.Add(observer)

    /// Unregister an observer
    member __.UnregisterObserver(observer: Func<Metric seq, Task>) = observers.Remove(observer) |> ignore

    interface IMetricPoller with
        member self.Stop() = self.Stop()
        member self.Dispose() = self.Dispose()
        member self.PollMetrics() = self.PollMetrics()
        member self.RegisterObserver(x) = self.RegisterObserver(x)
        member self.UnregisterObserver(x) = self.UnregisterObserver(x)
