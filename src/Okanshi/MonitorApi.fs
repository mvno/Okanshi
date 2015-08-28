namespace Okanshi

open System
open System.Net
open Newtonsoft.Json

/// Api options
type MonitorApiOptions() =
    /// Enable CORS request. Default value is false
    member val EnableCors = false with get, set
    /// The HTTP prefix used as endpoint. Default value is "http://+:13004/"
    member val HttpPrefix = "http://+:13004/" with get, set

/// Response object
type Response<'T> = { Version : string; Data : 'T }

/// The monitor API
type MonitorApi(options : MonitorApiOptions) =
    let listener = new HttpListener()
    let mutable monitor : Monitor.Monitor option = None

    /// Create the API with default values
    new () =
        MonitorApi(new MonitorApiOptions())

    /// Start API and monitoring using the default values
    member self.Start() =
        self.Start(new MonitorOptions())

    /// Start API and monitoring using the provided options
    member __.Start(monitorOptions) =
        if monitor.IsSome then invalidOp "Already started"
        monitor <- monitorOptions |> Monitor.start |> Some
        CSharp.Monitor.SetMonitor(monitor.Value)
        let sendResponse (context : HttpListenerContext) content =
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
                    Monitor.runHealthChecks() |> sendResponse context
                elif context.Request.Url.AbsolutePath.EndsWith("/dependencies", StringComparison.OrdinalIgnoreCase) then
                    Monitor.getDependencies() |> sendResponse context
                else
                    Monitor.getMetrics() |> sendResponse context
        }
        Async.Start(server)
        monitor.Value

    /// Stop the API and monitoring
    member __.Stop() =
        if monitor.IsSome then
            monitor.Value |> Monitor.stop
            CSharp.Monitor.ClearMonitor()
            monitor <- None
            listener.Stop()
        Async.CancelDefaultToken()
