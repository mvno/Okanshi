namespace Okanshi

open System
open System.Threading.Tasks

/// Observer that can receive updates about metrics
type IMetricObserver = 
    inherit IDisposable


/// Observer that can receive updates about metrics and return its accumulated values to someone else
type IProcessingMetricObserver =
    inherit IMetricObserver

    /// Get the observations observed
    abstract GetObservations : unit -> Metric seq seq


/// Metric observer storing the specified number of observations in memory
type MemoryMetricObserver(poller : IMetricPoller, numberOfSamplesToStore) =
    let observations = new Collections.Concurrent.ConcurrentQueue<_>()
    
    let addObservation x =
        if observations.Count = numberOfSamplesToStore then observations.TryDequeue() |> ignore
        observations.Enqueue(x)

    let observerAction = new Func<Metric seq, Task>(fun metrics -> Task.Run(fun() -> metrics |> addObservation))
    
    do
        poller.RegisterObserver(observerAction)

    new(poller) = new MemoryMetricObserver(poller, 100)
    
    /// Update the observer with the specified metrics
    
    
    /// Get the observations observed
    member __.GetObservations() = observations |> Seq.cache
    
    /// Dispose the observer
    member __.Dispose() = poller.UnregisterObserver(observerAction)
    
    interface IMetricObserver with
        member self.Dispose() = self.Dispose()
    
    interface IProcessingMetricObserver with
        member self.GetObservations() = self.GetObservations()

/// Observer that prints to the Console. An easy way to see that Okanshi is working
/// The serializer function could be e.g. a standard serializer such as newtonsoft Json: 
/// <code>observer = new ConsoleObserver(poller, x => JsonConvert.SerializeObject(x, Formatting.Indented))</code>
type ConsoleObserver(poller : IMetricPoller, serializer : Func<Object, string>) as self = 
    
    do
        poller.RegisterObserver(new Func<Metric seq, Task>(self.Update))
   
    /// Update the observer with the specified metrics
    member __.Update(metrics : Metric seq) = 
        Task.Run(fun() ->
            printf "\n** %s\n" <| DateTime.Now.ToString()
            for metric in metrics do
                printf "%s\n" <| serializer.Invoke(metric)
        )

    /// Dispose the observer
    member __.Dispose() = ()
    
    interface IMetricObserver with
        member self.Update(metrics) = self.Update(metrics)
        member self.Dispose() = self.Dispose()
    
