﻿namespace Okanshi

open System
open System.Threading
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

/// The default monitor registry handled as a singleton. Currently this is a OkanshiMonitorRegistry.
[<AbstractClass; Sealed>]
type DefaultMonitorRegistry() = 
    static let instance = new OkanshiMonitorRegistry()
    static member Instance = instance

type WeakMonitorReference(monitor: IMonitor) =
    let reference = new WeakReference(monitor, false)

    member __.GetTarget() : Option<IMonitor> =
        match reference.Target with
        | :? IMonitor as monitor -> Some(monitor)
        | _ -> None

/// Registry that will hold a weak reference to monitors, allowing them to be garbage collected.
/// When a monitor is garbage collected, it will be removed from the internal collection when the cleanup thread runs.
/// This registry allows registration of monitors with the same name, but different types.
type EvictingRegistry(cleanupInterval: TimeSpan) =
    let monitors = new System.Collections.Generic.Dictionary<int, WeakMonitorReference>()
    let syncRoot = new obj()
    let cleanupCancelSource = new CancellationTokenSource()

    let cleanup() =
        let expiredMonitors =
            monitors
            |> Seq.filter (fun kvp -> kvp.Value.GetTarget() |> Option.isNone)
            |> Seq.map (fun kvp -> kvp.Key)
            |> Seq.toList
        expiredMonitors
        |> Seq.iter (fun x -> monitors.Remove(x) |> ignore)

    let rec cleanupAsync() = async {
        do! Async.Sleep (int cleanupInterval.TotalMilliseconds)
        Lock.lock syncRoot cleanup
        do! cleanupAsync()
    }

    let hash (config : MonitorConfig) (t : Type) =
        let mutable hash = 2166136261L |> int
        hash <- (hash * 16777619) ^^^ config.GetHashCode()
        (hash * 16777619) ^^^ t.FullName.GetHashCode()

    let getRegisteredMonitors'() =
        monitors.Values
        |> Seq.map (fun x -> x.GetTarget())
        |> Seq.filter (fun x -> x.IsSome)
        |> Seq.map (fun x -> x.Value)
        |> Seq.toArray

    let unregister' (monitor : IMonitor) =
        let hash = hash monitor.Config (monitor.GetType())
        monitors.Remove(hash) |> ignore

    let isRegistered' (monitor : IMonitor) =
        let hash = hash monitor.Config (monitor.GetType())
        monitors.ContainsKey(hash)

    let clear'() = monitors.Clear()

    let add' (hash: int) (config : MonitorConfig) (factory : Func<MonitorConfig, 'a>) : 'a =
        let monitor = factory.Invoke(config)
        monitors.Add(hash, new WeakMonitorReference(monitor :> IMonitor))
        monitor

    let getOrAdd' (config : MonitorConfig, factory : Func<MonitorConfig, 'a>) : 'a =
        let hash = hash config typeof<'a>
        match monitors.TryGetValue(hash) with
        | true, reference ->
            match reference.GetTarget() with
            | Some(monitor) -> monitor :?> 'a
            | _ ->
                monitors.Remove(hash) |> ignore
                add' hash config factory
        | _ -> add' hash config factory

    do
        Async.Start(cleanupAsync(), cleanupCancelSource.Token)

    new () = new EvictingRegistry(TimeSpan.FromMinutes(1.0))

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

    /// Get all registered monitors, even monitors garbage collector. Should only be used for debugging
    member __.GetAllRegisteredMonitors() = Lock.lock syncRoot (fun () -> monitors.Values)

    interface IMonitorRegistry with
        member self.GetRegisteredMonitors() = self.GetRegisteredMonitors()
        member self.Unregister(monitor) = self.Unregister(monitor)
        member self.IsRegistered(monitor) = self.IsRegistered(monitor)
        member self.Clear() = self.Clear()
        member self.GetOrAdd<'a when 'a :> IMonitor>(config, factory : Func<MonitorConfig, 'a>) = self.GetOrAdd(config, factory)
        member __.Dispose() =
            if not cleanupCancelSource.IsCancellationRequested then
                Lock.lock syncRoot (fun () ->
                    if not cleanupCancelSource.IsCancellationRequested then
                        cleanupCancelSource.Cancel()
                )