namespace Okanshi

open System
open System.Collections.Generic

type IMonitorFactory = 
    abstract CumulativeCounter : string -> ICounter<int64>
    abstract CumulativeCounter : string * Tag array -> ICounter<int64>
    abstract Counter : string -> ICounter<int64>
    abstract Counter : string * Tag array -> ICounter<int64>
    abstract DoubleCounter : string -> ICounter<double>
    abstract DoubleCounter: string * Tag array -> ICounter<double>
    abstract MaxGauge: string -> IGauge<int64>
    abstract MaxGauge: string * Tag array -> IGauge<int64>
    abstract MinGauge: string -> IGauge<int64>
    abstract MinGauge: string * Tag array -> IGauge<int64>
    abstract MinMaxAvgGauge: string -> IGauge<float>
    abstract MinMaxAvgGauge: string * Tag array -> IGauge<float>
    abstract AverageGauge: string -> IGauge<float>
    abstract AverageGauge: string * Tag array -> IGauge<float>
    abstract LongGauge: string -> IGauge<int64>
    abstract LongGauge: string * Tag array -> IGauge<int64>
    abstract DoubleGauge: string -> IGauge<double>
    abstract DoubleGauge: string * Tag array -> IGauge<double>
    abstract DecimalGauge: string -> IGauge<decimal>
    abstract DecimalGauge: string * Tag array -> IGauge<decimal>
    abstract Timer: string -> ITimer
    abstract Timer: string * Tag array -> ITimer
    abstract SlaTimer: string * TimeSpan -> ITimer
    abstract SlaTimer: string * TimeSpan * Tag array -> ITimer
#if NET46
    abstract PerformanceCounter: PerformanceCounterConfig * string -> IMonitor
    abstract PerformanceCounter: PerformanceCounterConfig * string * Tag array -> IMonitor
#endif

/// The AbsentFilterFactory can create monitors wrapped in an absent filter preventing okanshi from 
/// sending 0-values when no measurements are made
type AbsentMeasurementsFilterFactory (monitorRegistry : IMonitorRegistry, DefaultTags, monitorFactory : MonitorFactory) = 
    let mutable defaultTags = DefaultTags

    new (monitorRegistry, defaultTags) = AbsentMeasurementsFilterFactory (monitorRegistry, defaultTags, new MonitorFactory(monitorRegistry, defaultTags))

    member self.WithNoFiltering = monitorFactory 

    member self.UpdateDefaultTags(newTags) = 
        defaultTags <- newTags
        self.WithNoFiltering.UpdateDefaultTags(newTags)

    member internal self.UpdateDefaultTagsNonRecursive(newTags) = 
        defaultTags <- newTags
    
    /// Get or create a CumulativeCounter
    member self.CumulativeCounter(name : string) = self.CumulativeCounter(name, [||])
    
    /// Get or create a CumulativeCounter with custom tags
    member self.CumulativeCounter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new CounterAbsentFilter<int64>(new CumulativeCounter(x)))
    
    /// Get or create a PeakRateCounter
    member self.Counter(name) = self.Counter(name, [||])
    
    /// Get or create a PeakRateCounter with custom tags
    member self.Counter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new CounterAbsentFilter<int64>(new Counter(x)))
    
    /// Get or create a DoubleCounter
    member self.DoubleCounter(name) = self.DoubleCounter(name, [||])
    
    /// Get or create a DoubleCounter
    member self.DoubleCounter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new CounterAbsentFilter<double>(new DoubleCounter(x)))

    /// Get or create a MaxGauge 
    member self.MaxGauge(name : string) = self.MaxGauge(name, [||])
    
    /// Get or create a MaxGaug with custom tags
    member self.MaxGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new GaugeAbsentFilter<int64>(new MaxGauge(x)))
    
    /// Get or create a MinGauge
    member self.MinGauge(name : string) = self.MinGauge(name, [||])
    
    /// Get or create a MinGauge with custom tags
    member self.MinGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new GaugeAbsentFilter<int64>(new MinGauge(x)))

    /// Get or create a MinMaxAvgGauge with custom tags
    member self.MinMaxAvgGauge(name : string) = self.MinMaxAvgGauge(name, [||])
    
    /// Get or create a MinMaxAvgGauge with custom tags
    member self.MinMaxAvgGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new GaugeAbsentFilter<float>(new MinMaxAvgGauge(x)))
    
    /// Get or create a AverageGauge
    member self.AverageGauge(name : string) = self.AverageGauge(name, [||])
    
    /// Get or create a AverageGauge with custom tags
    member self.AverageGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new GaugeAbsentFilter<float>(new AverageGauge(x)))
    
    /// Get or create a LongGauge
    member self.LongGauge(name : string) = self.LongGauge(name, [||])
    
    /// Get or create a LongGauge
    member self.LongGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new GaugeAbsentFilter<int64>(new LongGauge(x)))
    
    /// Get or create a DoubleGauge
    member self.DoubleGauge(name : string) = self.DoubleGauge(name, [||])
    
    /// Get or create a DoubleGauge with custom tags
    member self.DoubleGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new GaugeAbsentFilter<double>(new DoubleGauge(x)))
    
    /// Get or create a DecimalGauge
    member self.DecimalGauge(name : string) = self.DecimalGauge(name, [||])
    
    /// Get or create a DecimalGauge with custom tags
    member self.DecimalGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new GaugeAbsentFilter<decimal>(new DecimalGauge(x)))
    
    /// Get or create a Timer, with a step size of 1 minute
    member self.Timer(name) = self.Timer(name, [||])
    
    /// Get or create a Timer with custom tags
    member self.Timer(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new TimerAbsentFilter(new Timer(x)))

    /// Get or create a SLA-Timer, with a step size of 1 minute
    member self.SlaTimer(name : string, sla: TimeSpan) = self.SlaTimer(name, sla, [||])
    
    /// Get or create a SLA-Timer with custom tags
    member self.SlaTimer(name : string, sla : TimeSpan, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new TimerAbsentFilter(new SlaTimer(x, sla)))

