namespace Okanshi

open System

/// Observer that can receive updates about metrics
type IMetricObserver = 
    inherit IDisposable
    
    /// Update the observer with the specified metrics
    abstract Update : Metric array -> unit
    
    /// Get the observations observed
    abstract GetObservations : unit -> Metric array array

/// Metric observer storing the specified number of observations in memory
type MemoryMetricObserver(poller : IMetricPoller, numberOfSamplesToStore) as self = 
    let observations = new System.Collections.Concurrent.ConcurrentQueue<_>()
    
    let addObservation x = 
        if observations.Count = numberOfSamplesToStore then observations.TryDequeue() |> ignore
        observations.Enqueue(x)
    
    let innerPoller = poller.MetricsPolled.Subscribe(fun x -> self.Update(x.Metrics))
    new(poller) = new MemoryMetricObserver(poller, 100)
    
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
