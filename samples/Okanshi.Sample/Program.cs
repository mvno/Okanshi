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
            // Get the default registry instance. This is the registry used when adding metrics
            // through OkanshiMonitor.
            var registry = DefaultMonitorRegistry.Instance;

            // Create a new poller to get data from the registry every 10 seconds
            poller = new MetricMonitorRegistryPoller(registry, TimeSpan.FromSeconds(10), false);

            // Add a memory observer getting data from the poller, and keep the last 100 observations
            // in memory
            observer = new MemoryMetricObserver(poller, 100);

            // Start the actual program
            Start();

            // This flushes all the measurements to the observer, before displaying the result
            poller.PollMetrics().Wait();

            // Write the result as JSON
            Console.WriteLine("Measurements:");
            var observations = observer.GetObservations();
            var result = JsonConvert.SerializeObject(observations, Formatting.Indented);
            Console.WriteLine(result);
        }

        public static void Start() {
            Console.WriteLine("Enter 10 numbers");
            var count = 0;
            while (count < 10) {
                // Time how long the user takes to enter input.
                var value = OkanshiMonitor.Timer("TimeToEnterInput").Record(() => {
                    Console.Write("Enter an integer: ");
                    return Console.ReadLine();
                });

                int result;
                if (!Int32.TryParse(value, out result)) {
                    Console.WriteLine("Invalid number, try again...");
                    // Count the number of invalids numbers, using a tag to indicate that the
                    // number was invalid
                    OkanshiMonitor.Counter("Numbers", new[] { new Tag("state", "Invalid") }).Increment();
                    continue;
                }

                // Count the number of valids numbers, using a tag to indicate that the
                // number was valid
                OkanshiMonitor.Counter("Numbers", new[] { new Tag("state", "Valid") }).Increment();

                // Calculate the average value of the numbers
                OkanshiMonitor.AverageGauge("AverageNumber").Set(result);

                // Calculate the minimum value of the numbers
                OkanshiMonitor.MinGauge("MinimumNumber").Set(result);

                // Calculate the maximum value of the numbers
                OkanshiMonitor.MaxGauge("MaximumNumber").Set(result);
                count++;
            }
        }
    }
}
