using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Xunit;
using Okanshi.CSharp;

namespace Okanshi.Test
{
	public class MonitorTest : IDisposable
	{
		public MonitorTest()
		{
			CSharp.Monitor.Start(new MonitorOptions { WindowSize = 1000 * 60 * 60, MaxNumberOfMeasurements = 200 });
		}

		public void Dispose()
		{
			CSharp.Monitor.Stop();
		}

		[Fact]
		public void Fetching_when_no_metrics_empty_result_is_returned()
		{
			var metrics = CSharp.Monitor.GetMetrics();

			metrics.Should().BeEmpty();
		}

		[Fact]
		public void Increment_success_adds_metrics_if_none_exist()
		{
			CSharp.Monitor.Success("test");

			Thread.Sleep(1000);
			CSharp.Monitor.GetMetrics().Should().NotBeEmpty();
		}

		[Fact]
		public void Increment_failed_adds_metrics_if_none_exist()
		{
			CSharp.Monitor.Failed("test");

			Thread.Sleep(1000);
			CSharp.Monitor.GetMetrics().Should().NotBeEmpty();
		}

		[Theory]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(122)]
		public void Success_increments_success_counter_of_metric(int numberOfIncrements)
		{
			const string name = "test";
			for (var i = 0; i < numberOfIncrements; i++)
			{
				CSharp.Monitor.Success(name);
			}

			Thread.Sleep(1000);
			CSharp.Monitor.GetMetrics()[name].measurements.First().numberOfSuccess.Should().Be(numberOfIncrements);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(122)]
		public void Failed_increments_failed_counter_of_metric(int numberOfIncrements)
		{
			const string name = "test";
			for (var i = 0; i < numberOfIncrements; i++)
			{
				CSharp.Monitor.Failed(name);
			}

			Thread.Sleep(1000);
			CSharp.Monitor.GetMetrics()[name].measurements.First().numberOfFailed.Should().Be(numberOfIncrements);
		}

		[Fact]
		public void Time_with_action_adds_metric_if_none_exists()
		{
			const string key = "test";

			CSharp.Monitor.Time(key, () => { });

			Thread.Sleep(1000);
			CSharp.Monitor.GetMetrics().Should().NotBeEmpty();
		}

		[Fact]
		public void Time_with_func_adds_metric_if_none_exists()
		{
			const string key = "test";

			CSharp.Monitor.Time(key, () => true);

			Thread.Sleep(1000);
			CSharp.Monitor.GetMetrics().Should().NotBeEmpty();
		}

		[Fact]
		public void Time_with_func_returns_value_from_func()
		{
			const string key = "test";

			var value = CSharp.Monitor.Time(key, () => true);

			Thread.Sleep(1000);
			value.Should().BeTrue();
		}

		[Theory]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(122)]
		public void Time_updates_timed_calls_value(int numberOfCalls)
		{
			const string key = "test";

			for (var i = 0; i < numberOfCalls; i++)
			{
				CSharp.Monitor.Time(key, () => { });
			}

			Thread.Sleep(1000);
			CSharp.Monitor.GetMetrics()[key].measurements.First().numberOfTimedCalls.Should().Be(numberOfCalls);
		}

		[Fact]
		public void Getting_assembly_dependencies_returns_current_assembly()
		{
			var currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

			var dependencies = CSharp.Monitor.GetDependencies();

			dependencies.Should().Contain(depencency => depencency.name == currentAssemblyName);
		}

		[Fact]
		public void OnMetricUpdated_is_called_when_incrementing_success()
		{
			MetricUpdated metricUpdated = null;
			CSharp.Monitor.Stop();
			CSharp.Monitor.Start(new MonitorOptions { OnMetricUpdated = x => { metricUpdated = x; } });
			const string name = "key";

			CSharp.Monitor.Success(name);

			Thread.Sleep(1000);
			metricUpdated.Added.GetIncrementSuccess().Should().Be(name);
			metricUpdated.Metric.measurements.Single().numberOfSuccess.Should().Be(1);
			metricUpdated.Timestamp.Should().BeWithin(5.Seconds()).Before(DateTimeOffset.Now);
		}

		[Fact]
		public void OnMetricUpdated_is_called_when_incrementing_failed()
		{
			MetricUpdated metricUpdated = null;
			CSharp.Monitor.Stop();
			CSharp.Monitor.Start(new MonitorOptions { OnMetricUpdated = x => { metricUpdated = x; } });
			const string name = "key";

			CSharp.Monitor.Failed(name);

			Thread.Sleep(1000);
			metricUpdated.Added.GetIncrementFailed().Should().Be(name);
			metricUpdated.Metric.measurements.Single().numberOfFailed.Should().Be(1);
			metricUpdated.Timestamp.Should().BeWithin(5.Seconds()).Before(DateTimeOffset.Now);
		}

		[Fact]
		public void OnMetricUpdated_is_called_when_timing()
		{
			MetricUpdated metricUpdated = null;
			CSharp.Monitor.Stop();
			CSharp.Monitor.Start(new MonitorOptions { OnMetricUpdated = x => { metricUpdated = x; } });
			const string name = "key";

			CSharp.Monitor.Time(name, () => { });

			Thread.Sleep(1000);
			var tuple = metricUpdated.Added.GetTime();
			tuple.Item1.Should().Be(name);
			tuple.Item2.Should().BeInRange(0L, 500L);
			metricUpdated.Metric.measurements.Single().minimum.Should().Be(tuple.Item2);
			metricUpdated.Timestamp.Should().BeWithin(5.Seconds()).Before(DateTimeOffset.Now);
		}

		[Fact]
		public void Passing_in_monitor_when_already_started_throws()
		{
			Action setMonitor = () => CSharp.Monitor.SetMonitor(Monitor.start(Monitor.defaultOptions));

			setMonitor.ShouldThrow<InvalidOperationException>();
		}

		[Fact]
		public void Starting_monitor_returns_monitor_instance()
		{
			CSharp.Monitor.Stop();
			
			var monitor = CSharp.Monitor.Start();

			monitor.Should().NotBeNull();
		}
	}
}