#if NET46
    /// Get or create a performance counter monitor
    member self.PerformanceCounter(check, name) = self.PerformanceCounter(check, name, [||])
    
    /// Get or create a performance counter monitor with custom tags
    member self.PerformanceCounter(counterConfig : PerformanceCounterConfig, name, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new MonitorAbsentFilter(new PerformanceCounterMonitor(x, counterConfig)))
#endif

    interface IMonitorFactory with
        member self.CumulativeCounter(name : string) = self.CumulativeCounter(name) :> ICounter<int64>
        member self.CumulativeCounter(name : string, tags : Tag array) = self.CumulativeCounter(name, tags) :> ICounter<int64>
        member self.Counter(name) = self.Counter(name) :> ICounter<int64>
        member self.Counter(name : string, tags : Tag array) = self.Counter(name, tags) :> ICounter<int64>
        member self.DoubleCounter(name) = self.DoubleCounter(name) :> ICounter<double>
        member self.DoubleCounter(name : string, tags : Tag array) = self.DoubleCounter(name, tags) :> ICounter<double>
        member self.MaxGauge(name : string) = self.MaxGauge(name) :> IGauge<int64>
        member self.MaxGauge(name : string, tags : Tag array) = self.MaxGauge(name, tags) :> IGauge<int64>
        member self.MinGauge(name : string) = self.MinGauge(name) :> IGauge<int64>
        member self.MinGauge(name : string, tags : Tag array) = self.MinGauge(name, tags) :> IGauge<int64>
        member self.MinMaxAvgGauge(name : string) = self.MinMaxAvgGauge(name) :> IGauge<float>
        member self.MinMaxAvgGauge(name : string, tags : Tag array) = self.MinMaxAvgGauge(name, tags) :> IGauge<float>
        member self.AverageGauge(name : string) = self.AverageGauge(name) :> IGauge<float>
        member self.AverageGauge(name : string, tags : Tag array) = self.AverageGauge(name, tags) :> IGauge<float>
        member self.LongGauge(name : string) = self.LongGauge(name) :> IGauge<int64>
        member self.LongGauge(name : string, tags : Tag array) = self.LongGauge(name, tags) :> IGauge<int64>
        member self.DoubleGauge(name : string) = self.DoubleGauge(name) :> IGauge<double>
        member self.DoubleGauge(name : string, tags : Tag array) = self.DoubleGauge(name, tags) :> IGauge<double>
        member self.DecimalGauge(name : string) = self.DecimalGauge(name) :> IGauge<decimal>
        member self.DecimalGauge(name : string, tags : Tag array) = self.DecimalGauge(name, tags) :> IGauge<decimal>
        member self.Timer(name) = self.Timer(name) :> ITimer
        member self.Timer(name : string, tags : Tag array) = self.Timer(name, tags) :> ITimer
        member self.SlaTimer(name : string, sla: TimeSpan) = self.SlaTimer(name, sla) :> ITimer
        member self.SlaTimer(name : string, sla : TimeSpan, tags : Tag array) = self.SlaTimer(name, sla, tags) :> ITimer
