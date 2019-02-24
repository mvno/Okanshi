namespace Okanshi

open System
open System.Diagnostics


/// A filter decorator for imonitors.
/// The wrapped monitor only returns values to the poller in case a change has been registered
/// With this you avoid sending 0-value measurements that normally is sent when no measurements are registered
type MonitorAbsentFilter(inner : IMonitor) =
    let syncRoot = new obj()
    let isFloatGtZero(o :obj) : bool = 
        match o with 
        | :? float as f -> f > (float 0)
        | _ -> false

    member __.GetValues() = 
         lock syncRoot (fun() -> 
            inner.GetValues() |> Seq.where(fun x -> x.Name <> "value" || (x.Name="value" && isFloatGtZero x.Value)) |> Seq.cast)
    member __.Config = inner.Config
    member __.GetValuesAndReset() = 
        lock syncRoot (fun() -> 
            inner.GetValuesAndReset() |> Seq.where(fun x -> x.Name <> "value" || (x.Name="value" && isFloatGtZero x.Value)) |> Seq.cast)

    interface IMonitor with
        member self.GetValues() = self.GetValues()
        member self.Config = self.Config
        member self.GetValuesAndReset() = self.GetValuesAndReset()


/// A filter decorator for gauges.
/// The wrapped monitor only returns values to the poller in case a change has been registered
/// With this you avoid sending 0-value measurements that normally is sent when no measurements are registered
type GaugeAbsentFilter<'T>(inner : IGauge<'T>) =
    let mutable hasChanged = false
    let syncRoot = new obj()

    member __.Set(newValue) = 
        lock syncRoot (fun() -> 
            hasChanged <- true
            inner.Set(newValue))
    member __.GetValues() = 
        lock syncRoot (fun() -> 
            match hasChanged with
            | false -> Seq.empty<IMeasurement>
            | true -> 
                hasChanged <- false
                inner.GetValues() |> Seq.cast)
    member __.Config = inner.Config
    member __.Reset() = 
        lock syncRoot (fun() ->
            hasChanged <- true
            inner.Reset())
    member __.GetValuesAndReset() = 
        lock syncRoot (fun() -> 
            match hasChanged with
            | false -> Seq.empty<IMeasurement>
            | true -> 
                hasChanged <- false
                inner.GetValuesAndReset() |> Seq.cast)

    interface IGauge<'T> with
        member self.Set(newValue) = self.Set(newValue)
        member self.GetValues() = self.GetValues()
        member self.Config = self.Config
        member self.Reset() = self.Reset()
        member self.GetValuesAndReset() = self.GetValuesAndReset()


/// A filter decorator for counters.
/// The wrapped monitor only returns values to the poller in case a change has been registered
/// With this you avoid sending 0-value measurements that normally is sent when no measurements are registered
type CounterAbsentFilter<'T>(inner : ICounter<'T>) =
    let mutable hasChanged = false
    let syncRoot = new obj()
    
    member __.Increment() = 
        lock syncRoot (fun() -> 
            hasChanged <- true
            inner.Increment())
    member __.Increment(amount) = 
        lock syncRoot (fun() -> 
            hasChanged <- true
            inner.Increment(amount))
    member __.GetValues() =
        lock syncRoot (fun() -> 
            match hasChanged with
            | false -> Seq.empty<IMeasurement>
            | true -> 
                hasChanged <- false
                inner.GetValues() |> Seq.cast)
    member __.Config = inner.Config
    member __.GetValuesAndReset() = 
        lock syncRoot (fun() -> 
            match hasChanged with
            | false -> Seq.empty<IMeasurement>
            | true -> 
                hasChanged <- false
                inner.GetValuesAndReset() |> Seq.cast)

    interface ICounter<'T> with
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)
        member self.GetValues() = self.GetValues()
        member self.Config = self.Config
        member self.GetValuesAndReset() = self.GetValuesAndReset()


/// A filter decorator for timers.
/// The wrapped monitor only returns values to the poller in case a change has been registered
/// With this you avoid sending 0-value measurements that normally is sent when no measurements are registered
type TimerAbsentFilter(inner : ITimer) =
    let mutable hasChanged = false
    let syncRoot = new obj()
    
    member __.Record(f : Func<'T>) = 
        lock syncRoot (fun() -> 
            hasChanged <- true
            inner.Record(f))
    member __.Record(f : Action) = 
        lock syncRoot (fun() -> 
            hasChanged <- true
            inner.Record(f))
    member __.GetValues() = 
        lock syncRoot (fun() -> 
            match hasChanged with
            | false -> Seq.empty<IMeasurement>
            | true -> 
                hasChanged <- false
                inner.GetValues() |> Seq.cast)
    member __.Config = inner.Config
    member __.Start() = inner.Start()
    member __.RegisterElapsed(elapsed : Stopwatch) = 
        lock syncRoot (fun() -> 
            hasChanged <- true
            inner.RegisterElapsed(elapsed))
    member __.Register(elapsed : TimeSpan) = 
        lock syncRoot (fun() -> 
            hasChanged <- true
            inner.Register(elapsed))
    member __.GetValuesAndReset() = 
        lock syncRoot (fun() -> 
            match hasChanged with
            | false -> Seq.empty<IMeasurement>
            | true -> 
                hasChanged <- false
                inner.GetValuesAndReset() |> Seq.cast)

    interface ITimer with
        member self.Record(f : Func<'T>) = self.Record(f)
        member self.Record(f : Action) = self.Record(f)
        member self.GetValues() = self.GetValues()
        member self.Config = self.Config
        member self.Start() = self.Start()
        member self.RegisterElapsed(elapsed : Stopwatch) = self.RegisterElapsed(elapsed)
        member self.Register(elapsed : TimeSpan) = self.Register(elapsed)
        member self.GetValuesAndReset() = self.GetValuesAndReset()
