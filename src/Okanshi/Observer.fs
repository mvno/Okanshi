namespace Okanshi

open System
open System.Threading.Tasks

/// Observer that can receive updates about metrics
type IMetricObserver = 
    inherit IDisposable
    
    /// Update the observer with the specified metrics
    abstract Update : Metric seq -> Task
    
    /// Get the observations observed
    abstract GetObservations : unit -> Metric seq seq

/// Metric observer storing the specified number of observations in memory
type MemoryMetricObserver(poller : IMetricPoller, numberOfSamplesToStore) as self = 
    let observations = new Collections.Concurrent.ConcurrentQueue<_>()
    
    let addObservation x = 
        if observations.Count = numberOfSamplesToStore then observations.TryDequeue() |> ignore
        observations.Enqueue(x)

    let observerAction = new Func<Metric seq, Task>(self.Update)
    
    do
        poller.RegisterObserver(observerAction)

    new(poller) = new MemoryMetricObserver(poller, 100)
    
    /// Update the observer with the specified metrics
    member __.Update(metrics : Metric seq) = Task.Run(fun() -> metrics |> addObservation)
    
    /// Get the observations observed
    member __.GetObservations() = observations |> Seq.cache
    
    /// Dispose the observer
    member __.Dispose() = poller.UnregisterObserver(observerAction)
    
    interface IMetricObserver with
        member self.Update(metrics) = self.Update(metrics)
        member self.GetObservations() = self.GetObservations()
        member self.Dispose() = self.Dispose()
