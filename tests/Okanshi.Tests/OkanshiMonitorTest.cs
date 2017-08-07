using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class OkanshiMonitorTest
    {
        [Fact]
        public void Asking_for_monitor_with_same_names_always_return_same_monitor()
        {
            OkanshiMonitor.BasicTimer("Test").Should().BeSameAs(OkanshiMonitor.BasicTimer("Test"));
        }

        [Fact]
        public void Asking_for_monitor_with_different_names_returns_different_monitors()
        {
            OkanshiMonitor.BasicTimer("Test").Should().NotBeSameAs(OkanshiMonitor.BasicTimer("Test2"));
        }

        [Fact]
        public void Asking_for_monitor_with_different_names_but_same_tags_returns_different_monitors()
        {
            var tag = new Tag("tag", "value");
            OkanshiMonitor.BasicTimer("Test", new[] { tag }).Should().NotBeSameAs(OkanshiMonitor.BasicTimer("Test2", new[] { tag }));
        }

        [Fact]
        public void Asking_for_monitor_with_same_names_and_tags_returns_same_monitor()
        {
            var tag = new Tag("tag", "value");
            OkanshiMonitor.BasicTimer("Test", new[] { tag }).Should().BeSameAs(OkanshiMonitor.BasicTimer("Test", new[] { tag }));
        }

        [Fact]
        public void Asking_for_monitor_with_same_names_but_different_tags_returns_different_monitor()
        {
            OkanshiMonitor.BasicTimer("Test", new[] { new Tag("tag", "value") })
                .Should()
                .NotBeSameAs(OkanshiMonitor.BasicTimer("Test", new[] { new Tag("tag2", "value") }));
        }
    }
}