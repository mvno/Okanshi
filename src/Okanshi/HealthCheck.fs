namespace Okanshi

open System
open System.Collections.Generic
open System.Collections.Concurrent

/// Handles healthchecks
[<AbstractClass; Sealed>]
type HealthChecks private () =
    static let mutable healthChecks = new ConcurrentDictionary<_, _>()

    /// Add a health check
    static member Add (name : string, x : Func<bool>) = healthChecks.TryAdd (name, x) |> ignore
   
    /// Check if a health check already exists
    static member Exists (name) = name |> healthChecks.ContainsKey
   
    /// Clear all healthchecks
    static member Clear() = healthChecks.Clear()
    
    /// Run all healthchecks defined
    static member RunAll() =
        let dict = new Dictionary<string, bool>()
        healthChecks.ToArray() |> Seq.iter (fun kvp -> dict.Add(kvp.Key, kvp.Value.Invoke()))
        dict

/// Health check monitor
type HealthCheck(registry : IMonitorRegistry, config : MonitorConfig, check : Func<bool>) =
    let config' = config.WithTag(DataSourceType.HealthCheck)

    let check' = new BasicGauge<bool>(config', check)
    
    new (config, check) = HealthCheck(DefaultMonitorRegistry.Instance, config, check)

    /// Get value
    member __.GetValue() = check'.GetValue()

    /// Gets the value and resets the monitor
    member __.GetValueAndReset() = check'.GetValueAndReset()

    /// Gets all the monitors on the current monitor. This is the best way to handle
    /// sub monitors.
    member self.GetAllMonitors() = seq { yield self :> IMonitor }
    
    /// The config
    member __.Config = config'

    interface IMonitor with
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config
        member self.GetValueAndReset() = self.GetValueAndReset() :> obj
        member self.GetAllMonitors() = self.GetAllMonitors()
