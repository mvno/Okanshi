namespace Okanshi

open System
open System.Collections.Concurrent

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
    let monitors = new ConcurrentDictionary<IMonitor, byte>()
    
    /// Gets the registered monitors
    member __.GetRegisteredMonitors() = monitors.Keys |> Seq.toArray
    /// Register a monitor
    member __.Register(monitor : IMonitor) = monitors.TryAdd(monitor, byte 0) |> ignore
    /// Unregister a monitor
    member __.Unregister(monitor : IMonitor) = monitors.TryRemove(monitor) |> ignore
    /// Check if a monitor is registered
    member __.IsRegistered(monitor : IMonitor) = monitors.ContainsKey(monitor)

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
