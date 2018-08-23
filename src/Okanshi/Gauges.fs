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
    member __.GetValues() = seq { yield Measurement("value", getValue.Invoke()) }

    /// Gets the current value
    member __.GetValueAs(name : string) = Measurement(name, getValue.Invoke())
    
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    
    /// Gets the value and resets the monitor
    member self.GetValuesAndReset() = self.GetValues()
    
    interface IMonitor with
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast

/// Gauge that keeps track of the maximum value seen since the last reset. Updates should be
/// non-negative, the initial value is 0.
type MaxGauge(config : MonitorConfig) = 
    let value = new AtomicLong()
    
    let rec exchangeValue newValue = 
        let originalValue = value.Get()
        if originalValue < newValue then 
            let result = value.CompareAndSet(newValue, originalValue)
            if result <> originalValue then exchangeValue newValue
    
    /// Sets the value
    member __.Set(newValue) = exchangeValue newValue
    
    /// Gets the current value
    member __.GetValues() = seq { yield Measurement("value", value.Get()) }

    /// Gets the current value
    member __.GetValueAs(name : string) = Measurement(name, value.Get()) 
    
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    
    /// Reset the gauge
    member __.Reset() = value.Set(0L)
    
    /// Gets the value and resets the monitor
    member __.GetValuesAndReset() =
        let value = value.GetAndSet(0L)
        seq { yield Measurement("value", value) }

    interface IGauge<int64> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.Reset() = self.Reset()
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast

/// Gauge that keeps track of the minimum value seen since the last reset. Updates should be
/// non-negative, the initial value is 0.
type MinGauge(config : MonitorConfig) = 
    let value = new AtomicLong()
    
    let rec exchangeValue newValue = 
        let originalValue = value.Get()
        if originalValue = 0L || originalValue > newValue then 
            let result = value.CompareAndSet(newValue, originalValue)
            if result <> originalValue then exchangeValue newValue
    
    /// Sets the value
    member __.Set(newValue) = exchangeValue newValue
    
    /// Gets the current value
    member __.GetValues() = seq { yield Measurement("value", value.Get()) }

    /// Gets the current value
    member __.GetValueAs(name : string) = Measurement(name, value.Get()) 
    
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    
    /// Reset the gauge
    member __.Reset() = value.Set(0L)
    
    /// Gets the value and resets the monitor
    member __.GetValuesAndReset() = seq { yield Measurement("value", value.GetAndSet(0L)) }
    
    interface IGauge<int64> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.Reset() = self.Reset()
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast

/// A gauge the reports a long value
type LongGauge(config : MonitorConfig) = 
    let value = new AtomicLong()
    
    /// Sets the value
    member __.Set(newValue) = value.Set(newValue)
    
    /// Gets the current value
    member __.GetValues() = seq { yield Measurement("value", value.Get()) }
    
    /// Gets the current value
    member __.GetValueAs(name : string) = Measurement(name, value.Get())
    
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    
    /// Reset the gauge
    member __.Reset() = value.Set(0L)
    
    /// Gets the value and resets the monitor
    member __.GetValuesAndReset() = seq { yield Measurement("value", value.GetAndSet(0L)) }
    
    interface IGauge<int64> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.Reset() = self.Reset()
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast

/// A gauge that reports a double value
type DoubleGauge(config : MonitorConfig) = 
    let value = new AtomicDouble()
    
    /// Sets the value
    member __.Set(newValue) = value.Set(newValue)
    
    /// Gets the current value
    member __.GetValues() = seq { yield Measurement("value", value.Get()) }

    /// Gets the current value
    member __.GetValueAs(name : string) = Measurement(name, value.Get())
    
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    
    /// Reset the gauge
    member __.Reset() = value.Set(0.0)
    
    /// Gets the value and resets the monitor
    member __.GetValuesAndReset() =
        let value = value.GetAndSet(0.0)
        seq { yield Measurement("value", value) }
    
    interface IGauge<double> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.Reset() = self.Reset()
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast

/// A gauge that reports a decimal value
type DecimalGauge(config : MonitorConfig) = 
    let value = new AtomicDecimal()
    
    /// Sets the value
    member __.Set(newValue) = value.Set(newValue)
    
    /// Gets the current value
    member __.GetValues() = seq { yield Measurement("value", value.Get()) }

    /// Gets the current value
    member __.GetValueAs(name : string) = Measurement(name, value.Get())
    
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    
    /// Reset the gauge
    member __.Reset() = value.Set(0m)
    
    /// Gets the value and resets the monitor
    member __.GetValuesAndReset() = seq { yield Measurement("value", value.GetAndSet(0m)) }
    
    interface IGauge<decimal> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.Reset() = self.Reset()
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast

/// Gauge that keeps track of the average value since last reset. Initial value is 0.
type AverageGauge(config : MonitorConfig) = 
    let mutable value = 0.0
    let mutable count = 0L
    let syncRoot = new obj()
    
    let rec updateAverage v = 
        count <- count + 1L
        value <- ((value * ((count - 1L) |> float)) + v) / (count |> float)
    
    let getValue'() = seq { yield Measurement("value", value) } |> Seq.toList
    let resetValue'() =
        count <- 0L
        value <- 0.0
    
    let getValueAndReset'() = 
        let result = getValue'()
        resetValue'()
        result |> List.toSeq
    
    /// Sets the value
    member __.Set(newValue) = lockWithArg syncRoot newValue updateAverage
    
    /// Gets the current value
    member __.GetValues() = Lock.lock syncRoot (fun () -> getValue'() |> List.toSeq)

    member __.GetValueAs(name : string) = Lock.lock syncRoot (fun () -> Measurement(name, value))
    
    /// Gets the monitor configuration
    member __.Config = config.WithTag(DataSourceType.Gauge)
    
    /// Reset the gauge
    member __.Reset() = Lock.lock syncRoot resetValue'
    
    /// Gets the value and resets the monitor
    member __.GetValuesAndReset() = Lock.lock syncRoot getValueAndReset'
    
    interface IGauge<float> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValues() = self.GetValues() |> Seq.cast
        member self.Config = self.Config
        member self.Reset() = self.Reset()
        member self.GetValuesAndReset() = self.GetValuesAndReset() |> Seq.cast
