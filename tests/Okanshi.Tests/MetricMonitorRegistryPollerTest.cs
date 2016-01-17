using System;
using System.Threading;
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

			Thread.Sleep(1100);

			_metricMonitorRegistryPoller.ShouldNotRaise("MetricsPolled");
		}
	}
}