#if NET46
        member self.PerformanceCounter(check, name) = self.PerformanceCounter(check, name) :> IMonitor
        member self.PerformanceCounter(counterConfig : PerformanceCounterConfig, name, tags : Tag array) =  self.PerformanceCounter(counterConfig, name, tags) :> IMonitor
#endif


/// factory to create monitors sharing the same polling frequency
and MonitorFactory (monitorRegistry : IMonitorRegistry, DefaultTags) = 
    let mutable defaultTags = DefaultTags
    
    member self.WithAbsentFiltering = new AbsentMeasurementsFilterFactory(monitorRegistry, defaultTags, self)
    
    member self.UpdateDefaultTags(newTags) = 
        defaultTags <- newTags
        self.WithAbsentFiltering.UpdateDefaultTagsNonRecursive(newTags)

    member internal self.UpdateDefaultTagsNonRecursive(newTags) = 
        defaultTags <- newTags
    
    /// Get or create a CumulativeCounter
    member self.CumulativeCounter(name : string) = self.CumulativeCounter(name, [||])
    
    /// Get or create a CumulativeCounter with custom tags
    member self.CumulativeCounter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new CumulativeCounter(x))
    
    /// Get or create a PeakRateCounter
    member self.Counter(name) = self.Counter(name, [||])
    
    /// Get or create a PeakRateCounter with custom tags
    member self.Counter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new Counter(x))
    
    /// Get or create a DoubleCounter
    member self.DoubleCounter(name) = self.DoubleCounter(name, [||])
    
    /// Get or create a DoubleCounter
    member self.DoubleCounter(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new DoubleCounter(x))
    
    /// Get or create a Gauge
    member self.Gauge(name : string, getValue : Func<'T>) = self.Gauge(name, getValue, [||])
    
    /// Get or create a Gauge with custom tags
    member self.Gauge<'T>(name : string, getValue : Func<'T>, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new Gauge<'T>(x, getValue))
    
    /// Get or create a MaxGauge 
    member self.MaxGauge(name : string) = self.MaxGauge(name, [||])
    
    /// Get or create a MaxGaug with custom tags
    member self.MaxGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new MaxGauge(x))
    
    /// Get or create a MinGauge
    member self.MinGauge(name : string) = self.MinGauge(name, [||])
    
    /// Get or create a MinGauge with custom tags
    member self.MinGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new MinGauge(x))

    /// Get or create a MinMaxAvgGauge with custom tags
    member self.MinMaxAvgGauge(name : string) = self.MinMaxAvgGauge(name, [||])
    
    /// Get or create a MinMaxAvgGauge with custom tags
    member self.MinMaxAvgGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new MinMaxAvgGauge(x))
    
    /// Get or create a AverageGauge
    member self.AverageGauge(name : string) = self.AverageGauge(name, [||])
    
    /// Get or create a AverageGauge with custom tags
    member self.AverageGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new AverageGauge(x))
    
    /// Get or create a LongGauge
    member self.LongGauge(name : string) = self.LongGauge(name, [||])
    
    /// Get or create a LongGauge
    member self.LongGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new LongGauge(x))
    
    /// Get or create a DoubleGauge
    member self.DoubleGauge(name : string) = self.DoubleGauge(name, [||])
    
    /// Get or create a DoubleGauge with custom tags
    member self.DoubleGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new DoubleGauge(x))
    
    /// Get or create a DecimalGauge
    member self.DecimalGauge(name : string) = self.DecimalGauge(name, [||])
    
    /// Get or create a DecimalGauge with custom tags
    member self.DecimalGauge(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new DecimalGauge(x))
    
    /// Get or create a Timer, with a step size of 1 minute
    member self.Timer(name) = self.Timer(name, [||])
    
    /// Get or create a Timer with custom tags
    member self.Timer(name : string, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new Timer(x))

    /// Get or create a SLA-Timer, with a step size of 1 minute
    member self.SlaTimer(name : string, sla: TimeSpan) = self.SlaTimer(name, sla, [||])
    
    /// Get or create a SLA-Timer with custom tags
    member self.SlaTimer(name : string, sla : TimeSpan, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new SlaTimer(x, sla))

