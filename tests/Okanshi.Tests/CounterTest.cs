using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class CounterTest
    {
        private readonly Counter counter;

        public CounterTest()
        {
            counter = new Counter(MonitorConfig.Build("Test"));
        }

        [Fact]
        public void Initial_peak_is_zero()
        {
            var value = counter.GetValues();

            value.First().Value.Should().Be(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(110)]
        public void Incrementing_value_updates_peak(int amount)
        {
            counter.Increment(amount);

            counter.GetValues().First().Value.Should().Be(amount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(110)]
        public void Get_and_reset_returns_the_peak(int amount)
        {
            counter.Increment(amount);

            counter.GetValuesAndReset().First().Value.Should().Be(amount);
        }

        [Fact]
        public void Peak_is_reset_after_get_and_reset()
        {
            counter.Increment();

            counter.GetValuesAndReset();

            var value = counter.GetValues();
            value.First().Value.Should().Be(0);
        }

        [Fact]
        public void Incrementing_with_negative_numbers_does_not_change_the_value()
        {
            counter.Increment();

            counter.Increment(-1);

            counter.GetValues().First().Value.Should().Be(1);
        }

        [Fact]
        public void Incrementing_with_negative_numbers_and_then_with_a_positive_does_not_change_the_value()
        {
            counter.Increment();
            counter.Increment(-1);
            counter.Increment();

            counter.GetValues().First().Value.Should().Be(1);
        }

        [Fact]
        public void Value_is_called_value()
        {
            counter.GetValues().Single().Name.Should().Be("value");
        }
    }
}