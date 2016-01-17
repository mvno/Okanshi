using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class DoubleGaugeTest
	{
		[Fact]
		public void Gauge_tag_is_added_to_configuration()
		{
			var gauge = new DoubleGauge(MonitorConfig.Build("Test"));

			gauge.Config.Tags.Should().Contain(DataSourceType.Gauge);
		}

		[Fact]
		public void Initial_value_is_zero()
		{
			var gauge = new DoubleGauge(MonitorConfig.Build("Test"));

			gauge.GetValue().Should().Be(0);
		}

		[Theory]
		[InlineData(1.67)]
		[InlineData(500.0)]
		[InlineData(10.672)]
		public void Updating_the_value_updates_the_value_correctly(double expectedValue)
		{
			var gauge = new DoubleGauge(MonitorConfig.Build("Test"));

			gauge.Set(expectedValue);

			gauge.GetValue().Should().Be(expectedValue);
		}
	}
}
