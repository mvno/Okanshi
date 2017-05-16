using System;
using FluentAssertions;
using Xunit;


namespace Okanshi.Test {
    public class AverageGaugeTest
    {
        private readonly ManualClock manualClock;
        private readonly AverageGauge gauge;

        public AverageGaugeTest() {
            manualClock = new ManualClock();
            gauge = new AverageGauge(MonitorConfig.Build("Test"), TimeSpan.FromMinutes(1), manualClock);
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
        public void After_interval_the_average_is_available() {
            gauge.Set(100);
            gauge.Set(200);
            manualClock.Advance(TimeSpan.FromMinutes(1));

            gauge.GetValue().Should().Be(150);
        }

        [Fact]
        public void Reset_occurs_after_interval() {
            gauge.Set(100);
            manualClock.Advance(TimeSpan.FromMinutes(1));
            gauge.GetValue();
            manualClock.Advance(TimeSpan.FromMinutes(1));

            gauge.GetValue().Should().Be(0);
        }
    }
}