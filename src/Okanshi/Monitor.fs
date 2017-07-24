namespace Okanshi

open System
open System.Collections.Generic
open System.Threading
open System.Collections.Concurrent

/// Tuple representing a key used to identify a monitor
type MonitorKey = string * Type * string

/// Type representing a monitor factory
type MonitorFactory = unit -> IMonitor

/// Static use of monitors
[<AbstractClass; Sealed>]
type OkanshiMonitor private () = 
    static let registeredMonitors = new Dictionary<MonitorKey, IMonitor>()
    static let registeredMonitorsLock = new obj()
    static let monitorRegistry = DefaultMonitorRegistry.Instance
    static let tagDictionary = new ConcurrentDictionary<Tag, byte>()
    static let defaultStep = TimeSpan.FromMinutes(float 1)
    
    /// Gets the default step size
    static member DefaultStep = defaultStep
    
    /// Gets the default tags added to all monitors created
    static member DefaultTags 
        with get () = tagDictionary.Keys |> Seq.toArray
        /// Sets the default tags added to all monitors created
        and set (value : Tag array) = 
            tagDictionary.Clear()
            value |> Seq.iter (fun x -> tagDictionary.TryAdd(x, byte 0) |> ignore)
    
    /// Get or add a BasicCounter
    static member BasicCounter(name : string) = OkanshiMonitor.BasicCounter(name, [||])
    
    /// Get or add a BasicCounter with custom tags
    static member BasicCounter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new BasicCounter(x))
    
    /// Get or add a StepCounter, with a step size of 1 minute
    static member StepCounter(name) = OkanshiMonitor.StepCounter(name, defaultStep, [||])
    
    /// Get or add a StepCounter with custom tags and a step size of 1 minute
    static member StepCounter(name, tags : Tag array) = OkanshiMonitor.StepCounter(name, defaultStep, tags)
    
    /// Get or add a StepCounter
    static member StepCounter(name : string, step, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new StepCounter(x, step))
    
    /// Get or add a PeakRateCounter, with a step size of 1 minute
    static member PeakRateCounter(name) = OkanshiMonitor.PeakRateCounter(name, defaultStep, [||])
    
    /// Get or add a PeakRateCounter with custom tags and a step size of 1 minute
    static member PeakRateCounter(name, tags : Tag array) = OkanshiMonitor.PeakRateCounter(name, defaultStep, tags)
    
    /// Get or add a PeakRateCounter
    static member PeakRateCounter(name : string, step, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new PeakRateCounter(x, step))
    
    /// Get or add a DoubleCounter, with a step size of 1 minute
    static member DoubleCounter(name) = OkanshiMonitor.DoubleCounter(name, defaultStep, [||])
    
    /// Get or add a DoubleCounter with custom tags and a step size of 1 minute
    static member DoubleCounter(name, tags : Tag array) = OkanshiMonitor.DoubleCounter(name, defaultStep, tags)
    
    /// Get or add a DoubleCounter
    static member DoubleCounter(name : string, step, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new DoubleCounter(x, step))
    
    /// Get or add a BasicGauge
    static member BasicGauge(name : string, getValue : Func<'T>) = OkanshiMonitor.BasicGauge(name, getValue, [||])
    
    /// Get or add a BasicGauge with custom tags
    static member BasicGauge<'T>(name : string, getValue : Func<'T>, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new BasicGauge<'T>(x, getValue))
    
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
    static member AverageGauge(name : string, step) = OkanshiMonitor.AverageGauge(name, step, [||])
    
    /// Get or add a AverageGauge with custom tags
    static member AverageGauge(name : string, step, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new AverageGauge(x, step))
    
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
    
    /// Get or add a BasicTimer, with a step size of 1 minute
    static member BasicTimer(name) = OkanshiMonitor.BasicTimer(name, defaultStep, [||])
    
    /// Get or add a BasicTimer with custom tags and a step size of 1 minute
    static member BasicTimer(name, tags : Tag array) = OkanshiMonitor.BasicTimer(name, defaultStep, tags)
    
    /// Get or add a BasicTimer
    static member BasicTimer(name : string, step, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new BasicTimer(x, step))
    
    /// Get or add a DurationTimer
    static member LongTaskTimer(name) = OkanshiMonitor.LongTaskTimer(name, [||])
    
    /// Get or add a DurationTimer with custom tags
    static member LongTaskTimer(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new LongTaskTimer(x))
    
    /// Get or add a HealthCheck
    static member HealthCheck(check, name) = OkanshiMonitor.HealthCheck(check, name)
    
    /// Get or add a HealthCheck with custom tags
    static member HealthCheck(check, name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new HealthCheck(x, check))
    
    /// Get or add a performance counter monitor
    static member PerformanceCounter(check, name) = OkanshiMonitor.PerformanceCounter(check, name)
    
    /// Get or add a performance counter monitor with custom tags
    static member PerformanceCounter(counterConfig : PerformanceCounterConfig, name, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new PerformanceCounterMonitor(x, counterConfig))
