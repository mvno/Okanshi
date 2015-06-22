using System.Linq;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class MetricTest
	{
		[Fact]
		public void Add_failed_adds_measurement()
		{
			var metricMeasurements = CreateEmptyMetricMeasurements();

			metricMeasurements = Metric.addFailed(metricMeasurements);

			metricMeasurements.measurements.Should().HaveCount(1);
		}

		[Fact]
		public void Add_success_adds_measurement()
		{
			var metricMeasurements = CreateEmptyMetricMeasurements();

			metricMeasurements = Metric.addSuccess(metricMeasurements);

			metricMeasurements.measurements.Should().HaveCount(1);
		}

		[Fact]
		public void Add_timing_adds_measurement()
		{
			var metricMeasurements = CreateEmptyMetricMeasurements();

			metricMeasurements = Metric.addTiming(1, metricMeasurements);

			metricMeasurements.measurements.Should().HaveCount(1);
		}

		[Fact]
		public void Adding_adds_new_measurment_if_window_is_passed()
		{
			var metricMeasurements = CreateEmptyMetricMeasurements();
			metricMeasurements = Metric.addSuccess(metricMeasurements);
			metricMeasurements = Metric.addSuccess(metricMeasurements);
			Thread.Sleep(1100);

			metricMeasurements = Metric.addSuccess(metricMeasurements);

			metricMeasurements.measurements.Should().HaveCount(2);
		}

		[Fact]
		public void Adding_drops_oldest_measurment_when_max_measurements_have_been_reached()
		{
			var metricMeasurements = CreateEmptyMetricMeasurements();
			metricMeasurements = Metric.addFailed(metricMeasurements);
			Thread.Sleep(1100);
			metricMeasurements = Metric.addSuccess(metricMeasurements);
			Thread.Sleep(1100);

			metricMeasurements = Metric.addSuccess(metricMeasurements);

			metricMeasurements.measurements.Last().numberOfFailed.Should().Be(0);
		}

		private static Metric.Metric CreateEmptyMetricMeasurements()
		{
			return new Metric.Metric(Enumerable.Empty<Statistics.Statistics>(), 2, 1000);
		}
	}
}