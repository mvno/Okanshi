namespace Okanshi

open System
open System.Collections.Generic
open System.Threading
open System.Collections.Concurrent

/// Tuple representing a key used to identify a monitor
type MonitorKey = (string * Type * Tag array)
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
    static let defaultStep = TimeSpan.FromMinutes(float 1)

    /// Gets the default tags added to all monitors created
    static member DefaultTags with get() = tagDictionary.Keys |> Seq.toArray
    /// Sets the default tags added to all monitors created
    static member DefaultTags
        with set(value : Tag array) =
            tagDictionary.Clear()
            value |> Seq.iter (fun x -> tagDictionary.TryAdd(x, byte 0) |> ignore)
    /// Gets the monitor key used to identify monitors
    static member GetMonitorKey(name : string, monitorType : Type, tags : Tag array) =
        (name, monitorType, tags)

    /// Get or add a BasicCounter
    static member BasicCounter(name : string) = OkanshiMonitor.BasicCounter(name, [||])

    /// Get or add a BasicCounter with custom tags
    static member BasicCounter(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<BasicCounter>, tags)
        let factory = fun () -> (new BasicCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags)) :> IMonitor)
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> BasicCounter

    /// Get or add a StepCounter, with a step size of 1 minute
    static member StepCounter(name) = OkanshiMonitor.StepCounter(name, defaultStep, [||])
    
    /// Get or add a StepCounter with custom tags and a step size of 1 minute
    static member StepCounter(name, tags : Tag array) = OkanshiMonitor.StepCounter(name, defaultStep, tags)

    /// Get or add a StepCounter
    static member StepCounter(name : string, step, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<StepCounter>, tags)
        let factory = fun () -> (new StepCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), step) :> IMonitor)
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> StepCounter

    /// Get or add a PeakRateCounter, with a step size of 1 minute
    static member PeakRateCounter(name) = OkanshiMonitor.PeakRateCounter(name, defaultStep, [||])
    
    /// Get or add a PeakRateCounter with custom tags and a step size of 1 minute
    static member PeakRateCounter(name, tags : Tag array) = OkanshiMonitor.PeakRateCounter(name, defaultStep, tags)

    /// Get or add a PeakRateCounter
    static member PeakRateCounter(name : string, step, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<PeakRateCounter>, tags)
        let factory = fun () -> (new PeakRateCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), step) :> IMonitor)
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> PeakRateCounter

    /// Get or add a DoubleCounter, with a step size of 1 minute
    static member DoubleCounter(name) = OkanshiMonitor.DoubleCounter(name, defaultStep, [||])

    /// Get or add a DoubleCounter with custom tags and a step size of 1 minute
    static member DoubleCounter(name, tags : Tag array) = OkanshiMonitor.DoubleCounter(name, defaultStep, tags)

    /// Get or add a DoubleCounter
    static member DoubleCounter(name : string, step, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<DoubleCounter>, tags)
        let factory = fun () -> (new DoubleCounter(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), step) :> IMonitor)
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> DoubleCounter

    /// Get or add a BasicGauge
    static member BasicGauge(name : string, getValue : Func<'T>) = OkanshiMonitor.BasicGauge(name, getValue, [||])

    /// Get or add a BasicGauge with custom tags
    static member BasicGauge<'T>(name : string, getValue : Func<'T>, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<BasicGauge<'T>>, tags)
        let factory = fun () -> (new BasicGauge<'T>(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), getValue)) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> BasicGauge<'T>

    /// Get or add a MaxGauge 
    static member MaxGauge(name : string) = OkanshiMonitor.MaxGauge(name, [||])

    /// Get or add a MaxGaug with custom tags
    static member MaxGauge(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<MaxGauge>, tags)
        let factory = fun () -> (new MaxGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> MaxGauge

    /// Get or add a MinGauge
    static member MinGauge(name : string) = OkanshiMonitor.MinGauge(name, [||])

    /// Get or add a MinGauge with custom tags
    static member MinGauge(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<MinGauge>, tags)
        let factory = fun () -> (new MaxGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> MinGauge

    /// Get or add a LongGauge
    static member LongGauge(name : string) = OkanshiMonitor.LongGauge(name, [||])

    /// Get or add a LongGauge
    static member LongGauge(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<LongGauge>, tags)
        let factory = fun () -> (new LongGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> LongGauge

    /// Get or add a DoubleGauge
    static member DoubleGauge(name : string) = OkanshiMonitor.DoubleGauge(name, [||])
    
    /// Get or add a DoubleGauge with custom tags
    static member DoubleGauge(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<DoubleGauge>, tags)
        let factory = fun () -> (new DoubleGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> DoubleGauge

    /// Get or add a DecimalGauge
    static member DecimalGauge(name : string) = OkanshiMonitor.DecimalGauge(name, [||])

    /// Get or add a DecimalGauge with custom tags
    static member DecimalGauge(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<DecimalGauge>, tags)
        let factory = fun () -> (new DecimalGauge(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> DecimalGauge

    /// Get or add a BasicTimer, with a step size of 1 minute
    static member BasicTimer(name) = OkanshiMonitor.BasicTimer(name, defaultStep, [||])

    /// Get or add a BasicTimer with custom tags and a step size of 1 minute
    static member BasicTimer(name, tags : Tag array) = OkanshiMonitor.BasicTimer(name, defaultStep, tags)

    /// Get or add a BasicTimer
    static member BasicTimer(name : string, step, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<BasicTimer>, tags)
        let factory = fun () -> (new BasicTimer(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags), step)) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> BasicTimer

    /// Get or add a DurationTimer
    static member DurationTimer(name) = OkanshiMonitor.DurationTimer(name, [||])

    /// Get or add a DurationTimer with custom tags
    static member DurationTimer(name : string, tags : Tag array) =
        let monitorKey = OkanshiMonitor.GetMonitorKey(name, typeof<DurationTimer>, tags)
        let factory = fun () -> (new DurationTimer(MonitorConfig.Build(name).WithTags(OkanshiMonitor.DefaultTags).WithTags(tags))) :> IMonitor
        monitorAgent.PostAndReply(fun reply -> GetMonitor(monitorKey, factory, reply)) :?> BasicTimer