using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class DoubleCounterTest
	{
		[Fact]
		public void Initial_value_is_zero()
		{
			var counter = new DoubleCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));

			var value = counter.GetValue();

			value.Should().Be(0);
		}

		[Fact]
		public void Value_is_reset_when_steps_is_crossed()
		{
			var counter = new DoubleCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));
			counter.GetValue();
			Thread.Sleep(600);

			var value = counter.GetValue();

			value.Should().Be(0);
		}

		[Theory]
		[InlineData(1.0, 2.0)]
		[InlineData(1.1, 2.2)]
		[InlineData(605.18, 1210.36)]
		public void Value_returns_previous_steps_value_per_second(double amount, double expectedValue)
		{
			var counter = new DoubleCounter(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));
			counter.GetValue();
			counter.Increment(amount);
			Thread.Sleep(600);

			var value = counter.GetValue();

			value.Should().Be(expectedValue);
		}
	}
}
