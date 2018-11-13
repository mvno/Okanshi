using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class FilterCounterTest
    {
        [Fact]
        public void okanshimonitor_can_create_instance()
        {
            ICounter<long> counter = OkanshiMonitor.WithAbsentFiltering.Counter("foo");

            counter.Config.Name.Should().Be("foo");
        }

        [Fact]
        public void factory_can_create_instance()
        {
            var factory = new AbsentMeasurementsFilterFactory(new OkanshiMonitorRegistry(), new Tag[0]);
            ICounter<long> counter = factory.Counter("foo");

            counter.Config.Name.Should().Be("foo");
        }

        [Fact]
        public void FilterCounter_is_an_icounter()
        {
            ICounter<long> counter = new CounterAbsentFilter<long>(new Counter(MonitorConfig.Build("Test")));
        }

        [Fact]
        public void Do_not_send_data_when_nothing_registered()
        {
            var counter = new CounterAbsentFilter<long>(new Counter(MonitorConfig.Build("Test")));
            
            counter.GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((ICounter<long>)counter).GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Fact]
        public void Do_not_send_data_when_nothing_registered_after_GetValues()
        {
            var counter = new CounterAbsentFilter<long>(new Counter(MonitorConfig.Build("Test")));
            counter.Increment();
            counter.GetValues();
            
            counter.GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((ICounter<long>)counter).GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Fact]
        public void FilterCounter_send_data_when_registered()
        {
            var counter = new CounterAbsentFilter<long>(new Counter(MonitorConfig.Build("Test")));
            counter.Increment();

            counter.GetValues().Single();
        }

        [Theory]
        [InlineData(-3)]
        [InlineData(0)]
        [InlineData(42)]
        public void FilterCounter_send_data_when_registered_as_incrementamount(int someValue)
        {
            var counter = new CounterAbsentFilter<long>(new Counter(MonitorConfig.Build("Test")));
            counter.Increment(someValue);

            counter.GetValues().Single();
        }

        [Fact]
        public void Do_not_send_data_and_reset_when_nothing_registered()
        {
            var counter = new CounterAbsentFilter<long>(new Counter(MonitorConfig.Build("Test")));
            
            counter.GetValuesAndReset().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((ICounter<long>)counter).GetValuesAndReset().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Fact]
        public void Do_not_send_data_and_reset_when_nothing_registered_after_GetValues()
        {
            var counter = new CounterAbsentFilter<long>(new Counter(MonitorConfig.Build("Test")));
            counter.Increment();
            counter.GetValuesAndReset();
            
            counter.GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((ICounter<long>)counter).GetValuesAndReset().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Fact]
        public void FilterCounter_send_data_and_reset_when_registered()
        {
            var counter = new CounterAbsentFilter<long>(new Counter(MonitorConfig.Build("Test")));
            counter.Increment();

            counter.GetValuesAndReset().Single();
        }
    }
}