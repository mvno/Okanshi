namespace Okanshi

open System
open System.Threading
open Newtonsoft.Json

/// Dependecy information
type Dependency =
    {
        /// Name of the dependency
        Name : string;
        /// The dependency version
        Version : string;
    }

/// Api options
type MonitorApiOptions() =
    /// Enable CORS request. Default value is false
    member val EnableCors = false with get, set
    /// The HTTP prefix used as endpoint. Default value is "http://+:13004/"
    member val HttpPrefix = "http://+:13004/" with get, set
    /// The polling interval. Default value is 1 minute
    member val PollingInterval = TimeSpan.FromMinutes(float 1)

/// Response object
type Response<'T> = { Version : string; Data : 'T }

type MonitorApi(options : MonitorApiOptions) =
    let listener = new System.Net.HttpListener()
    let observer = new MemoryMetricObserver(new MetricMonitorRegistryPoller(DefaultMonitorRegistry.Instance, options.PollingInterval), 100)
    let cancellationTokenSource = new CancellationTokenSource()
    let cancellationToken = cancellationTokenSource.Token
    
    /// Create the API with default values
    new () = MonitorApi(new MonitorApiOptions())

    /// Start API and using the default metrics registry
    member __.Start() =
        let sendResponse (context : System.Net.HttpListenerContext) content =
            try
                use response = context.Response
                if options.EnableCors then
                    response.AddHeader("Access-Control-Allow-Origin", "*")
                    response.AddHeader("Access-Control-Allow-Methods", "*")
                let serializedContent = content |> JsonConvert.SerializeObject
                let buffer = serializedContent |> System.Text.Encoding.UTF8.GetBytes
                response.ContentLength64 <- buffer.LongLength
                use output = response.OutputStream
                output.Write(buffer, 0, buffer.Length)
                output.Close()
                response.Close()
            with
                | _ -> ()

        let server = async {
            if listener.IsListening then invalidOp "Already started"
            listener.Prefixes.Add(options.HttpPrefix)
            listener.Start()
            while true do
                let! context = listener.GetContextAsync() |> Async.AwaitTask
                if context.Request.Url.AbsolutePath.EndsWith("/healthchecks", StringComparison.OrdinalIgnoreCase) then
                    { Version = "1"; Data = HealthChecks.RunAll() } |> sendResponse context
                elif context.Request.Url.AbsolutePath.EndsWith("/dependencies", StringComparison.OrdinalIgnoreCase) then
                    let dependencies =
                        List.ofSeq([for dep in AppDomain.CurrentDomain.GetAssemblies() -> { Name = dep.GetName().Name; Version = dep.GetName().Version.ToString() }])
                        |> List.toArray
                    { Version = "1"; Data = dependencies } |> sendResponse context
                else
                    { Version = "1"; Data = observer.GetObservations() } |> sendResponse context
        }
        Async.Start(server, cancellationToken)

    member __.Stop() =
        cancellationTokenSource.Cancel()
