using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class BasicCounterTest
    {
        [Fact]
        public void Initial_value_is_zero()
        {
            var counter = new BasicCounter(MonitorConfig.Build("Test"));

            counter.GetValueAs("").Value.Should().Be(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(110)]
        public void Incrementing_the_counters_works_as_expected(int amount)
        {
            var counter = new BasicCounter(MonitorConfig.Build("Test"));

            counter.Increment(amount);

            counter.GetValueAs("").Value.Should().Be(amount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(110)]
        public void Get_and_reset_does_not_reset_the_value(int amount)
        {
            var counter = new BasicCounter(MonitorConfig.Build("Test"));
            counter.Increment(amount);

            counter.GetValuesAndReset();

            counter.GetValueAs("").Value.Should().Be(amount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(110)]
        public void Get_and_reset_returns_the_value(int amount)
        {
            var counter = new BasicCounter(MonitorConfig.Build("Test"));
            counter.Increment(amount);

            var value = counter.GetValuesAndReset();

            value.First().Value.Should().Be(amount);
        }

        [Fact]
        public void Value_is_called_value()
        {
            var counter = new BasicCounter(MonitorConfig.Build("Test"));
            counter.GetValues().Single().Name.Should().Be("value");
        }
    }
}