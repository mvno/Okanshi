using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class StepLongTest
	{
		[Fact]
		public void Default_current_value_is_zero()
		{
			var stepLong = new StepLong(TimeSpan.FromMinutes(1));

			var value = stepLong.GetCurrent().Get();

			value.Should().Be(0);
		}

		[Fact]
		public void Default_previous_value_is_zero()
		{
			var stepLong = new StepLong(TimeSpan.FromMinutes(1));

			var value = stepLong.Poll().Value;

			value.Should().Be(0);
		}

		[Fact]
		public void Polling_multiple_times_within_the_step_returns_zero_value()
		{
			var stepLong = new StepLong(TimeSpan.FromMinutes(1));
			stepLong.Poll();

			var datapoint = stepLong.Poll();

			datapoint.Value.Should().Be(0);
		}

		[Fact]
		public void Polling_multiple_times_outside_the_step_returns_the_different_datapoints()
		{
			var stepLong = new StepLong(TimeSpan.FromMilliseconds(500));
			var firstDatapoint = stepLong.Poll();
			Thread.Sleep(700);

			var secondDatapoint = stepLong.Poll();

			secondDatapoint.Should().NotBe(firstDatapoint);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(11)]
		[InlineData(56)]
		public void Increment_increments_the_current_value_as_expected(int amount)
		{
			var stepLong = new StepLong(TimeSpan.FromMinutes(1));

			stepLong.Increment(amount);

			stepLong.GetCurrent().Get().Should().Be(amount);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(11)]
		[InlineData(56)]
		public void Increment_within_the_same_interval_does_not_increment_the_polled_value(int amount)
		{
			var stepLong = new StepLong(TimeSpan.FromMinutes(1));

			stepLong.Increment(amount);
			stepLong.Increment(amount);

			stepLong.Poll().Value.Should().Be(0);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(11)]
		[InlineData(56)]
		public void Polling_after_step_interval_returns_the_previous_steps_value(int expectedValue)
		{
			var stepLong = new StepLong(TimeSpan.FromMilliseconds(500));
			stepLong.Poll();
			stepLong.Increment(expectedValue);
			Thread.Sleep(600);

			var datapoint = stepLong.Poll();

			datapoint.Value.Should().Be(expectedValue);
		}

		[Fact]
		public void When_value_has_not_been_polled_for_more_than_one_step_empty_datapoint_is_returned()
		{
			var stepLong = new StepLong(TimeSpan.FromMilliseconds(500));
			stepLong.Poll();
			stepLong.Increment(100);
			Thread.Sleep(2000);

			var datapoint = stepLong.Poll();

			datapoint.Should().Be(Datapoint.Empty);
		}
	}
}
