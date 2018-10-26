using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Okanshi.Test
{
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
        public void Stop_stops_metric_collection()
        {
            var called = false;
            _metricMonitorRegistryPoller.RegisterObserver(_ => {
                called = true;
                return Task.FromResult<object>(null);
            });
            _metricMonitorRegistryPoller.Stop();

            Thread.Sleep(1100);

            called.Should().BeFalse();
        }

        [Fact]
        public void Dispose_stops_metric_collection()
        {
            var called = false;
            _metricMonitorRegistryPoller.RegisterObserver(_ => {
                called = true;
                return Task.FromResult<object>(null);
            });
            _metricMonitorRegistryPoller.Dispose();
            object t = 10;

            Thread.Sleep(1700);

            called.Should().BeFalse();
        }

        [Fact]
        public void Polling_metrics_resets_monitor()
        {
            var resetEvent = new ManualResetEventSlim(false);
            var monitor = Substitute.For<IMonitor>();
            monitor.Config.Returns(MonitorConfig.Build("Test"));
            monitor.When(x => x.GetValuesAndReset()).Do(_ => resetEvent.Set());

            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { monitor });

            resetEvent.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
        }

        [Fact]
        public void Counter_is_converted_to_a_single_metric_with_no_submetrics()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new Counter(MonitorConfig.Build("Test")) });
            var resetEvent = new ManualResetEventSlim(false);
            var metrics = Enumerable.Empty<Metric>();
            _metricMonitorRegistryPoller.RegisterObserver(x =>
            {
                metrics = x;
                resetEvent.Set();
                return Task.FromResult<object>(null);
            });

            _metricMonitorRegistryPoller.PollMetrics();

            resetEvent.Wait(TimeSpan.FromSeconds(1.5));
            metrics.Should().HaveCount(1);
            metrics.Single().Values.Should().HaveCount(1);
        }

        [Fact]
        public void Gauge_is_converted_to_a_metric_a_single_value()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new Gauge<int>(MonitorConfig.Build("Test"), () => 1) });
            var resetEvent = new ManualResetEventSlim(false);
            var metrics = Enumerable.Empty<Metric>();
            _metricMonitorRegistryPoller.RegisterObserver(x =>
            {
                metrics = x;
                resetEvent.Set();
                return Task.FromResult<object>(null);
            });

            _metricMonitorRegistryPoller.PollMetrics();

            resetEvent.Wait(TimeSpan.FromSeconds(2));
            metrics.Should().HaveCount(1);
            metrics.Single().Values.Should().HaveCount(1);
        }

        [Fact]
        public void Timer_is_converted_to_a_metric_with_four_values()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new Timer(MonitorConfig.Build("Test")) });
            var resetEvent = new ManualResetEventSlim(false);
            var metrics = Enumerable.Empty<Metric>();
            _metricMonitorRegistryPoller.RegisterObserver(x =>
            {
                metrics = x;
                resetEvent.Set();
                return Task.FromResult<object>(null);
            });

            _metricMonitorRegistryPoller.PollMetrics();

            resetEvent.Wait(TimeSpan.FromSeconds(2));
            metrics.Should().HaveCount(1);
            metrics.Single().Values.Should().HaveCount(5);
        }

        [Fact]
        public void After_unregistering_observer_it_is_not_called()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new Timer(MonitorConfig.Build("Test")) });
            var resetEvent = new ManualResetEventSlim(false);
            Func<IEnumerable<Metric>, Task> observer = _ =>
                {
                    resetEvent.Set();
                    return Task.FromResult<object>(null);
                };
            _metricMonitorRegistryPoller.RegisterObserver(observer);

            _metricMonitorRegistryPoller.UnregisterObserver(observer);
            _metricMonitorRegistryPoller.PollMetrics();

            resetEvent.Wait(TimeSpan.FromSeconds(1)).Should().BeFalse();
        }

        [Fact]
        public void Polling_metrics_Task_with_slow_observer_waits_for_the_observer_to_finish()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new Timer(MonitorConfig.Build("Test")) });
            Func<IEnumerable<Metric>, Task> observer = _ => Task.Delay(TimeSpan.FromSeconds(2));
            _metricMonitorRegistryPoller.RegisterObserver(observer);

            var task = _metricMonitorRegistryPoller.PollMetrics();

            task.Wait(TimeSpan.FromMilliseconds(500)).Should().BeFalse();
            task.Wait(TimeSpan.FromSeconds(20)).Should().BeTrue();
        }
    }
}