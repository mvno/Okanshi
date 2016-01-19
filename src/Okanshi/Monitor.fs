namespace Okanshi

open System
open System.Collections.Generic
open System.Threading
open System.Collections.Concurrent

/// Tuple representing a key used to identify a monitor
type MonitorKey = (string * Type)
/// Type representing a monitor factory
type MonitorFactory = unit -> IMonitor

type OkanshiMonitorMessage =
    private
    | GetMonitor of MonitorKey * MonitorFactory * AsyncReplyChannel<IMonitor>

/// Static use of monitors
[<AbstractClass; Sealed>]
type OkanshiMonitor private() =
    static let cancellationTokenSource = new CancellationTokenSource()
    static let cancellationToken = cancellationTokenSource.Token

    static let monitorAgentLoop (inbox : MailboxProcessor<OkanshiMonitorMessage>) =
        let registeredMonitors = new Dictionary<MonitorKey, IMonitor>()
        let monitorRegistry = DefaultMonitorRegistry.Instance
        let rec loop () =
            async {
                let! msg = inbox.Receive()
                match msg with
                | GetMonitor (key, factory, reply) ->
                    match registeredMonitors.TryGetValue(key) with
                    | success, monitor when success -> reply.Reply(monitor)
                    | success, _ when not success ->
                        let monitor = factory()
                        registeredMonitors.Add(key, monitor)
                        monitorRegistry.Register(monitor)
                        reply.Reply(monitor)
                    | _ -> ()

                return! loop()
            }
        loop()
    static let monitorAgent = MailboxProcessor.Start(monitorAgentLoop, cancellationToken)

    static let tagDictionary = new ConcurrentDictionary<Tag, byte>()

    /// Gets the default tags added to all monitors created
    static member DefaultTags with get() = tagDictionary.Keys |> Seq.toArray
    /// Sets the default tags added to all monitors created
    static member DefaultTags
        with set(value : Tag array) =
            tagDictionary.Clear()
            value |> Seq.iter (fun x -> tagDictionary.TryAdd(x, byte 0) |> ignore)
    /// Gets the monitor key used to identify monitors
    static member GetMonitorKey(name : string, monitorType : Type) = (name, monitorType)

    /// Get or add a BasicCounter
    static member BasicCounter(name : string) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<BasicCounter>)
        let factory = fun () -> (new BasicCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags)) :> IMonitor)
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> BasicCounter

    /// Get or add a StepCounter
    static member StepCounter(name : string, step) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<StepCounter>)
        let factory = fun () -> (new StepCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags), step) :> IMonitor)
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> StepCounter

    /// Get or add a PeakRateCounter
    static member PeakRateCounter(name : string, step) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<PeakRateCounter>)
        let factory = fun () -> (new PeakRateCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags), step) :> IMonitor)
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> PeakRateCounter

    /// Get or add a DoubleCounter
    static member DoubleCounter(name : string, step) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<DoubleCounter>)
        let factory = fun () -> (new DoubleCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags), step) :> IMonitor)
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> DoubleCounter

    /// Get or add a BasicGauge
    static member BasicGauge<'T>(name : string, getValue : Func<'T>) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<BasicGauge<'T>>)
        let factory = fun () -> (new BasicGauge<'T>(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags), getValue)) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> BasicGauge<'T>

    /// Get or add a MaxGauge
    static member MaxGauge(name : string) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<MaxGauge>)
        let factory = fun () -> (new MaxGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags))) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> MaxGauge

    /// Get or add a MinGauge
    static member MinGauge(name : string) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<MinGauge>)
        let factory = fun () -> (new MaxGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags))) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> MinGauge

    /// Get or add a LongGauge
    static member LongGauge(name : string) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<LongGauge>)
        let factory = fun () -> (new LongGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags))) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> LongGauge

    /// Get or add a DoubleGauge
    static member DoubleGauge(name : string) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<DoubleGauge>)
        let factory = fun () -> (new DoubleGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags))) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> DoubleGauge

    /// Get or add a DecimalGauge
    static member DecimalGauge(name : string) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<DecimalGauge>)
        let factory = fun () -> (new DecimalGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags))) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> DecimalGauge

    /// Get or add a BasicTimer
    static member BasicTimer(name : string, step) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<BasicTimer>)
        let factory = fun () -> (new BasicTimer(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags), step)) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> BasicTimer
