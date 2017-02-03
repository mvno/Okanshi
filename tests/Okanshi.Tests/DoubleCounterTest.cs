using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class DoubleCounterTest
	{
	    private DoubleCounter counter;
	    private readonly ManualClock manualClock = new ManualClock();

	    public DoubleCounterTest()
	    {
			counter = new DoubleCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500), manualClock);
        }

        [Fact]
		public void Initial_value_is_zero()
		{
			var value = counter.GetValue();

			value.Should().Be(0);
		}

		[Fact]
		public void Value_is_reset_when_steps_is_crossed()
		{
			counter.GetValue();
			manualClock.Advance(TimeSpan.FromMilliseconds(600));

			var value = counter.GetValue();

			value.Should().Be(0);
		}

		[Theory]
		[InlineData(1.0, 0.5)]
		[InlineData(2.2, 1.1)]
		[InlineData(605.18, 302.59)]
		public void Value_returns_previous_steps_value_per_second(double amount, double expectedValue)
		{
			counter.GetValue();
			counter.Increment(amount);
			manualClock.Advance(TimeSpan.FromMilliseconds(600));

            var value = counter.GetValue();

			value.Should().Be(expectedValue);
		}
	}
}
