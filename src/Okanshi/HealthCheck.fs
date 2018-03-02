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
    member __.GetValues() = check'.GetValues()

    /// Gets the value and resets the monitor
    member __.GetValuesAndReset() = check'.GetValuesAndReset()
    
    /// The config
    member __.Config = config'

    interface IMonitor with
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast
