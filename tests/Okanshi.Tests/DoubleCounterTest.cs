using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class DoubleCounterTest
    {
        private readonly DoubleCounter counter;

        public DoubleCounterTest()
        {
            counter = new DoubleCounter(MonitorConfig.Build("Test"));
        }

        [Fact]
        public void Initial_value_is_zero()
        {
            var value = counter.GetValue();

            value.Should().Be(0);
        }

        [Fact]
        public void Value_is_reset_when_get_and_reset_is_called()
        {
            counter.Increment();

            counter.GetValueAndReset();

            counter.GetValue().Should().Be(0);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.2)]
        [InlineData(605.18)]
        public void Value_returns_the_value(double expectedValue)
        {
            counter.Increment(expectedValue);

            var value = counter.GetValue();

            value.Should().Be(expectedValue);
        }

        [Fact]
        public void Incrementing_multiple_time_increments_the_value()
        {
            counter.Increment(1);
            counter.Increment(1);

            var value = counter.GetValue();

            value.Should().Be(2);
        }

        [Fact]
        public void Get_and_reset_returns_the_value()
        {
            counter.Increment(1);

            var value = counter.GetValueAndReset();

            value.Should().Be(1);
        }

        [Fact]
        public void Consists_of_a_single_monitor()
        {
            counter.GetAllMonitors().Should().HaveCount(1);
            counter.GetAllMonitors().Single().Should().BeSameAs(counter);
        }
    }
}