using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class TagTest
    {
        [Fact]
        public void Creating_a_new_tag_sets_the_correct_key()
        {
            var expectedKey = "key";

            var tag = new Tag(expectedKey, "anything");

            tag.Key.Should().Be(expectedKey);
        }

        [Fact]
        public void Creating_a_new_tag_sets_the_correct_value()
        {
            var expectedValue = "value";

            var tag = new Tag("anything", expectedValue);

            tag.Value.Should().Be(expectedValue);
        }

        [Fact]
        public void Two_tags_with_the_same_key_and_value_should_be_equal()
        {
            var tag1 = new Tag("key", "value");
            var tag2 = new Tag("key", "value");

            tag1.Should().Be(tag2);
        }

        [Fact]
        public void Two_tags_with_the_same_key_and_value_should_have_same_hash()
        {
            var tag1Hash = new Tag("key", "value").GetHashCode();
            var tag2Hash = new Tag("key", "value").GetHashCode();

            tag1Hash.Should().Be(tag2Hash);
        }
    }
}