using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Okanshi.Test
{
	public class MemoryMetricObserverTest
	{
		private readonly IMetricPoller _metricPoller;
		private readonly MemoryMetricObserver _memoryMetricObserver;

		public MemoryMetricObserverTest()
		{
			_metricPoller = Substitute.For<IMetricPoller>();
			_memoryMetricObserver = new MemoryMetricObserver(_metricPoller);
		}

		[Fact]
		public void Updates_from_poller_are_received()
		{
			var metrics = new[] { new Metric("Test", DateTimeOffset.Now, new[] { new Tag("Test", "Test"), }, 100), };
			_metricPoller.MetricsPolled += Raise.Event<MetricEventDelegate>(this, new MetricEventArgs(metrics));

			_memoryMetricObserver.GetObservations().First().Should().BeEquivalentTo(metrics);
		}

		[Fact]
		public void Multiple_updates_from_poller_are_received_in_different_batches()
		{
			var metrics = new[] { new Metric("Test", DateTimeOffset.Now, new[] { new Tag("Test", "Test"), }, 100), };
			var metrics2 = new[] { new Metric("Test2", DateTimeOffset.Now, new[] { new Tag("Test2", "Test2"), }, 100), };
			_metricPoller.MetricsPolled += Raise.Event<MetricEventDelegate>(this, new MetricEventArgs(metrics));
			_metricPoller.MetricsPolled += Raise.Event<MetricEventDelegate>(this, new MetricEventArgs(metrics2));

			_memoryMetricObserver.GetObservations().First().Should().BeEquivalentTo(metrics);
			_memoryMetricObserver.GetObservations().Last().Should().BeEquivalentTo(metrics2);
		}
	}
}
