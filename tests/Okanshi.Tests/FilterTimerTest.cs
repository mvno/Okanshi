using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class FilterTimerTest
    {
        [Fact]
        public void FilterTimer_is_an_itimer()
        {
            ITimer timer = new TimerZeroFilter(new Timer(MonitorConfig.Build("Test")));
        }

        [Fact]
        public void okanshimonitor_can_create_instance()
        {
            ITimer timer = OkanshiMonitor.WithZeroFiltering.Timer("foo");

            timer.Config.Name.Should().Be("foo");
        }

        [Fact]
        public void factory_can_create_instance()
        {
            var factory = new ZeroFilterFactory(new OkanshiMonitorRegistry(), new Tag[0]);
            ITimer timer = factory.Timer("foo");

            timer.Config.Name.Should().Be("foo");
        }

        [Fact]
        public void Do_not_send_data_when_nothing_registered()
        {
            var timer = new TimerZeroFilter(new Timer(MonitorConfig.Build("Test")));
            
            timer.GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((ITimer)timer).GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Fact]
        public void Do_not_send_data_when_nothing_registered_after_GetValues()
        {
            var timer = new TimerZeroFilter(new Timer(MonitorConfig.Build("Test")));
            timer.Register(TimeSpan.FromSeconds(1));
            timer.GetValues();
            
            timer.GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((ITimer)timer).GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Theory]
        [InlineData(-3)]
        [InlineData(0)]
        [InlineData(42)]
        public void Send_data_when_registered(int someValue)
        {
            var timer = new TimerZeroFilter(new Timer(MonitorConfig.Build("Test")));
            timer.Register(TimeSpan.FromSeconds(someValue));

            timer.GetValues().Any().Should().BeTrue();
        }

        [Fact]
        public void Send_data_when_registered_as_elapsed()
        {
            var timer = new TimerZeroFilter(new Timer(MonitorConfig.Build("Test")));
            timer.RegisterElapsed(new System.Diagnostics.Stopwatch());

            timer.GetValues().Any().Should().BeTrue();
        }
        [Fact]
        public void Send_data_when_registered_as_function()
        {
            var timer = new TimerZeroFilter(new Timer(MonitorConfig.Build("Test")));
            timer.Record(() => 42);

            timer.GetValues().Any().Should().BeTrue();
        }

        [Fact]
        public void Do_not_send_data_and_reset_when_nothing_registered()
        {
            var timer = new TimerZeroFilter(new Timer(MonitorConfig.Build("Test")));
            
            timer.GetValuesAndReset().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((ITimer)timer).GetValuesAndReset().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Fact]
        public void Do_not_send_data_and_reset_when_nothing_registered_after_GetValues()
        {
            var timer = new TimerZeroFilter(new Timer(MonitorConfig.Build("Test")));
            timer.Record(() => 42);
            timer.GetValuesAndReset();
            
            timer.GetValues().ShouldBeEquivalentTo(new IMeasurement[0]);
            ((ITimer)timer).GetValuesAndReset().ShouldBeEquivalentTo(new IMeasurement[0]);
        }

        [Theory]
        [InlineData(-3)]
        [InlineData(0)]
        [InlineData(42)]
        public void Send_data_and_reset_when_registered(int someValue)
        {
            var timer = new TimerZeroFilter(new Timer(MonitorConfig.Build("Test")));
            timer.Register(TimeSpan.FromSeconds(someValue));

            timer.GetValues().Any().Should().BeTrue();
        }

    }
}