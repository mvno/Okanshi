namespace Okanshi

open System
open System.Collections.Generic
open System.Threading
open System.Collections.Concurrent

/// Tuple representing a key used to identify a monitor
type MonitorKey = (string * Type * string)
/// Type representing a monitor factory
type MonitorFactory = unit -> IMonitor

/// Static use of monitors
[<AbstractClass; Sealed>]
type OkanshiMonitor private() =
    static let registeredMonitors = new Dictionary<MonitorKey, IMonitor>()
    static let registeredMonitorsLock = new obj()
    static let monitorRegistry = DefaultMonitorRegistry.Instance
        
    static let getOrAddMonitor monitorKey (factory : MonitorFactory) =
        // It is a conscious to use a lock instead of a ConcurrentDictionary. This is due to the fact that the
        // factory method in ConcurrentDictionary.GetOrAdd, aren't thread-safe, it may be called multiple times
        // which would cause the monitor to be registered multiple times.
        lock registeredMonitorsLock (fun () ->
            match registeredMonitors.TryGetValue(monitorKey) with
            | true, result -> result
            | false, _ ->
                let monitor = factory()
                registeredMonitors.Add(monitorKey, monitor)
                monitorRegistry.Register(monitor)
                monitor)

    static let tagDictionary = new ConcurrentDictionary<Tag, byte>()
    static let defaultStep = TimeSpan.FromMinutes(float 1)

    /// Gets the default step size
    static member DefaultStep = defaultStep
    /// Gets the default tags added to all monitors created
    static member DefaultTags with get() = tagDictionary.Keys |> Seq.toArray
    /// Sets the default tags added to all monitors created
    static member DefaultTags
        with set(value : Tag array) =
            tagDictionary.Clear()
            value |> Seq.iter (fun x -> tagDictionary.TryAdd(x, byte 0) |> ignore)
    /// Gets the monitor key used to identify monitors
    static member GetMonitorKey(name : string, monitorType : Type, tags : Tag array) =
        let tagKeys = tags |> Array.map (fun x -> sprintf "%s; %s" x.Key x.Value)
        (name, monitorType, String.Join(",", tagKeys))

    /// Get or add a BasicCounter
    static member BasicCounter(name : string) = OkanshiMonitor.BasicCounter(name, [||])

    /// Get or add a BasicCounter with custom tags
    static member BasicCounter(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<BasicCounter>, tags)
        let factory = fun () -> (new BasicCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)) :> IMonitor)
        getOrAddMonitor monitorKey factory :?> BasicCounter

    /// Get or add a StepCounter, with a step size of 1 minute
    static member StepCounter(name) = OkanshiMonitor.StepCounter(name, defaultStep, [||])
    
    /// Get or add a StepCounter with custom tags and a step size of 1 minute
    static member StepCounter(name, tags : Tag array) = OkanshiMonitor.StepCounter(name, defaultStep, tags)

    /// Get or add a StepCounter
    static member StepCounter(name : string, step, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<StepCounter>, tags)
        let factory = fun () -> (new StepCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), step) :> IMonitor)
        getOrAddMonitor monitorKey factory :?> StepCounter

    /// Get or add a PeakRateCounter, with a step size of 1 minute
    static member PeakRateCounter(name) = OkanshiMonitor.PeakRateCounter(name, defaultStep, [||])
    
    /// Get or add a PeakRateCounter with custom tags and a step size of 1 minute
    static member PeakRateCounter(name, tags : Tag array) = OkanshiMonitor.PeakRateCounter(name, defaultStep, tags)

    /// Get or add a PeakRateCounter
    static member PeakRateCounter(name : string, step, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<PeakRateCounter>, tags)
        let factory = fun () -> (new PeakRateCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), step) :> IMonitor)
        getOrAddMonitor monitorKey factory :?> PeakRateCounter

    /// Get or add a DoubleCounter, with a step size of 1 minute
    static member DoubleCounter(name) = OkanshiMonitor.DoubleCounter(name, defaultStep, [||])

    /// Get or add a DoubleCounter with custom tags and a step size of 1 minute
    static member DoubleCounter(name, tags : Tag array) = OkanshiMonitor.DoubleCounter(name, defaultStep, tags)

    /// Get or add a DoubleCounter
    static member DoubleCounter(name : string, step, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<DoubleCounter>, tags)
        let factory = fun () -> (new DoubleCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), step) :> IMonitor)
        getOrAddMonitor monitorKey factory :?> DoubleCounter

    /// Get or add a BasicGauge
    static member BasicGauge(name : string, getValue : Func<'T>) = OkanshiMonitor.BasicGauge(name, getValue, [||])

    /// Get or add a BasicGauge with custom tags
    static member BasicGauge<'T>(name : string, getValue : Func<'T>, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<BasicGauge<'T>>, tags)
        let factory = fun () -> (new BasicGauge<'T>(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), getValue)) :> IMonitor
        getOrAddMonitor monitorKey factory :?> BasicGauge<'T>

    /// Get or add a MaxGauge 
    static member MaxGauge(name : string) = OkanshiMonitor.MaxGauge(name, [||])

    /// Get or add a MaxGaug with custom tags
    static member MaxGauge(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<MaxGauge>, tags)
        let factory = fun () -> (new MaxGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        getOrAddMonitor monitorKey factory :?> MaxGauge

    /// Get or add a MinGauge
    static member MinGauge(name : string) = OkanshiMonitor.MinGauge(name, [||])

    /// Get or add a MinGauge with custom tags
    static member MinGauge(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<MinGauge>, tags)
        let factory = fun () -> (new MaxGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        getOrAddMonitor monitorKey factory :?> MinGauge

    /// Get or add a AverageGauge
    static member AverageGauge(name : string, step) = OkanshiMonitor.AverageGauge(name, step, [||])

    /// Get or add a AverageGauge with custom tags
    static member AverageGauge(name : string, step, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<AverageGauge>, tags)
        let factory = fun () -> (new AverageGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), step)) :> IMonitor
        getOrAddMonitor monitorKey factory :?> AverageGauge

    /// Get or add a LongGauge
    static member LongGauge(name : string) = OkanshiMonitor.LongGauge(name, [||])

    /// Get or add a LongGauge
    static member LongGauge(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<LongGauge>, tags)
        let factory = fun () -> (new LongGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        getOrAddMonitor monitorKey factory :?> LongGauge

    /// Get or add a DoubleGauge
    static member DoubleGauge(name : string) = OkanshiMonitor.DoubleGauge(name, [||])
    
    /// Get or add a DoubleGauge with custom tags
    static member DoubleGauge(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<DoubleGauge>, tags)
        let factory = fun () -> (new DoubleGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        getOrAddMonitor monitorKey factory :?> DoubleGauge

    /// Get or add a DecimalGauge
    static member DecimalGauge(name : string) = OkanshiMonitor.DecimalGauge(name, [||])

    /// Get or add a DecimalGauge with custom tags
    static member DecimalGauge(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<DecimalGauge>, tags)
        let factory = fun () -> (new DecimalGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        getOrAddMonitor monitorKey factory :?> DecimalGauge

    /// Get or add a BasicTimer, with a step size of 1 minute
    static member BasicTimer(name) = OkanshiMonitor.BasicTimer(name, defaultStep, [||])

    /// Get or add a BasicTimer with custom tags and a step size of 1 minute
    static member BasicTimer(name, tags : Tag array) = OkanshiMonitor.BasicTimer(name, defaultStep, tags)

    /// Get or add a BasicTimer
    static member BasicTimer(name : string, step, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<BasicTimer>, tags)
        let factory = fun () -> (new BasicTimer(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), step)) :> IMonitor
        getOrAddMonitor monitorKey factory :?> BasicTimer

    /// Get or add a DurationTimer
    static member LongTaskTimer(name) = OkanshiMonitor.LongTaskTimer(name, [||])

    /// Get or add a DurationTimer with custom tags
    static member LongTaskTimer(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<LongTaskTimer>, tags)
        let factory = fun () -> (new LongTaskTimer(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        getOrAddMonitor monitorKey factory :?> LongTaskTimer

    /// Get or add a HealthCheck
    static member HealthCheck(check, name) = OkanshiMonitor.HealthCheck(check, name)

    /// Get or add a HealthCheck with custom tags
    static member HealthCheck(check, name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<HealthCheck>, tags)
        let factory = fun () -> (new HealthCheck(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), check)) :> IMonitor
        getOrAddMonitor monitorKey factory :?> HealthCheck

    /// Get or add a performance counter monitor
    static member PerformanceCounter(check, name) = OkanshiMonitor.PerformanceCounter(check, name)

    /// Get or add a performance counter monitor with custom tags
    static member PerformanceCounter(counterConfig : PerformanceCounterConfig, name, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<PerformanceCounterMonitor>, tags)
        let factory = fun () -> (new PerformanceCounterMonitor(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), counterConfig)) :> IMonitor
        getOrAddMonitor monitorKey factory :?> PerformanceCounterMonitor
