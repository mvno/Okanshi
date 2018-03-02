using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class AverageGaugeTest
    {
        private readonly AverageGauge gauge;

        public AverageGaugeTest()
        {
            gauge = new AverageGauge(MonitorConfig.Build("Test"));
        }

        [Fact]
        public void Gauge_tag_is_added_to_configuration()
        {
            gauge.Config.Tags.Should().Contain(DataSourceType.Gauge);
        }

        [Fact]
        public void Initial_value_is_zero()
        {
            gauge.GetValues().First().Value.Should().Be(0);
        }

        [Fact]
        public void Value_is_the_average()
        {
            gauge.Set(100);
            gauge.Set(200);

            gauge.GetValues().First().Value.Should().Be(150);
        }

        [Fact]
        public void Get_and_reset_sets_the_value_to_zero()
        {
            gauge.Set(100L);

            gauge.GetValuesAndReset();

            gauge.GetValues().First().Value.Should().Be(0L);
        }

        [Fact]
        public void Get_and_reset_gets_the_maximum_value()
        {
            const long expected = 100L;
            gauge.Set(expected);

            gauge.GetValuesAndReset().First().Value.Should().Be(expected);
        }

        [Fact]
        public void Average_is_calculated_correctly_after_reset()
        {
            gauge.Set(100L);
            gauge.GetValuesAndReset();
            gauge.Set(200L);

            var result = gauge.GetValuesAndReset();

            result.First().Value.Should().Be(200);
        }

        [Fact]
        public void Average_value_is_called_value()
        {
            gauge.GetValues().Single().Name.Should().Be("value");
        }
    }
}