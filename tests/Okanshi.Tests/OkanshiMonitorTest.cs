using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class OkanshiMonitorTest
    {
        [Fact]
        public void Asking_for_monitor_with_same_names_always_return_same_monitor()
        {
            OkanshiMonitor.Timer("Test").Should().BeSameAs(OkanshiMonitor.Timer("Test"));
        }

        [Fact]
        public void Asking_for_monitor_with_different_names_returns_different_monitors()
        {
            OkanshiMonitor.Timer("Test").Should().NotBeSameAs(OkanshiMonitor.Timer("Test2"));
        }

        [Fact]
        public void Asking_for_monitor_with_different_names_but_same_tags_returns_different_monitors()
        {
            var tag = new Tag("tag", "value");
            OkanshiMonitor.Timer("Test", new[] { tag }).Should().NotBeSameAs(OkanshiMonitor.Timer("Test2", new[] { tag }));
        }

        [Fact]
        public void Asking_for_monitor_with_same_names_and_tags_returns_same_monitor()
        {
            var tag = new Tag("tag", "value");
            OkanshiMonitor.Timer("Test", new[] { tag }).Should().BeSameAs(OkanshiMonitor.Timer("Test", new[] { tag }));
        }

        [Fact]
        public void Asking_for_monitor_with_same_names_but_different_tags_returns_different_monitor()
        {
            OkanshiMonitor.Timer("Test", new[] { new Tag("tag", "value") })
                .Should()
                .NotBeSameAs(OkanshiMonitor.Timer("Test", new[] { new Tag("tag2", "value") }));
        }

        [Fact]
        public void Adding_identical_tags_to_default_tags_results_in_only_a_single_entry()
        {
            var tag = new Tag("key", "value");
            OkanshiMonitor.DefaultTags.Add(tag);
            OkanshiMonitor.DefaultTags.Add(tag);

            OkanshiMonitor.DefaultTags.Should().HaveCount(1);
            OkanshiMonitor.DefaultTags.Should().BeEquivalentTo(new[] { tag });
        }
    }
}