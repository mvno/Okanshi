namespace Okanshi

open System
open Okanshi.Helpers

/// Registry to keep track for monitors
type IMonitorRegistry =
    inherit IDisposable
    /// Gets the registered monitors
    abstract GetRegisteredMonitors : unit -> IMonitor array
    /// Register a monitor
    abstract Register : IMonitor -> unit
    /// Unregister a monitor
    abstract Unregister : IMonitor -> unit
    /// Check if a monitor is registered
    abstract IsRegistered : IMonitor -> bool

/// Monitor registry used by the OkanshiMonitor. This registry allows registration of monitors
/// with the same name, but different types
type OkanshiMonitorRegistry() =
    let monitors = new System.Collections.Generic.HashSet<IMonitor>()
    let syncRoot = new obj()
    
    let getRegisteredMonitors'() = monitors |> Seq.toArray
    let register' monitor = monitors.Add(monitor) |> ignore
    let unregister' monitor = monitors.Remove(monitor) |> ignore
    let isRegistered' monitor = monitors.Contains(monitor)

    /// Gets the registered monitors
    member __.GetRegisteredMonitors() = Lock.lock syncRoot getRegisteredMonitors'
    /// Register a monitor
    member __.Register(monitor : IMonitor) = Lock.lockWithArg syncRoot monitor register'
    /// Unregister a monitor
    member __.Unregister(monitor : IMonitor) = Lock.lockWithArg syncRoot monitor unregister'
    /// Check if a monitor is registered
    member __.IsRegistered(monitor : IMonitor) = Lock.lockWithArg syncRoot monitor isRegistered'

    interface IMonitorRegistry with
        member self.GetRegisteredMonitors() = self.GetRegisteredMonitors()
        member self.Register(monitor) = self.Register(monitor)
        member self.Unregister(monitor) = self.Unregister(monitor)
        member self.IsRegistered(monitor) = self.IsRegistered(monitor)
        member __.Dispose() = ()

/// The default monitor registry handled as a singleton. Currently this is a OkanshiMonitorRegistry.
[<AbstractClass; Sealed>]
type DefaultMonitorRegistry() =
    static let instance = new OkanshiMonitorRegistry()
    static member Instance = instance
