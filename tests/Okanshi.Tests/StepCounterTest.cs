using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class StepCounterTest
	{
		[Fact]
		public void Initial_value_is_zero()
		{
			var stepCounter = new StepCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));

			var value = stepCounter.GetValue();

			value.Should().Be(0);
		}

		[Fact]
		public void Value_is_NaN_if_more_than_one_step_has_been_crossed_without_value_has_been_incremented()
		{
			var stepCounter = new StepCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));
			stepCounter.GetValue();
			Thread.Sleep(500 * 3);

			var value = stepCounter.GetValue();

			value.Should().Be(double.NaN);
		}

		[Theory]
		[InlineData(1, 1)]
		[InlineData(5, 5)]
		public void Value_is_number_of_counts_per_interval_in_previous_step(int amount, float expectedValue)
		{
			var stepCounter = new StepCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(1000));
			stepCounter.Increment(amount);
			stepCounter.GetValue();
			Thread.Sleep(1100);
			
			var value = stepCounter.GetValue();

			value.Should().Be(expectedValue);
		}

		[Theory]
		[InlineData(1, 2)]
		[InlineData(5, 10)]
		public void Incrementing_counter_does_not_affect_current_interval(int amount, int expectedValue)
		{
			var stepCounter = new StepCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));
			stepCounter.Increment(amount);
			
			var value = stepCounter.GetValue();

			value.Should().Be(0);
		}
	}
}
