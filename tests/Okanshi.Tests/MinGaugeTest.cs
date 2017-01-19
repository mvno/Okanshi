using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class MinGaugeTest
	{
		[Fact]
		public void Gauge_tag_is_added_to_configuration()
		{
			var gauge = new MinGauge(MonitorConfig.Build("Test"));

			gauge.Config.Tags.Should().Contain(DataSourceType.Gauge);
		}

		[Fact]
		public void Initial_value_is_zero()
		{
			var gauge = new MinGauge(MonitorConfig.Build("Test"));

			gauge.GetValue().Should().Be(0);
		}

		[Fact]
		public void Supplying_value_less_than_previously_record_updates_the_value()
		{
			const long maximumValue = 1000;
			const long minimumValue = 10;
			var gauge = new MinGauge(MonitorConfig.Build("Test"));
			gauge.Set(maximumValue);

			gauge.Set(minimumValue);

			gauge.GetValue().Should().Be(minimumValue);
		}

		[Fact]
		public void Supplying_value_greater_than_previously_record_does_not_update_the_value()
		{
			const long minimumValue = 500;
			const long maximumValue = 1000;
			var gauge = new MinGauge(MonitorConfig.Build("Test"));
			gauge.Set(minimumValue);

			gauge.Set(maximumValue);

			gauge.GetValue().Should().Be(minimumValue);
        }

        [Fact]
        public void Reset_sets_the_value_to_zero()
        {
            var gauge = new MaxGauge(MonitorConfig.Build("Test"));
            gauge.Set(-100L);

            gauge.Reset();

            gauge.GetValue().Should().Be(0L);
        }
    }
}
