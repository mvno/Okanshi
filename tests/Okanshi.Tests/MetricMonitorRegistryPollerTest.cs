using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Okanshi.Test
{
#if NET45
    public class MetricMonitorRegistryPollerTest
    {
        private readonly IMonitorRegistry _monitorRegistry;
        private readonly MetricMonitorRegistryPoller _metricMonitorRegistryPoller;

        public MetricMonitorRegistryPollerTest()
        {
            _monitorRegistry = Substitute.For<IMonitorRegistry>();
            _metricMonitorRegistryPoller = new MetricMonitorRegistryPoller(_monitorRegistry, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Metrics_are_polled_from_registry_when_interval_has_passed()
        {
            _metricMonitorRegistryPoller.MonitorEvents();

            Thread.Sleep(1100);

            _metricMonitorRegistryPoller.ShouldRaise("MetricsPolled");
        }

        [Fact]
        public void Stop_stops_metric_collection()
        {
            _metricMonitorRegistryPoller.MonitorEvents();
            _metricMonitorRegistryPoller.Stop();

            Thread.Sleep(1100);

            _metricMonitorRegistryPoller.ShouldNotRaise("MetricsPolled");
        }

        [Fact]
        public void Dispose_stops_metric_collection()
        {
            _metricMonitorRegistryPoller.MonitorEvents();
            _metricMonitorRegistryPoller.Dispose();
            object t = 10;

            Thread.Sleep(1100);

            _metricMonitorRegistryPoller.ShouldNotRaise("MetricsPolled");
        }

        [Fact]
        public void Polling_metrics_resets_monitor()
        {
            _metricMonitorRegistryPoller.MonitorEvents();
            var counter = new PeakCounter(MonitorConfig.Build("Test"));
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { counter });
            counter.Increment();

            Thread.Sleep(1100);

            counter.GetValue().Should().Be(0);
        }

        [Fact]
        public void Peak_counter_is_converted_to_a_single_metric_with_no_submetrics()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new PeakCounter(MonitorConfig.Build("Test")) });
            var resetEvent = new ManualResetEventSlim(false);
            var metrics = new Metric[0];
            _metricMonitorRegistryPoller.MetricsPolled += (sender, args) =>
            {
                metrics = args.Metrics;
                resetEvent.Set();
            };

            _metricMonitorRegistryPoller.PollMetrics();

            resetEvent.Wait(TimeSpan.FromSeconds(1));
            metrics.Should().HaveCount(1);
            metrics.Single().SubMetrics.Should().HaveCount(0);
        }

        [Fact]
        public void Basic_gauge_is_converted_to_a_single_metric_with_no_submetrics()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new BasicGauge<int>(MonitorConfig.Build("Test"), () => 1) });
            var resetEvent = new ManualResetEventSlim(false);
            var metrics = new Metric[0];
            _metricMonitorRegistryPoller.MetricsPolled += (sender, args) =>
            {
                metrics = args.Metrics;
                resetEvent.Set();
            };

            _metricMonitorRegistryPoller.PollMetrics();

            resetEvent.Wait(TimeSpan.FromSeconds(1));
            metrics.Should().HaveCount(1);
            metrics.Single().SubMetrics.Should().HaveCount(0);
        }

        [Fact]
        public void Timer_is_converted_to_a_one_metric_with_four_sub_metrics()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new BasicTimer(MonitorConfig.Build("Test")) });
            var resetEvent = new ManualResetEventSlim(false);
            var metrics = new Metric[0];
            _metricMonitorRegistryPoller.MetricsPolled += (sender, args) =>
            {
                metrics = args.Metrics;
                resetEvent.Set();
            };

            _metricMonitorRegistryPoller.PollMetrics();

            resetEvent.Wait(TimeSpan.FromSeconds(1));
            metrics.Should().HaveCount(1);
            metrics.Single().SubMetrics.Should().HaveCount(4);
        }
    }
#endif
}