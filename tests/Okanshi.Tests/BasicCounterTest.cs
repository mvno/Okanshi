using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class BasicCounterTest
	{
		[Fact]
		public void Initial_value_is_zero()
		{
			var counter = new BasicCounter(MonitorConfig.Build("Test"));

			counter.GetValue().Should().Be(0);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(110)]
		public void Incrementing_the_counters_works_as_expected(int amount)
		{
			var counter = new BasicCounter(MonitorConfig.Build("Test"));

			counter.Increment(amount);

			counter.GetValue().Should().Be(amount);
		}
	}
}
