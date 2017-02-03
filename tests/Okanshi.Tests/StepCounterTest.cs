using System;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class StepCounterTest
	{
        private readonly ManualClock manualClock = new ManualClock();
	    private readonly StepCounter stepCounter;

	    public StepCounterTest()
	    {
            stepCounter = new StepCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500), manualClock);
        }

		[Fact]
		public void Initial_value_is_zero()
		{
			var value = stepCounter.GetValue();

			value.Should().Be(0);
		}

		[Fact]
		public void Value_is_NaN_if_more_than_one_step_has_been_crossed_without_value_has_been_incremented()
		{
			stepCounter.GetValue();
			manualClock.Advance(TimeSpan.FromSeconds(2));

			var value = stepCounter.GetValue();

			value.Should().Be(double.NaN);
		}

		[Theory]
		[InlineData(1, 0.5)]
		[InlineData(5, 2.5)]
		public void Value_is_rate_in_previous_step(int amount, float expectedValue)
		{
            stepCounter.Increment(amount);
			stepCounter.GetValue();
            manualClock.Advance(TimeSpan.FromMilliseconds(600));
			
			var value = stepCounter.GetValue();

			value.Should().Be(expectedValue);
		}

		[Theory]
		[InlineData(1, 2)]
		[InlineData(5, 10)]
		public void Incrementing_counter_does_not_affect_current_interval(int amount, int expectedValue)
		{
			var stepCounter = new StepCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500), manualClock);
			stepCounter.Increment(amount);
			
			var value = stepCounter.GetValue();

			value.Should().Be(0);
		}
	}
}
