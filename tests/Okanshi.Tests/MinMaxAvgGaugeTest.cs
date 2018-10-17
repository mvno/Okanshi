using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class MinMaxAvgGaugeTest
    {
        private readonly MinMaxAvgGauge gauge;

        public MinMaxAvgGaugeTest()
        {
            gauge = new MinMaxAvgGauge(MonitorConfig.Build("Test"));
        }

        [Fact]
        public void Okanshimonitor_integration_returns_fresh_instance()
        {
            var monitor = OkanshiMonitor.MinMaxAvgGauge("Foo");

            monitor.Config.Name.Should().Be("Foo");
            monitor.GetValues().Select(x => x.ToString()).Should().BeEquivalentTo("min:0", "max:0", "avg:0");
        }

        [Fact]
        public void Initial_value_is_zero()
        {
            gauge.GetValues().Select(x => x.ToString()).Should().BeEquivalentTo("min:0", "max:0", "avg:0");
        }

        [Fact]
        public void One_value_sets_all_gauges_to_the_that_value()
        {
            gauge.Set(300);

            gauge.GetValues().Select(x => x.ToString()).Should().BeEquivalentTo("min:300", "max:300", "avg:300");
        }

        [Fact]
        public void Three_distinct_measurements_show_all_three_kinds_of_gauges_calculate_correctly()
        {
            gauge.Set(300);
            gauge.Set(900);
            gauge.Set(1200);

            gauge.GetValues().Select(x => x.ToString()).Should().BeEquivalentTo("min:300", "max:1200", "avg:800");
        }

        [Fact]
        public void Set_and_reset_sets_the_values_to_zero()
        {
            gauge.Set(100);

            gauge.GetValuesAndReset();

            gauge.GetValues().Select(x => x.ToString()).Should().BeEquivalentTo("min:0", "max:0", "avg:0");
        }

        [Fact]
        public void Get_and_reset_gets_the_values()
        {
            gauge.Set(300);
            gauge.Set(900);
            gauge.Set(1200);

            gauge.GetValuesAndReset().Select(x => x.ToString()).Should().BeEquivalentTo("min:300", "max:1200", "avg:800");
        }

        [Fact]
        public void Average_is_calculated_correctly_after_reset()
        {
            gauge.Set(100L);
            gauge.GetValuesAndReset();
            gauge.Set(200L);

            gauge.GetValuesAndReset().Select(x => x.ToString()).Should().BeEquivalentTo("min:200", "max:200", "avg:200");
        }
    }
}