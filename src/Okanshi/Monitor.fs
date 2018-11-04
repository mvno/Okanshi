namespace Okanshi

open System
open System.Collections.Generic

/// Global monitor factory
[<AbstractClass; Sealed>]
type OkanshiMonitor private () = 
    static let monitorRegistry = DefaultMonitorRegistry.Instance
    static let mutable defaultTags = new HashSet<Tag>() :> ISet<Tag>;
    static let factory = new MonitorFactory(monitorRegistry, defaultTags)
    
    /// factory for zero value filtering
    static member WithZeroFiltering = new ZeroFilterFactory(monitorRegistry, defaultTags)
    
    /// Gets the default tags added to all monitors created
    static member DefaultTags
        with get () = defaultTags
        /// Sets the default tags added to all monitors created
        and set (value: ISet<Tag>) =
            OkanshiMonitor.WithZeroFiltering.UpdateDefaultTags(value)
            factory.UpdateDefaultTags(value)
            defaultTags <- value
    
    /// Get or create a CumulativeCounter
    static member CumulativeCounter(name : string) = factory.CumulativeCounter(name, [||])
    
    /// Get or create a CumulativeCounter with custom tags
    static member CumulativeCounter(name : string, tags : Tag array) = factory.CumulativeCounter(name, tags)
    
    /// Get or create a PeakRateCounter
    static member Counter(name) = factory.Counter(name, [||])
    
    /// Get or create a PeakRateCounter with custom tags
    static member Counter(name : string, tags : Tag array) = factory.Counter(name, tags)
    
    /// Get or create a DoubleCounter
    static member DoubleCounter(name) = factory.DoubleCounter(name, [||])
    
    /// Get or create a DoubleCounter
    static member DoubleCounter(name : string, tags : Tag array) = factory.DoubleCounter(name, tags)
    
    /// Get or create a Gauge
    static member Gauge(name : string, getValue : Func<'T>) = factory.Gauge(name, getValue, [||])
    
    /// Get or create a Gauge with custom tags
    static member Gauge<'T>(name : string, getValue : Func<'T>, tags : Tag array) = factory.Gauge(name, getValue, tags)
    
    /// Get or create a MaxGauge 
    static member MaxGauge(name : string) = factory.MaxGauge(name, [||])
    
    /// Get or create a MaxGaug with custom tags
    static member MaxGauge(name : string, tags : Tag array) = factory.MaxGauge(name, tags)
    
    /// Get or create a MinGauge
    static member MinGauge(name : string) = factory.MinGauge(name, [||])
    
    /// Get or create a MinGauge with custom tags
    static member MinGauge(name : string, tags : Tag array) = factory.MinGauge(name, tags)

    /// Get or create a MinMaxAvgGauge with custom tags
    static member MinMaxAvgGauge(name : string) = factory.MinMaxAvgGauge(name, [||])
    
    /// Get or create a MinMaxAvgGauge with custom tags
    static member MinMaxAvgGauge(name : string, tags : Tag array) = factory.MinMaxAvgGauge(name, tags)
    
    /// Get or create a AverageGauge
    static member AverageGauge(name : string) = factory.AverageGauge(name, [||])
    
    /// Get or create a AverageGauge with custom tags
    static member AverageGauge(name : string, tags : Tag array) = factory.AverageGauge(name, tags)
    
    /// Get or create a LongGauge
    static member LongGauge(name : string) = factory.LongGauge(name, [||])
    
    /// Get or create a LongGauge
    static member LongGauge(name : string, tags : Tag array) = factory.LongGauge(name, tags)
    
    /// Get or create a DoubleGauge
    static member DoubleGauge(name : string) = factory.DoubleGauge(name, [||])
    
    /// Get or create a DoubleGauge with custom tags
    static member DoubleGauge(name : string, tags : Tag array) = factory.DoubleGauge(name, tags)
    
    /// Get or create a DecimalGauge
    static member DecimalGauge(name : string) = factory.DecimalGauge(name, [||])
    
    /// Get or create a DecimalGauge with custom tags
    static member DecimalGauge(name : string, tags : Tag array) = factory.DecimalGauge(name, tags)
    
    /// Get or create a Timer, with a step size of 1 minute
    static member Timer(name) = factory.Timer(name, [||])
    
    /// Get or create a Timer with custom tags
    static member Timer(name : string, tags : Tag array) = factory.Timer(name, tags)

    /// Get or create a SLA-Timer, with a step size of 1 minute
    static member SlaTimer(name : string, sla: TimeSpan) = factory.SlaTimer(name, sla, [||])
    
    /// Get or create a SLA-Timer with custom tags
    static member SlaTimer(name : string, sla : TimeSpan, tags : Tag array) = factory.SlaTimer(name, sla, tags)

#if NET46
    /// Get or create a performance counter monitor
    static member PerformanceCounter(check, name) = factory.PerformanceCounter(check, name, [||])
    
    /// Get or create a performance counter monitor with custom tags
    static member PerformanceCounter(counterConfig : PerformanceCounterConfig, name, tags : Tag array) = factory.PerformanceCounter(counterConfig, name, tags)
#endif
