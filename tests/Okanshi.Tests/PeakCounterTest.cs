using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class PeakCounterTest
    {
        private readonly PeakCounter counter;

        public PeakCounterTest()
        {
            counter = new PeakCounter(MonitorConfig.Build("Test"));
        }

        [Fact]
        public void Initial_peak_is_zero()
        {
            var value = counter.GetValue();

            value.Should().Be(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(110)]
        public void Incrementing_value_updates_peak(int amount)
        {
            counter.Increment(amount);

            counter.GetValue().Should().Be(amount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(110)]
        public void Get_and_reset_returns_the_peak(int amount)
        {
            counter.Increment(amount);

            counter.GetValueAndReset().Should().Be(amount);
        }

        [Fact]
        public void Peak_is_reset_after_get_and_reset()
        {
            counter.Increment();

            counter.GetValueAndReset();

            var value = counter.GetValue();
            value.Should().Be(0);
        }

        [Fact]
        public void Incrementing_with_negative_numbers_does_not_change_the_value()
        {
            counter.Increment();

            counter.Increment(-1);

            counter.GetValue().Should().Be(1);
        }

        [Fact]
        public void Incrementing_with_negative_numbers_and_then_with_a_positive_does_not_change_the_value()
        {
            counter.Increment();
            counter.Increment(-1);
            counter.Increment();

            counter.GetValue().Should().Be(1);
        }

        [Fact]
        public void Consists_of_a_single_monitor()
        {
            counter.GetAllMonitors().Should().HaveCount(1);
            counter.GetAllMonitors().Single().Should().BeSameAs(counter);
        }
    }
}