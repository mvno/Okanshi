using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class PerformanceCounterTest
    {
        public PerformanceCounterTest()
        {
            DefaultMonitorRegistry.Instance.Clear();
        }

        [Fact]
        public void Performance_counter_without_instance_name()
        {
            var performanceCounter = new PerformanceCounter("Memory", "Available Bytes");

            var monitor = new PerformanceCounterMonitor(MonitorConfig.Build("Test"),
                PerformanceCounterConfig.Build("Memory", "Available Bytes"));

            monitor.GetValue()
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

            monitor.GetValue()
                .Should()
                .BeGreaterThan(0)
                .And.BeApproximately(performanceCounter.NextValue(), 1000000,
                    "Because memory usage can change between the two values");
        }

        [Fact]
        public void Performance_counter_consists_of_a_single_monitor()
        {
            var monitor = new PerformanceCounterMonitor(MonitorConfig.Build("Test"),
                PerformanceCounterConfig.Build("Process", "Private Bytes", Process.GetCurrentProcess().ProcessName));

            monitor.GetAllMonitors().Should().HaveCount(1);
            monitor.GetAllMonitors().Single().Should().BeSameAs(monitor);
        }
    }
}