#if NET46
    /// Get or create a performance counter monitor
    member self.PerformanceCounter(check, name) = self.PerformanceCounter(check, name, [||])
    
    /// Get or create a performance counter monitor with custom tags
    member self.PerformanceCounter(counterConfig : PerformanceCounterConfig, name, tags : Tag array) = 
        let config = MonitorConfig.Build(name).WithTags(defaultTags).WithTags(tags)
        monitorRegistry.GetOrAdd(config, fun x -> new PerformanceCounterMonitor(x, counterConfig))
#endif

    interface IMonitorFactory with
        member self.CumulativeCounter(name : string) = self.CumulativeCounter(name) :> ICounter<int64>
        member self.CumulativeCounter(name : string, tags : Tag array) = self.CumulativeCounter(name, tags) :> ICounter<int64>
        member self.Counter(name) = self.Counter(name) :> ICounter<int64>
        member self.Counter(name : string, tags : Tag array) = self.Counter(name, tags) :> ICounter<int64>
        member self.DoubleCounter(name) = self.DoubleCounter(name) :> ICounter<double>
        member self.DoubleCounter(name : string, tags : Tag array) = self.DoubleCounter(name, tags) :> ICounter<double>
        member self.MaxGauge(name : string) = self.MaxGauge(name) :> IGauge<int64>
        member self.MaxGauge(name : string, tags : Tag array) = self.MaxGauge(name, tags) :> IGauge<int64>
        member self.MinGauge(name : string) = self.MinGauge(name) :> IGauge<int64>
        member self.MinGauge(name : string, tags : Tag array) = self.MinGauge(name, tags) :> IGauge<int64>
        member self.MinMaxAvgGauge(name : string) = self.MinMaxAvgGauge(name) :> IGauge<float>
        member self.MinMaxAvgGauge(name : string, tags : Tag array) = self.MinMaxAvgGauge(name, tags) :> IGauge<float>
        member self.AverageGauge(name : string) = self.AverageGauge(name) :> IGauge<float>
        member self.AverageGauge(name : string, tags : Tag array) = self.AverageGauge(name, tags) :> IGauge<float>
        member self.LongGauge(name : string) = self.LongGauge(name) :> IGauge<int64>
        member self.LongGauge(name : string, tags : Tag array) = self.LongGauge(name, tags) :> IGauge<int64>
        member self.DoubleGauge(name : string) = self.DoubleGauge(name) :> IGauge<double>
        member self.DoubleGauge(name : string, tags : Tag array) = self.DoubleGauge(name, tags) :> IGauge<double>
        member self.DecimalGauge(name : string) = self.DecimalGauge(name) :> IGauge<decimal>
        member self.DecimalGauge(name : string, tags : Tag array) = self.DecimalGauge(name, tags) :> IGauge<decimal>
        member self.Timer(name) = self.Timer(name) :> ITimer
        member self.Timer(name : string, tags : Tag array) = self.Timer(name, tags) :> ITimer
        member self.SlaTimer(name : string, sla: TimeSpan) = self.SlaTimer(name, sla) :> ITimer
        member self.SlaTimer(name : string, sla : TimeSpan, tags : Tag array) = self.SlaTimer(name, sla, tags) :> ITimer
#if NET46
        member self.PerformanceCounter(check, name) = self.PerformanceCounter(check, name) :> IMonitor
        member self.PerformanceCounter(counterConfig : PerformanceCounterConfig, name, tags : Tag array) =  self.PerformanceCounter(counterConfig, name, tags) :> IMonitor
#endif