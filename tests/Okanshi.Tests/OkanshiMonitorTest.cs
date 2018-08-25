using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	/// <summary>
	/// Messing with default tags should be run in a different context than other tests dealing with tags
	/// in order for tests to be executable in parallel
	/// </summary>
	public class OkanshiMonitorDefaultTagsTest
	{
		[Fact]
		public void When_setting_duplicate_tags_Then_accept_input_but_remove_duplicates()
		{
			var duplicateTag = new Tag("a", "b");

			OkanshiMonitor.DefaultTags = 
				OkanshiMonitor.DefaultTags
				.Union(new[] {duplicateTag, duplicateTag})
				.ToArray();

			OkanshiMonitor.DefaultTags.Should().BeEquivalentTo(new[] {duplicateTag});
		}
	}

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
    }
}