namespace Okanshi

open System
open Okanshi.Helpers

/// Monitor type that provides the current value, fx. the percentage of disk space used
type IGauge<'T> =
    inherit IMonitor
    /// Sets the value
    abstract Set : 'T -> unit
    /// Resets the gauge
    abstract Reset : unit -> unit

/// A gauge implemenation that invokes a func to get the current value
type BasicGauge<'T>(config : MonitorConfig, getValue : Func<'T>) =
    /// Gets the current value
    member __.GetValue() = getValue.Invoke()
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)

    interface IMonitor with
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config

/// Gauge that keeps track of the maximum value seen since the last reset. Updates should be
/// non-negative, the initial value is 0.
type MaxGauge(config : MonitorConfig) =
    let value = new AtomicLong()

    /// Sets the value
    member __.Set(newValue) =
        let rec exchangeValue () =
            let originalValue = value.Get()
            if originalValue < newValue then
                let result = value.CompareAndSet(newValue, originalValue)
                if result <> originalValue then exchangeValue()
        exchangeValue()

    /// Gets the current value
    member __.GetValue() = value.Get()
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    /// Reset the gauge
    member __.Reset() = value.Set(0L)

    interface IGauge<int64> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config
        member self.Reset() = self.Reset()

/// Gauge that keeps track of the minimum value seen since the last reset. Updates should be
/// non-negative, the initial value is 0.
type MinGauge(config : MonitorConfig) =
    let value = new AtomicLong()

    /// Sets the value
    member __.Set(newValue) =
        let rec exchangeValue () =
            let originalValue = value.Get()
            if originalValue = 0L || originalValue > newValue then
                let result = value.CompareAndSet(newValue, originalValue)
                if result <> originalValue then exchangeValue()
        exchangeValue()

    /// Gets the current value
    member __.GetValue() = value.Get()
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    /// Reset the gauge
    member __.Reset() = value.Set(0L)

    interface IGauge<int64> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config
        member self.Reset() = self.Reset()

/// A gauge the reports a long value
type LongGauge(config : MonitorConfig) =
    let value = new AtomicLong()

    /// Sets the value
    member __.Set(newValue) = value.Set(newValue)
    /// Gets the current value
    member __.GetValue() = value.Get()
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    /// Reset the gauge
    member __.Reset() = value.Set(0L)

    interface IGauge<int64> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config
        member self.Reset() = self.Reset()

/// A gauge that reports a double value
type DoubleGauge(config : MonitorConfig) =
    let value = new AtomicDouble()

    /// Sets the value
    member __.Set(newValue) = value.Set(newValue)
    /// Gets the current value
    member __.GetValue() = value.Get()
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    /// Reset the gauge
    member __.Reset() = value.Set(0.0)

    interface IGauge<double> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config
        member self.Reset() = self.Reset()

/// A gauge that reports a decimal value
type DecimalGauge(config : MonitorConfig) =
    let value = new AtomicDecimal()

    /// Sets the value
    member __.Set(newValue) = value.Set(newValue)
    /// Gets the current value
    member __.GetValue() = value.Get()
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    /// Reset the gauge
    member __.Reset() = value.Set(0m)

    interface IGauge<decimal> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config
        member self.Reset() = self.Reset()

/// Gauge that keeps track of the average value since last reset. Initial value is 0.
type AverageGauge(config : MonitorConfig, step : TimeSpan, clock : IClock) =
    let value = new StepLong(0L, step, clock)
    let count = new StepLong(0L, step, clock)
    let syncRoot = new obj()

    let rec updateAverage v =
        clock.Freeze()
        let count = count.Increment(1L)
        let original = value.GetCurrent().Get()
        value.GetCurrent().Set(((original * (count - 1L)) + v) / count)
        clock.Unfreeze()

    new (config, step) = AverageGauge(config, step, new SystemClock())

    /// Sets the value
    member __.Set(newValue) =
        lockWithArg syncRoot newValue updateAverage

    /// Gets the current value
    member __.GetValue() = value.Poll().Value
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    /// Reset the gauge
    member __.Reset() = value.GetCurrent().Set(0L)

    interface IGauge<int64> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValue() = self.GetValue() :> obj
        member self.Config = self.Config
        member self.Reset() = self.Reset()