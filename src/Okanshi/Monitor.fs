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
    static let defaultStep = TimeSpan.FromMinutes(float 1)
    
    /// Gets the default step size
    static member DefaultStep = defaultStep
    
    /// Gets the default tags added to all monitors created
    static member DefaultTags
        with get () = defaultTags
        /// Sets the default tags added to all monitors created
        and set (value: ISet<Tag>) =
            defaultTags <- value
    
    /// Get or add a CumulativeCounter
    static member CumulativeCounter(name : string) = OkanshiMonitor.CumulativeCounter(name, [||])
    
    /// Get or add a CumulativeCounter with custom tags
    static member CumulativeCounter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new CumulativeCounter(x))
    
    /// Get or add a PeakRateCounter
    static member Counter(name) = OkanshiMonitor.Counter(name, [||])
    
    /// Get or add a PeakRateCounter with custom tags
    static member Counter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new Counter(x))
    
    /// Get or add a DoubleCounter
    static member DoubleCounter(name) = OkanshiMonitor.DoubleCounter(name, [||])
    
    /// Get or add a DoubleCounter
    static member DoubleCounter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new DoubleCounter(x))
    
    /// Get or add a Gauge
    static member Gauge(name : string, getValue : Func<'T>) = OkanshiMonitor.Gauge(name, getValue, [||])
    
    /// Get or add a Gauge with custom tags
    static member Gauge<'T>(name : string, getValue : Func<'T>, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new Gauge<'T>(x, getValue))
    
    /// Get or add a MaxGauge 
    static member MaxGauge(name : string) = OkanshiMonitor.MaxGauge(name, [||])
    
    /// Get or add a MaxGaug with custom tags
    static member MaxGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new MaxGauge(x))
    
    /// Get or add a MinGauge
    static member MinGauge(name : string) = OkanshiMonitor.MinGauge(name, [||])
    
    /// Get or add a MinGauge with custom tags
    static member MinGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new MinGauge(x))
    
    /// Get or add a AverageGauge
    static member AverageGauge(name : string) = OkanshiMonitor.AverageGauge(name, [||])
    
    /// Get or add a AverageGauge with custom tags
    static member AverageGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new AverageGauge(x))
    
    /// Get or add a LongGauge
    static member LongGauge(name : string) = OkanshiMonitor.LongGauge(name, [||])
    
    /// Get or add a LongGauge
    static member LongGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new LongGauge(x))
    
    /// Get or add a DoubleGauge
    static member DoubleGauge(name : string) = OkanshiMonitor.DoubleGauge(name, [||])
    
    /// Get or add a DoubleGauge with custom tags
    static member DoubleGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new DoubleGauge(x))
    
    /// Get or add a DecimalGauge
    static member DecimalGauge(name : string) = OkanshiMonitor.DecimalGauge(name, [||])
    
    /// Get or add a DecimalGauge with custom tags
    static member DecimalGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new DecimalGauge(x))
    
    /// Get or add a Timer, with a step size of 1 minute
    static member Timer(name) = OkanshiMonitor.Timer(name, [||])
    
    /// Get or add a Timer with custom tags
    static member Timer(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new Timer(x))

    /// Get or add a ApdexTimer, with a step size of 1 minute
    static member ApdexTimer(name : string, toleratableThreshold : TimeSpan) = OkanshiMonitor.ApdexTimer(name, toleratableThreshold, [||])
    
    /// Get or add a ApdexTimer with custom tags
    static member ApdexTimer(name : string, toleratableThreshold : TimeSpan, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new ApdexTimer(x, toleratableThreshold))
    
    
#if NET45
    /// Get or add a performance counter monitor
    static member PerformanceCounter(check, name) = OkanshiMonitor.PerformanceCounter(check, name)
    
    /// Get or add a performance counter monitor with custom tags
    static member PerformanceCounter(counterConfig : PerformanceCounterConfig, name, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new PerformanceCounterMonitor(x, counterConfig))
#endif
