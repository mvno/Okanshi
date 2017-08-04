using System;
using System.Linq;
using FluentAssertions;
using Xunit;


namespace Okanshi.Test {
    public class AverageGaugeTest
    {
        private readonly ManualClock manualClock;
        private readonly AverageGauge gauge;

        public AverageGaugeTest() {
            manualClock = new ManualClock();
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
            gauge.GetValue().Should().Be(0);
        }

        [Fact]
        public void Value_is_the_average() {
            gauge.Set(100);
            gauge.Set(200);

            gauge.GetValue().Should().Be(150);
        }

        [Fact]
        public void Get_and_reset_sets_the_value_to_zero() {
            gauge.Set(100L);

            gauge.GetValueAndReset();

            gauge.GetValue().Should().Be(0L);
        }

        [Fact]
        public void Get_and_reset_gets_the_maximum_value() {
            const long expected = 100L;
            gauge.Set(expected);

            gauge.GetValueAndReset().Should().Be(expected);
        }

        [Fact]
        public void Consists_of_a_single_monitor() {
            var gauge = new AverageGauge(MonitorConfig.Build("Test"));

            gauge.GetAllMonitors().Should().HaveCount(1);
            gauge.GetAllMonitors().Single().Should().BeSameAs(gauge);
        }
    }
}