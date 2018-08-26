using System;
using System.Threading;
using System.Threading.Tasks;
using Okanshi;
using Newtonsoft.Json;

namespace Okanshi.Sample
{
    public class Program
    {
        public static readonly AutoResetEvent closing = new AutoResetEvent(false);
        public static IMetricPoller poller;
        public static IProcessingMetricObserver observer;

        public static void Main(string[] args)
        {
            poller = new MetricMonitorRegistryPoller(DefaultMonitorRegistry.Instance, TimeSpan.FromSeconds(10), false);
            observer = new MemoryMetricObserver(poller, 100);

            Start();

            poller.PollMetrics().Wait();
            Console.WriteLine("Measurements:");
            var observations = observer.GetObservations();
            var result = JsonConvert.SerializeObject(observations, Formatting.Indented);
            Console.WriteLine(result);
        }

        public static void Start() {
            Console.WriteLine("Enter 10 numbers");
            var count = 0;
            while (count < 10) {
                var value = OkanshiMonitor.Timer("TimeToEnterInput").Record(() => {
                    Console.Write("Enter an integer: ");
                    return Console.ReadLine();
                });
                int result;
                if (!Int32.TryParse(value, out result)) {
                    Console.WriteLine("Invalid number, try again...");
                    OkanshiMonitor.Counter("InvalidNumbers").Increment();
                    continue;
                }
                OkanshiMonitor.Counter("ValidNumbers").Increment();
                OkanshiMonitor.AverageGauge("AverageNumber").Set(result);
                OkanshiMonitor.MinGauge("MinimumNumber").Set(result);
                OkanshiMonitor.MaxGauge("MaximumNumber").Set(result);
                count++;
            }
        }
    }
}
