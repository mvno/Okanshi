namespace Okanshi

open System
open System.Collections.Generic

module HealthChecks =
    let mutable private healthChecks = Map.empty

    /// Add a health check
    let Add (name : string, x : Func<bool>) =
        healthChecks <- healthChecks.Add (name, x)

    /// Check if a health check already exists
    let Exists (name) =
        name |> healthChecks.ContainsKey

    /// Clear all healthchecks
    let Clear() =
        healthChecks <- Map.empty

    /// Run all healthchecks defined
    let RunAll() =
        let dict = new Dictionary<string, bool>()
        healthChecks |> Map.iter (fun key value -> dict.Add(key, value.Invoke()))
        dict
