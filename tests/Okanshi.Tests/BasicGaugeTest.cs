using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class BasicGaugeTest
	{
		[Fact]
		public void Gauge_tag_is_added_to_configuration()
		{
			var gauge = new BasicGauge<int>(MonitorConfig.Build("Test"), () => 0);

			gauge.Config.Tags.Should().Contain(DataSourceType.Gauge);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(167)]
		public void Value_is_gotten_through_passed_in_func(int expectedValue)
		{
			var gauge = new BasicGauge<int>(MonitorConfig.Build("Test"), () => expectedValue);

			gauge.GetValue().Should().Be(expectedValue);
		}
	}
}
