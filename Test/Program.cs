using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.WriteOnly;
using Okanshi;
using Okanshi.Observers;

namespace Test {
    class Program {
        static void Main(string[] args)
        {
            var client = new InfluxDbClient(new Uri("https://telemetrytest.northeurope.cloudapp.azure.com:8086/"), "suadmin", "SimCorp1SimCorp1", throwOnException: true)
            {
                RequestConfigurator = request => request.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true
            };
            var poller = new MetricMonitorRegistryPoller(DefaultMonitorRegistry.Instance, TimeSpan.FromSeconds(40));
            var observer = new InfluxDbObserver(poller, client, new InfluxDbObserverOptions("telemetry"));
            OkanshiMonitor.PeakRateCounter("Test", TimeSpan.FromSeconds(30), new [] { new Okanshi.Tag("Test", "Test1") }).Increment();
            OkanshiMonitor.PeakRateCounter("Test", TimeSpan.FromSeconds(30), new[] { new Okanshi.Tag("Test", "Test2") }).Increment();
            Thread.Sleep(40000);
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
