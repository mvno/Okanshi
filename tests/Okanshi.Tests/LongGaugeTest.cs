using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class LongGaugeTest
    {
        [Fact]
        public void Gauge_tag_is_added_to_configuration()
        {
            var gauge = new LongGauge(MonitorConfig.Build("Test"));

            gauge.Config.Tags.Should().Contain(DataSourceType.Gauge);
        }

        [Fact]
        public void Initial_value_is_zero()
        {
            var gauge = new LongGauge(MonitorConfig.Build("Test"));

            gauge.GetValueAs("").Value.Should().Be(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(500)]
        [InlineData(10)]
        public void Updating_the_value_updates_the_value_correctly(long expectedValue)
        {
            var gauge = new LongGauge(MonitorConfig.Build("Test"));

            gauge.Set(expectedValue);

            gauge.GetValueAs("").Value.Should().Be(expectedValue);
        }

        [Fact]
        public void Reset_sets_the_value_to_zero()
        {
            var gauge = new LongGauge(MonitorConfig.Build("Test"));
            gauge.Set(100L);

            gauge.Reset();

            gauge.GetValueAs("").Value.Should().Be(0L);
        }

        [Fact]
        public void Get_and_reset_sets_the_value_to_zero()
        {
            var gauge = new LongGauge(MonitorConfig.Build("Test"));
            gauge.Set(100L);

            gauge.GetValuesAndReset().ToList();

            gauge.GetValueAs("").Value.Should().Be(0L);
        }

        [Fact]
        public void Get_and_reset_gets_the_maximum_value()
        {
            const long expected = 100L;
            var gauge = new LongGauge(MonitorConfig.Build("Test"));
            gauge.Set(expected);

            gauge.GetValuesAndReset().First().Value.Should().Be(expected);
        }

        [Fact]
        public void Average_value_is_called_value()
        {
            var gauge = new LongGauge(MonitorConfig.Build("Test"));
            gauge.GetValues().Single().Name.Should().Be("value");
        }
    }
}