namespace Okanshi

open System.Diagnostics

/// Performance counter configuration
type PerformanceCounterConfig = 
    { /// The category
      Category : string
      /// The category
      Counter : string
      /// The category
      Instance : string }
    
    /// Create performance counter config without an instance
    static member Build(category, counter) = 
        { Category = category
          Counter = counter
          Instance = "" }
    
    /// Create performance counter config with an instance
    static member Build(category, counter, instance) = 
        { Category = category
          Counter = counter
          Instance = instance }

/// Used to monitor Windows performance counters. Be aware that some performance counters, requires multiple readings before
/// returning any value.
type PerformanceCounterMonitor(registry : IMonitorRegistry, monitorConfig : MonitorConfig, performanceCounterConfig) = 
    let performanceCounter = 
        new PerformanceCounter(performanceCounterConfig.Category, performanceCounterConfig.Counter, 
                               performanceCounterConfig.Instance, true)
    let gauge = new BasicGauge<_>(monitorConfig, fun () -> performanceCounter.NextValue())
    do registry.Register(gauge)
    new(monitorConfig, performanceCounterConfig) = 
        PerformanceCounterMonitor(DefaultMonitorRegistry.Instance, monitorConfig, performanceCounterConfig)
    
    /// Gets the performance counter value
    member __.GetValue() = gauge.GetValue()
    
    /// Gets the monitor config
    member __.Config = gauge.Config

    /// Gets the value and resets the monitor
    member __.GetValueAndReset() = gauge.GetValueAndReset()
    
    interface IMonitor with
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config
