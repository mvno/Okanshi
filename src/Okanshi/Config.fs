namespace Okanshi

open System

/// A tag used to attach information to a monitor
type Tag = { Key : string; Value : string }

/// Configuration of a monitor
[<CustomEquality; NoComparison>]
type MonitorConfig =
    {
        /// Name of the monitor
        Name: string;
        /// Monitor tags
        Tags : Tag array
    }
    /// Builder method of the configuration
    static member Build(name) =
        { Name = name; Tags = [||] }
    /// Adds a tag specified by the key and value
    member self.WithTag(key, value) =
        self.WithTag({ Key = key; Value = value })
    /// Adds a tag
    member self.WithTag(tag) =
        { self with Tags = self.Tags |> Array.append [| tag |] }
    /// Adds multiple tags to the configuration
    member self.WithTags(tags : Tag seq) =
        { self with Tags = self.Tags |> Array.append (tags |> Seq.toArray) }

    member self.Equals(other : MonitorConfig) =
        if Object.ReferenceEquals(other, null) then false
        elif Object.ReferenceEquals(other, self) then true
        else other.GetHashCode() = self.GetHashCode()

    override self.Equals(other : obj) =
        match other with
        | :? MonitorConfig as x -> self.Equals(x)
        | _ -> false

    override self.GetHashCode() =
        let mutable hash = 2166136261L |> int
        hash <- (hash * 16777619) ^^^ self.Name.GetHashCode()
        self.Tags |> Seq.fold (fun state x -> (state * 16777619) ^^^ x.GetHashCode()) hash

    interface IEquatable<MonitorConfig> with
        member self.Equals(other) = self.Equals(other)

/// A monitor
type IMonitor =
    /// Gets the value of the monitor
    abstract member GetValue : unit -> obj
    /// Gets the configuration of the monitor
    abstract member Config : MonitorConfig

/// The types used to indicate the type of a value
[<AbstractClass; Sealed>]
type DataSourceType private() =
    [<Literal>]
    static let Key = "dataSource"
    
    /// A gauge is a metric that can be sampled without modification
    ///
    /// Examples of metrics could be number of connections, disk usage, etc.
    static member Gauge = { Key = Key; Value = "gauge" }
    
    /// A counter is for numeric values that get incremented when some event occurs.
    ///
    /// Most counters will be sampled and converted to a rate of change per second.
    static member Counter = { Key = Key; Value = "counter" }
    
    /// A rate is for numeric values that represents a rate per second.
    static member Rate = { Key = Key; Value = "rate" }

    /// A health check value
    static member HealthCheck = { Key = Key; Value = "healthCheck" }
