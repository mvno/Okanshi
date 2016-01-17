using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class MonitorConfigTest
	{
		[Theory]
		[InlineData("Test")]
		[InlineData("Fact")]
		[InlineData("Anything")]
		public void Building_a_config_uses_the_specified_name(string expectedName)
		{
			var config = MonitorConfig.Build(expectedName);

			config.Name.Should().Be(expectedName);
		}

		[Fact]
		public void Config_without_tags_specified_has_no_tags()
		{
			var config = MonitorConfig.Build("Anything");

			config.Tags.Should().BeEmpty();
		}

		[Fact]
		public void Config_with_single_tag_specified_using_key_value_has_the_tag_attached()
		{
			const string key = "key";
			const string value = "value";
			var config = MonitorConfig.Build("Anything").WithTag(key, value);

			config.Tags.Should().HaveCount(1)
				.And.OnlyContain(x => x.Key == key && x.Value == value);
		}

		[Fact]
		public void Config_with_single_tag_specified_has_the_tag_attached()
		{
			const string key = "key";
			const string value = "value";
			var config = MonitorConfig.Build("Anything").WithTag(new Tag(key, value));

			config.Tags.Should().HaveCount(1)
				.And.OnlyContain(x => x.Key == key && x.Value == value);
		}

		[Fact]
		public void Config_with_multiple_tags_chained_has_all_the_tags_specified()
		{
			var expectedTags = new[] { new Tag("key", "value"), new Tag("key2", "value") };
			var config = MonitorConfig.Build("Anything");
			foreach (var tag in expectedTags)
			{
				config = config.WithTag(tag);
			}

			config.Tags.Should().BeEquivalentTo(expectedTags);
		}

		[Fact]
		public void Config_with_multiple_tags_in_one_call_has_all_the_tags_specified()
		{
			var expectedTags = new[] { new Tag("key", "value"), new Tag("key2", "value") };
			var config = MonitorConfig.Build("Anything").WithTags(expectedTags);

			config.Tags.Should().BeEquivalentTo(expectedTags);
		}

		[Fact]
		public void Configs_with_same_name_and_tags_are_equal()
		{
			var config1 = new MonitorConfig("Test", new[] { new Tag("tag", "value") });
			var config2 = new MonitorConfig("Test", new[] { new Tag("tag", "value") });

			config1.Should().Be(config2);
		}

		[Fact]
		public void Configs_with_same_name_and_tags_should_have_same_hash_code()
		{
			var config1 = new MonitorConfig("Test", new[] { new Tag("tag", "value") });
			var config2 = new MonitorConfig("Test", new[] { new Tag("tag", "value") });

			config1.GetHashCode().Should().Be(config2.GetHashCode());
		}
	}
}
