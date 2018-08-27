using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class MinGaugeTest
    {
        [Fact]
        public void Initial_value_is_zero()
        {
            var gauge = new MinGauge(MonitorConfig.Build("Test"));

            gauge.GetValues().First().Value.Should().Be(0);
        }

        [Fact]
        public void Supplying_value_less_than_previously_record_updates_the_value()
        {
            const long maximumValue = 1000;
            const long minimumValue = 10;
            var gauge = new MinGauge(MonitorConfig.Build("Test"));
            gauge.Set(maximumValue);

            gauge.Set(minimumValue);

            gauge.GetValues().First().Value.Should().Be(minimumValue);
        }

        [Fact]
        public void Supplying_value_greater_than_previously_record_does_not_update_the_value()
        {
            const long minimumValue = 500;
            const long maximumValue = 1000;
            var gauge = new MinGauge(MonitorConfig.Build("Test"));
            gauge.Set(minimumValue);

            gauge.Set(maximumValue);

            gauge.GetValues().First().Value.Should().Be(minimumValue);
        }

        [Fact]
        public void Reset_sets_the_value_to_zero()
        {
            var gauge = new MinGauge(MonitorConfig.Build("Test"));
            gauge.Set(-100L);

            gauge.Reset();

            gauge.GetValues().First().Value.Should().Be(0L);
        }

        [Fact]
        public void Get_and_reset_sets_the_value_to_zero()
        {
            var gauge = new MinGauge(MonitorConfig.Build("Test"));
            gauge.Set(100L);

            gauge.GetValuesAndReset().ToList();

            gauge.GetValues().First().Value.Should().Be(0L);
        }

        [Fact]
        public void Get_and_reset_gets_the_maximum_value()
        {
            const long expected = 100L;
            var gauge = new MinGauge(MonitorConfig.Build("Test"));
            gauge.Set(expected);

            gauge.GetValuesAndReset().First().Value.Should().Be(expected);
        }

        [Fact]
        public void Value_is_called_value()
        {
            var gauge = new MinGauge(MonitorConfig.Build("Test"));
            gauge.GetValues().Single().Name.Should().Be("value");
        }
    }
}