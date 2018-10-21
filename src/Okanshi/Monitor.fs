namespace Okanshi

open System
open System.Collections.Generic

/// Tuple representing a key used to identify a monitor
type MonitorKey = string * Type * string

/// Type representing a monitor factory
type MonitorFactory = unit -> IMonitor

/// Static use of monitors
[<AbstractClass; Sealed>]
type OkanshiMonitor private () = 
    static let monitorRegistry = DefaultMonitorRegistry.Instance
    static let mutable defaultTags = new HashSet<Tag>() :> ISet<Tag>;

    static let StandardMeasurementNames = dict [ (typeof<CumulativeCounter>, dict[("value","value")])]
    static let LegacyMeasurementNames = dict [ (typeof<CumulativeCounter>, dict[("value","value")])]

    static let mutable defaultMeasurementNames = StandardMeasurementNames
    

    /// Gets the default tags added to all monitors created
    static member DefaultTags
        with get () = defaultTags
        /// Sets the default tags added to all monitors created
        and set (value: ISet<Tag>) =
            defaultTags <- value
    
    /// Gets the default measurement names 
    static member DefaultMeasurementNames
        with get () = defaultMeasurementNames
        and set (value) =
            defaultMeasurementNames <- value

    /// Get or create a CumulativeCounter
    static member CumulativeCounter(name : string) = OkanshiMonitor.CumulativeCounter(name, [||])
    
    /// Get or create a CumulativeCounter with custom tags
    static member CumulativeCounter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new CumulativeCounter(x))
    
    /// Get or create a PeakRateCounter
    static member Counter(name) = OkanshiMonitor.Counter(name, [||])
    
    /// Get or create a PeakRateCounter with custom tags
    static member Counter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new Counter(x))
    
    /// Get or create a DoubleCounter
    static member DoubleCounter(name) = OkanshiMonitor.DoubleCounter(name, [||])
    
    /// Get or create a DoubleCounter
    static member DoubleCounter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new DoubleCounter(x))
    
    /// Get or create a Gauge
    static member Gauge(name : string, getValue : Func<'T>) = OkanshiMonitor.Gauge(name, getValue, [||])
    
    /// Get or create a Gauge with custom tags
    static member Gauge<'T>(name : string, getValue : Func<'T>, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        let names = OkanshiMonitor.DefaultMeasurementNames.[typeof<MaxGauge>]
        monitorRegistry.GetOrAdd(config, fun x -> new Gauge<'T>(x, getValue))
    
    /// Get or create a MaxGauge 
    static member MaxGauge(name : string) = OkanshiMonitor.MaxGauge(name, [||])
    
    /// Get or create a MaxGaug with custom tags
    static member MaxGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        let names = OkanshiMonitor.DefaultMeasurementNames.[typeof<MaxGauge>]
        monitorRegistry.GetOrAdd(config, fun x -> new MaxGauge(x, names))
    
    /// Get or create a MinGauge
    static member MinGauge(name : string) = OkanshiMonitor.MinGauge(name, [||])
    
    /// Get or create a MinGauge with custom tags
    static member MinGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        let names = OkanshiMonitor.DefaultMeasurementNames.[typeof<MinGauge>]
        monitorRegistry.GetOrAdd(config, fun x -> new MinGauge(x, names))

    /// Get or create a MinMaxAvgGauge with custom tags
    static member MinMaxAvgGauge(name : string) = OkanshiMonitor.MinMaxAvgGauge(name, [||])
    
    /// Get or create a MinMaxAvgGauge with custom tags
    static member MinMaxAvgGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        let names = OkanshiMonitor.DefaultMeasurementNames.[typeof<MinMaxAvgGauge>]
        monitorRegistry.GetOrAdd(config, fun x -> new MinMaxAvgGauge(x, names))
    
    /// Get or create a AverageGauge
    static member AverageGauge(name : string) = OkanshiMonitor.AverageGauge(name, [||])
    
    /// Get or create a AverageGauge with custom tags
    static member AverageGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        let names = OkanshiMonitor.DefaultMeasurementNames.[typeof<AverageGauge>]
        monitorRegistry.GetOrAdd(config, fun x -> new AverageGauge(x, names))
    
    /// Get or create a LongGauge
    static member LongGauge(name : string) = OkanshiMonitor.LongGauge(name, [||])
    
    /// Get or create a LongGauge
    static member LongGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        let names = OkanshiMonitor.DefaultMeasurementNames.[typeof<LongGauge>]
        monitorRegistry.GetOrAdd(config, fun x -> new LongGauge(x, names))
    
    /// Get or create a DoubleGauge
    static member DoubleGauge(name : string) = OkanshiMonitor.DoubleGauge(name, [||])
    
    /// Get or create a DoubleGauge with custom tags
    static member DoubleGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        let names = OkanshiMonitor.DefaultMeasurementNames.[typeof<DoubleGauge>]
        monitorRegistry.GetOrAdd(config, fun x -> new DoubleGauge(x, names))
    
    /// Get or create a DecimalGauge
    static member DecimalGauge(name : string) = OkanshiMonitor.DecimalGauge(name, [||])
    
    /// Get or create a DecimalGauge with custom tags
    static member DecimalGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        let names = OkanshiMonitor.DefaultMeasurementNames.[typeof<DecimalGauge>]
        monitorRegistry.GetOrAdd(config, fun x -> new DecimalGauge(x, names))
    
    /// Get or create a Timer, with a step size of 1 minute
    static member Timer(name) = OkanshiMonitor.Timer(name, [||])
    
    /// Get or create a Timer with custom tags
    static member Timer(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new Timer(x))

    /// Get or create a SLA-Timer, with a step size of 1 minute
    static member SlaTimer(name : string, sla: TimeSpan) = OkanshiMonitor.SlaTimer(name, sla, [||])
    
    /// Get or create a SLA-Timer with custom tags
    static member SlaTimer(name : string, sla : TimeSpan, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new SlaTimer(x, sla))

#if NET45
    /// Get or create a performance counter monitor
    static member PerformanceCounter(check, name) = OkanshiMonitor.PerformanceCounter(check, name)
    
    /// Get or create a performance counter monitor with custom tags
    static member PerformanceCounter(counterConfig : PerformanceCounterConfig, name, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new PerformanceCounterMonitor(x, counterConfig))
#endif
