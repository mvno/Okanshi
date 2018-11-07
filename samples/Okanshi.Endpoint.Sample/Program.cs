using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Okanshi.Endpoint.Sample
{
    class Program
    {
        // The point of this sample to is to showcase the use of the HTTP endpoint
        static void Main(string[] args)
        {
            // Get the default registry instance. This is the registry used when adding metrics
            // through OkanshiMonitor.
            var registry = DefaultMonitorRegistry.Instance;
            var poller = new MetricMonitorRegistryPoller(registry, TimeSpan.FromMinutes(1));

            // Create a HTTP endpoint, this listens on the default registry instance, gets the values
            // every 10 seconds and keeps the last 100 samples in memory
            var httpEndpoint = new MonitorEndpoint(new EndpointOptions {
                NumberOfSamplesToStore = 100,
            }, poller, o => JsonConvert.SerializeObject(o, Formatting.Indented));
            httpEndpoint.Start();

            while (true) {
                Console.WriteLine("Monitor key presses. Start typing to measure");
                while (true) {
                    var info = Console.ReadKey();
                    string measurementName;
                    if ((int)info.Modifiers == 0) {
                        measurementName = info.Key.ToString();
                    } else {
                        measurementName = $"{info.Modifiers} + {info.Key}";
                    }
                    System.Console.WriteLine();
                    OkanshiMonitor.Counter("Key press", new[] { new Tag("combination", measurementName) }).Increment();
                }
            }
        }
    }
}
