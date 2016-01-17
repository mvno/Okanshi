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
