using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class MaxGaugeTest
	{
		[Fact]
		public void Gauge_tag_is_added_to_configuration()
		{
			var gauge = new MaxGauge(MonitorConfig.Build("Test"));

			gauge.Config.Tags.Should().Contain(DataSourceType.Gauge);
		}

		[Fact]
		public void Initial_value_is_zero()
		{
			var gauge = new MaxGauge(MonitorConfig.Build("Test"));

			gauge.GetValue().Should().Be(0);
		}

		[Fact]
		public void Supplying_value_greater_than_previously_record_updates_the_value()
		{
			const long expectedValue = 1000;
			var gauge = new MaxGauge(MonitorConfig.Build("Test"));

			gauge.Set(expectedValue);

			gauge.GetValue().Should().Be(expectedValue);
		}

		[Fact]
		public void Supplying_value_less_than_previously_record_does_not_update_the_value()
		{
			const long maximumValue = 500;
			var gauge = new MaxGauge(MonitorConfig.Build("Test"));
			gauge.Set(maximumValue);

			gauge.Set(100);

			gauge.GetValue().Should().Be(maximumValue);
		}
	}
}
