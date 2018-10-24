namespace Okanshi.Endpoint

open System
open System.Threading
open System.Xml
open System.Xml.Linq
open Newtonsoft.Json
open Okanshi

/// Dependecy information
type Dependency =
    {
        /// Name of the dependency
        Name : string;
        /// The dependency version
        Version : string;
    }

/// Pacakge information
type Package =
    {
        /// Id of the package
        Id : string;
        /// The package version
        Version : string;
    }

/// Response object
type Response<'T> = { Version : string; Data : 'T }

/// Api options
type EndpointOptions() =
    /// Enable CORS request. Default value is false
    member val EnableCors = false with get, set
    /// The HTTP prefix used as endpoint. Default value is "http://+:13004/"
    member val HttpPrefix = "http://+:13004/" with get, set
    /// The polling interval. Default value is 1 minute
    member val PollingInterval = TimeSpan.FromMinutes(float 1) with get, set
    /// The number of samples to store in memory. Default value is 100
    member val NumberOfSamplesToStore = 100 with get, set
    /// Should metrics be collected when process is exiting. Default value is false.
    member val CollectMetricsOnProcessExit = false with get, set

/// Controls the monitor endpoint. This needs to be started to allow querying the endpoint for information.
type MonitorEndpoint(options : EndpointOptions, jsonSerialize : Func<Object, string>) =
    let jsonSerializeObject = jsonSerialize
    let listener = new System.Net.HttpListener()
    let observer = new MemoryMetricObserver(new MetricMonitorRegistryPoller(DefaultMonitorRegistry.Instance, options.PollingInterval, options.CollectMetricsOnProcessExit), options.NumberOfSamplesToStore)
    let cancellationTokenSource = new CancellationTokenSource()
    let cancellationToken = cancellationTokenSource.Token

    let getObservations (observer : IProcessingMetricObserver) =
        observer.GetObservations()
    
    /// Create the endpoint with default values
    new (jsonSerialize : Func<Object, string>) = MonitorEndpoint(new EndpointOptions(), jsonSerialize)

    /// Start endpoint and using the default metrics registry
    member __.Start() =
        let sendResponse (context : System.Net.HttpListenerContext) content =
            try
                use response = context.Response
                if options.EnableCors then
                    response.AddHeader("Access-Control-Allow-Origin", "*")
                    response.AddHeader("Access-Control-Allow-Methods", "*")
                let serializedContent = content |> jsonSerializeObject.Invoke   
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
                elif context.Request.Url.AbsolutePath.EndsWith("/packages", StringComparison.OrdinalIgnoreCase) then
                    if System.IO.File.Exists("packages.config") then
                        let xname s = XName.Get(s)
                        let packages =
                            try
                                (XDocument.Load "packages.config")
                                    .Descendants(xname "package")
                                        |> Seq.map(fun p -> {
                                                                Id = p.Attribute(xname "id").Value;
                                                                Version = p.Attribute(xname "version").Value
                                                            })
                            with
                            | _ -> Seq.empty<Package>
                        { Version = "1"; Data = packages |> Seq.toArray } |> sendResponse context
                    else
                        { Version = "1"; Data = [] } |> sendResponse context
                else
                    { Version = "2"; Data = observer |> getObservations } |> sendResponse context
        }
        Async.Start(server, cancellationToken)

    /// Stops the endpoint
    member __.Stop() =
        cancellationTokenSource.Cancel()