namespace Okanshi

open System
open Okanshi.Helpers

/// Registry to keep track for monitors
type IMonitorRegistry = 
    inherit IDisposable
    
    /// Gets the registered monitors
    abstract GetRegisteredMonitors : unit -> IMonitor array
    
    /// Unregister a monitor
    abstract Unregister : IMonitor -> unit
    
    /// Check if a monitor is registered
    abstract IsRegistered : IMonitor -> bool
    
    /// Clear the registry
    abstract Clear : unit -> unit
    
    /// Get or add a new registration
    abstract GetOrAdd<'a when 'a :> IMonitor> : MonitorConfig * Func<MonitorConfig, 'a> -> 'a

/// Monitor registry used by the OkanshiMonitor. This registry allows registration of monitors
/// with the same name, but different types
type OkanshiMonitorRegistry() = 
    let monitors = new System.Collections.Generic.Dictionary<int, IMonitor>()
    let syncRoot = new obj()
    
    let hash (config : MonitorConfig) (t : Type) = 
        let mutable hash = 2166136261L |> int
        hash <- (hash * 16777619) ^^^ config.GetHashCode()
        (hash * 16777619) ^^^ t.FullName.GetHashCode()
    
    let getRegisteredMonitors'() = monitors.Values |> Seq.toArray
    
    let unregister' (monitor : IMonitor) = 
        let hash = hash monitor.Config (monitor.GetType())
        monitors.Remove(hash) |> ignore
    
    let isRegistered' (monitor : IMonitor) = 
        let hash = hash monitor.Config (monitor.GetType())
        monitors.ContainsKey(hash)
    
    let clear'() = monitors.Clear()
    
    let getOrAdd' (config : MonitorConfig, factory : Func<MonitorConfig, 'a>) : 'a = 
        let hash = hash config typeof<'a>
        match monitors.TryGetValue(hash) with
        | true, x -> x :?> 'a
        | _ ->
            let monitor = factory.Invoke(config)
            monitors.Add(hash, monitor)
            monitor
    
    /// Gets the registered monitors
    member __.GetRegisteredMonitors() = Lock.lock syncRoot getRegisteredMonitors'
    
    /// Unregister a monitor
    member __.Unregister(monitor : IMonitor) = Lock.lockWithArg syncRoot monitor unregister'
    
    /// Check if a monitor is registered
    member __.IsRegistered(monitor : IMonitor) = Lock.lockWithArg syncRoot monitor isRegistered'
    
    /// Clear the registry
    member __.Clear() = Lock.lock syncRoot clear'
    
    /// Get or add a new registration
    member __.GetOrAdd<'a when 'a :> IMonitor>(config, factory : Func<MonitorConfig, 'a>) =
        Lock.lockWithArg syncRoot (config, factory) getOrAdd'
    
    interface IMonitorRegistry with
        member self.GetRegisteredMonitors() = self.GetRegisteredMonitors()
        member self.Unregister(monitor) = self.Unregister(monitor)
        member self.IsRegistered(monitor) = self.IsRegistered(monitor)
        member self.Clear() = self.Clear()
        member self.GetOrAdd<'a when 'a :> IMonitor>(config, factory : Func<MonitorConfig, 'a>) = self.GetOrAdd(config, factory)
        member __.Dispose() = ()

/// This registry holds monitors only for a limited amount of pulls
/// This is very useful for general monitoring where you NEVER store any monitors as local references
/// and where you potentially instantiate a lot of monitors
/// For example, in the OWIN or WebAPI integrations we may use the URL to the endpoint as part of the monitor
/// thus for each argument passed to the endpoint we may create a new monitor - effectively creating a memory bottleneck
/// and potentially require Okanshi to search through millions of monitors with each pull
type EvictingOkanshiMonitorRegistry(numberOfPullsBeforeEviction : int) =
    inherit OkanshiMonitorRegistry()
    let mutable iterationsLeft = numberOfPullsBeforeEviction
    let monitors = new System.Collections.Generic.Dictionary<int, IMonitor>()
    let syncRoot = new obj()

    do
        if numberOfPullsBeforeEviction <= 0 then 
            raise (ArgumentOutOfRangeException("numberOfPullsBeforeEviction", "Must be greater than 0"))
    
    let hash (config : MonitorConfig) (t : Type) = 
        let mutable hash = 2166136261L |> int
        hash <- (hash * 16777619) ^^^ config.GetHashCode()
        (hash * 16777619) ^^^ t.FullName.GetHashCode()
    
    let getRegisteredMonitors'() = 
        let result = monitors.Values |> Seq.toArray
        iterationsLeft <- iterationsLeft - 1
        match iterationsLeft with
        | 0 -> 
            iterationsLeft <- numberOfPullsBeforeEviction
            for m in monitors do 
                Logger.Debug.Invoke (sprintf "Evicting '%s', it will no longer send any data." m.Value.Config.Name)
            monitors.Clear()
        | _ -> ()
        
        result
    
    let unregister' (monitor : IMonitor) = 
        let hash = hash monitor.Config (monitor.GetType())
        monitors.Remove(hash) |> ignore
    
    let isRegistered' (monitor : IMonitor) = 
        let hash = hash monitor.Config (monitor.GetType())
        monitors.ContainsKey(hash)
    
    let clear'() = monitors.Clear()
    
    let getOrAdd' (config : MonitorConfig, factory : Func<MonitorConfig, 'a>) : 'a = 
        let hash = hash config typeof<'a>
        match monitors.TryGetValue(hash) with
        | true, x -> x :?> 'a
        | _ ->
            let monitor = factory.Invoke(config)
            monitors.Add(hash, monitor)
            monitor
    
    /// Gets the registered monitors
    member __.GetRegisteredMonitors() = Lock.lock syncRoot getRegisteredMonitors'
    
    /// Unregister a monitor
    member __.Unregister(monitor : IMonitor) = Lock.lockWithArg syncRoot monitor unregister'
    
    /// Check if a monitor is registered
    member __.IsRegistered(monitor : IMonitor) = Lock.lockWithArg syncRoot monitor isRegistered'
    
    /// Clear the registry
    member __.Clear() = Lock.lock syncRoot clear'
    
    /// Get or add a new registration
    member __.GetOrAdd<'a when 'a :> IMonitor>(config, factory : Func<MonitorConfig, 'a>) =
        Lock.lockWithArg syncRoot (config, factory) getOrAdd'
    
    interface IMonitorRegistry with
        member self.GetRegisteredMonitors() = self.GetRegisteredMonitors()
        member self.Unregister(monitor) = self.Unregister(monitor)
        member self.IsRegistered(monitor) = self.IsRegistered(monitor)
        member self.Clear() = self.Clear()
        member self.GetOrAdd<'a when 'a :> IMonitor>(config, factory : Func<MonitorConfig, 'a>) = self.GetOrAdd(config, factory)
        member __.Dispose() = ()

/// The default monitor registry handled as a singleton. Currently this is a OkanshiMonitorRegistry.
[<AbstractClass; Sealed>]
type DefaultMonitorRegistry() = 
    static let instance = new OkanshiMonitorRegistry()
    static member Instance = instance
