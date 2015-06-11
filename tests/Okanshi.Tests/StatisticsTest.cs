using System;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class StatisticsTest
	{
		[Fact]
		public void Add_failed_increments_the_failed_counter()
		{
			var measurement = Statistics.createEmptyMeasurement(TimeSpan.FromDays(1));

			measurement = Statistics.addFailed(measurement);

			measurement.numberOfFailed.Should().Be(1);
		}

		[Fact]
		public void Add_succes_increments_the_success_counter()
		{
			var measurement = Statistics.createEmptyMeasurement(TimeSpan.FromDays(1));

			measurement = Statistics.addSuccess(measurement);

			measurement.numberOfSuccess.Should().Be(1);
		}

		[Fact]
		public void Add_timing_increments_the_timed_counter()
		{
			var measurement = Statistics.createEmptyMeasurement(TimeSpan.FromDays(1));

			measurement = Statistics.addTiming(1L, measurement);

			measurement.numberOfTimedCalls.Should().Be(1);
		}

		[Fact]
		public void Add_timing_sets_the_minimum_value_to_smallest_value()
		{
			const long expectedValue = 1L;
			var measurement = Statistics.createEmptyMeasurement(TimeSpan.FromDays(1));
			measurement = Statistics.addTiming(2L, measurement);

			measurement = Statistics.addTiming(expectedValue, measurement);

			measurement.minimum.Should().Be(expectedValue);
		}

		[Fact]
		public void Add_timing_sets_the_maximum_value_to_largest_value()
		{
			const long expectedValue = 100L;
			var measurement = Statistics.createEmptyMeasurement(TimeSpan.FromDays(1));
			measurement = Statistics.addTiming(1L, measurement);

			measurement = Statistics.addTiming(expectedValue, measurement);

			measurement.maximum.Should().Be(expectedValue);
		}

		[Fact]
		public void Add_timing_calculats_the_average()
		{
			const long expected = 20L;
			var measurement = Statistics.createEmptyMeasurement(TimeSpan.FromDays(1));
			measurement = Statistics.addTiming(10L, measurement);

			measurement = Statistics.addTiming(30L, measurement);

			measurement.average.Should().Be(expected);
		}

		[Fact]
		public void Add_timing_calculats_the_variance()
		{
			const decimal expected = 50M;
			var measurement = Statistics.createEmptyMeasurement(TimeSpan.FromDays(1));
			measurement = Statistics.addTiming(1994L, measurement);

			measurement = Statistics.addTiming(2004L, measurement);

			measurement.variance.Should().Be(expected);
		}

		[Fact]
		public void Standard_deviation_is_square_root_of_variance()
		{
			var measurement = Statistics.createEmptyMeasurement(TimeSpan.FromDays(1));
			measurement = Statistics.addTiming(10L, measurement);

			var standardDeviation = measurement.standardDeviation;

			standardDeviation.Should().Be(Math.Sqrt((double)measurement.variance));
		}
	}
}