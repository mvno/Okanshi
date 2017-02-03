using System;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class PeakRateCounterTest
	{
	    private readonly PeakRateCounter counter;
	    private readonly ManualClock manualClock = new ManualClock();

	    public PeakRateCounterTest()
	    {
	        counter = new PeakRateCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500), manualClock);
	    }

		[Fact]
		public void Initial_peak_rate_is_zero()
		{
			var value = counter.GetValue();

			value.Should().Be(0);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(110)]
		public void Incrementing_value_updates_peak_rate_after_interval(int amount)
		{
			counter.Increment(amount);

			manualClock.Advance(TimeSpan.FromMilliseconds(600));
			counter.GetValue().Should().Be(amount);
		}

		[Fact]
		public void Peak_rate_is_reset_when_crossing_interval_again_and_polling_multiple_times()
		{
			counter.Increment();
            manualClock.Advance(TimeSpan.FromMilliseconds(600));
            counter.GetValue();
            manualClock.Advance(TimeSpan.FromMilliseconds(600));

            var value = counter.GetValue();

			value.Should().Be(0);
		}

		[Fact]
		public void Peak_rate_is_per_defined_step()
		{
			counter.Increment();
			counter.Increment();
            manualClock.Advance(TimeSpan.FromMilliseconds(700));

            counter.GetValue().Should().Be(2);
		}

		[Fact]
		public void Peak_rate_is_updated_correctly_by_interval()
		{
			counter.Increment();
            manualClock.Advance(TimeSpan.FromMilliseconds(600));
            counter.Increment();
            manualClock.Advance(TimeSpan.FromMilliseconds(600));

            counter.GetValue().Should().Be(1);
		}

		[Fact]
		public void Incrementing_with_negative_numbers_does_not_change_the_value()
		{
			counter.Increment();

			counter.Increment(-1);

            manualClock.Advance(TimeSpan.FromMilliseconds(600));
            counter.GetValue().Should().Be(1);
		}

		[Fact]
		public void Incrementing_with_negative_numbers_and_then_with_a_positive_does_not_change_the_value()
		{
			counter.Increment();
			counter.Increment(-1);
			counter.Increment();

            manualClock.Advance(TimeSpan.FromMilliseconds(600));
            counter.GetValue().Should().Be(1);
		}
	}
}
