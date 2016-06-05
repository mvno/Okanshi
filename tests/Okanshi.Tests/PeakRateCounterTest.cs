using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class PeakRateCounterTest
	{
		[Fact]
		public void Initial_peak_rate_is_zero()
		{
			var counter = new PeakRateCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));

			var value = counter.GetValue();

			value.Should().Be(0);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(110)]
		public void Incrementing_value_updates_peak_rate_after_interval(int amount)
		{
			var counter = new PeakRateCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));

			counter.Increment(amount);

			Thread.Sleep(600);
			counter.GetValue().Should().Be(amount);
		}

		[Fact]
		public void Peak_rate_is_reset_when_crossing_interval_again_and_polling_multiple_times()
		{
			var counter = new PeakRateCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));
			counter.Increment();
			Thread.Sleep(600);
			counter.GetValue();
			Thread.Sleep(600);

			var value = counter.GetValue();

			value.Should().Be(0);
		}

		[Fact]
		public void Peak_rate_is_per_defined_step()
		{
			var counter = new PeakRateCounter(MonitorConfig.Build("Test"), TimeSpan.FromSeconds(5));
			counter.Increment();
			Thread.Sleep(1000);
			counter.Increment();
			Thread.Sleep(6000);

			counter.GetValue().Should().Be(2);
		}

		[Fact]
		public void Peak_rate_is_updated_correctly_by_interval()
		{
			var counter = new PeakRateCounter(MonitorConfig.Build("Test"), TimeSpan.FromSeconds(1));
			counter.Increment();
			Thread.Sleep(1300);
			counter.Increment();
			Thread.Sleep(1300);

			counter.GetValue().Should().Be(1);
		}
	}
}
