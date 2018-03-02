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
        public void Metrics_are_polled_from_registry_when_interval_has_passed()
        {
            var called = false;
            _metricMonitorRegistryPoller.RegisterObserver(_ => {
                called = true;
                return Task.FromResult<object>(null);
            });

            Thread.Sleep(1500);

            called.Should().BeTrue();
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

            Thread.Sleep(1100);

            called.Should().BeFalse();
        }

        [Fact]
        public void Polling_metrics_resets_monitor()
        {
            var counter = new PeakCounter(MonitorConfig.Build("Test"));
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { counter });
            counter.Increment();

            Thread.Sleep(1100);

            counter.GetValues().First().Value.Should().Be(0);
        }

        [Fact]
        public void Peak_counter_is_converted_to_a_single_metric_with_no_submetrics()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new PeakCounter(MonitorConfig.Build("Test")) });
            var resetEvent = new ManualResetEventSlim(false);
            var metrics = Enumerable.Empty<Metric>();
            _metricMonitorRegistryPoller.RegisterObserver(x =>
            {
                metrics = x;
                resetEvent.Set();
                return Task.FromResult<object>(null);
            });

            _metricMonitorRegistryPoller.PollMetrics();

            resetEvent.Wait(TimeSpan.FromSeconds(1));
            metrics.Should().HaveCount(1);
            metrics.Single().Values.Should().HaveCount(1);
        }

        [Fact]
        public void Basic_gauge_is_converted_to_a_single_metric_with_no_submetrics()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new BasicGauge<int>(MonitorConfig.Build("Test"), () => 1) });
            var resetEvent = new ManualResetEventSlim(false);
            var metrics = Enumerable.Empty<Metric>();
            _metricMonitorRegistryPoller.RegisterObserver(x =>
            {
                metrics = x;
                resetEvent.Set();
                return Task.FromResult<object>(null);
            });

            _metricMonitorRegistryPoller.PollMetrics();

            resetEvent.Wait(TimeSpan.FromSeconds(1));
            metrics.Should().HaveCount(1);
            metrics.Single().Values.Should().HaveCount(1);
        }

        [Fact]
        public void Timer_is_converted_to_a_one_metric_with_four_sub_metrics()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new BasicTimer(MonitorConfig.Build("Test")) });
            var resetEvent = new ManualResetEventSlim(false);
            var metrics = Enumerable.Empty<Metric>();
            _metricMonitorRegistryPoller.RegisterObserver(x =>
            {
                metrics = x;
                resetEvent.Set();
                return Task.FromResult<object>(null);
            });

            _metricMonitorRegistryPoller.PollMetrics();

            resetEvent.Wait(TimeSpan.FromSeconds(1));
            metrics.Should().HaveCount(1);
            metrics.Single().Values.Should().HaveCount(5);
        }

        [Fact]
        public void After_unregistering_observer_it_is_not_called()
        {
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new BasicTimer(MonitorConfig.Build("Test")) });
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
            _monitorRegistry.GetRegisteredMonitors().Returns(new[] { new BasicTimer(MonitorConfig.Build("Test")) });
            var resetEvent = new ManualResetEventSlim(false);
            Func<IEnumerable<Metric>, Task> observer = _ => Task.Delay(TimeSpan.FromSeconds(2));
            _metricMonitorRegistryPoller.RegisterObserver(observer);

            var task = _metricMonitorRegistryPoller.PollMetrics();

            task.Wait(TimeSpan.FromMilliseconds(500)).Should().BeFalse();
            task.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
        }
    }
}