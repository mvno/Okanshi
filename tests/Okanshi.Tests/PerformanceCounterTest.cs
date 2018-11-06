using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Xunit;
using System;

namespace Okanshi.Test
{
#if NET46
    public class PerformanceCounterTest
    {
        public PerformanceCounterTest()
        {
            DefaultMonitorRegistry.Instance.Clear();
        }

        [Fact]
        public void Okanshimonitor_can_create_instance()
        {
            PerformanceCounterMonitor p = OkanshiMonitor.PerformanceCounter(PerformanceCounterConfig.Build("Memory", "Available Bytes"), "name");
            p.Should().NotBeNull();
        }

        [Fact]
        public void Okanshimonitor_can_create_instance_with_tags()
        {
            PerformanceCounterMonitor p = OkanshiMonitor.PerformanceCounter(PerformanceCounterConfig.Build("Memory", "Available Bytes"),"name", new[]{new Tag("a","b")});
            p.Should().NotBeNull();
        }

        [Fact]
        public void Performance_counter_without_instance_name()
        {
            var performanceCounter = new PerformanceCounter("Memory", "Available Bytes");

            var monitor = new PerformanceCounterMonitor(MonitorConfig.Build("Test"),
                PerformanceCounterConfig.Build("Memory", "Available Bytes"));

            monitor.GetValues()
                .Single()
                .Value
                .Should()
                .BeGreaterThan(0)
                .And.BeApproximately(performanceCounter.NextValue(), 1000000,
                    "Because memory usage can change between the two values");
        }

        [Fact]
        public void Performance_counter_with_instance_name()
        {
            var performanceCounter = new PerformanceCounter("Process", "Private Bytes", Process.GetCurrentProcess().ProcessName);

            var monitor = new PerformanceCounterMonitor(MonitorConfig.Build("Test"),
                PerformanceCounterConfig.Build("Process", "Private Bytes", Process.GetCurrentProcess().ProcessName));

            monitor.GetValues()
                .Single()
                .Value
                .Should()
                .BeGreaterThan(0)
                .And.BeApproximately(performanceCounter.NextValue(), 1000000,
                    "Because memory usage can change between the two values");
        }

        [Fact]
        public void Performance_counter_consists_of_a_single_value()
        {
            var monitor = new PerformanceCounterMonitor(MonitorConfig.Build("Test"),
                PerformanceCounterConfig.Build("Process", "Private Bytes", Process.GetCurrentProcess().ProcessName));

            monitor.GetValues().Should().HaveCount(1);
        }

        [Fact]
        public void Value_is_called_value()
        {
            var monitor = new PerformanceCounterMonitor(MonitorConfig.Build("Test"),
                PerformanceCounterConfig.Build("Process", "Private Bytes", Process.GetCurrentProcess().ProcessName));
            monitor.GetValues().Single().Name.Should().Be("value");
        }
    }
#endif
}