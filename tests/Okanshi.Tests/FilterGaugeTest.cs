using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class FilterGaugeTest
    {
        [Fact]
        public void FilterGauge_is_an_igauge()
        {
            IGauge<long> gauge = new GaugeZeroFilter<long>(new LongGauge(MonitorConfig.Build("Test")));
        }

        [Fact]
        public void okanshimonitor_can_create_instance()
        {
            IGauge<long> gauge = OkanshiMonitor.WithZeroFiltering.LongGauge("foo");

            gauge.Config.Name.Should().Be("foo");
        }

        [Fact]
        public void factory_can_create_instance()
        {
            var factory = new ZeroFilterFactory(new OkanshiMonitorRegistry(), new Tag[0]);
            IGauge<long> gauge = factory.LongGauge("foo");

            gauge.Config.Name.Should().Be("foo");
        }

        [Fact]
        public void Do_not_send_data_when_nothing_registered()
        {
            var gauge = new GaugeZeroFilter<long>(new LongGauge(MonitorConfig.Build("Test")));
            
            gauge.GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((IGauge<long>)gauge).GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Fact]
        public void Do_not_send_data_when_nothing_registered_after_GetValues()
        {
            var gauge = new GaugeZeroFilter<long>(new LongGauge(MonitorConfig.Build("Test")));
            gauge.Set(33);
            gauge.GetValues();
            
            gauge.GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((IGauge<long>)gauge).GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Theory]
        [InlineData(-3)]
        [InlineData(0)]
        [InlineData(42)]        
        public void Send_data_when_registered(int someValue)
        {
            var gauge = new GaugeZeroFilter<long>(new LongGauge(MonitorConfig.Build("Test")));
            gauge.Set(someValue);

            gauge.GetValues().Single();
        }

        [Fact]
        public void Send_data_when_Reset()
        {
            var gauge = new GaugeZeroFilter<long>(new LongGauge(MonitorConfig.Build("Test")));
            gauge.Reset();

            gauge.GetValues().Single();
        }

        [Fact]
        public void Do_not_send_data_and_reset_when_nothing_registered()
        {
            var gauge = new GaugeZeroFilter<long>(new LongGauge(MonitorConfig.Build("Test")));
            
            gauge.GetValuesAndReset().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((IGauge<long>)gauge).GetValuesAndReset().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Theory]
        [InlineData(-3)]
        [InlineData(0)]
        [InlineData(42)]
        public void Do_not_send_data_and_reset_when_nothing_registered_after_GetValues(int someValue)
        {
            var gauge = new GaugeZeroFilter<long>(new LongGauge(MonitorConfig.Build("Test")));
            gauge.Set(someValue);
            gauge.GetValuesAndReset();
            
            gauge.GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((IGauge<long>)gauge).GetValuesAndReset().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Fact]
        public void Send_data_and_reset_when_registered()
        {
            var gauge = new GaugeZeroFilter<long>(new LongGauge(MonitorConfig.Build("Test")));
            gauge.Set(33);

            gauge.GetValuesAndReset().Single();
        }
    }
}