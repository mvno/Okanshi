using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class DefaultMonitorRegistryTest
    {
        [Fact]
        public void Instance_is_a_singleton()
        {
            var instance = DefaultMonitorRegistry.Instance;
            var instanceTwo = DefaultMonitorRegistry.Instance;

            instance.Should().BeSameAs(instanceTwo);
        }
    }
}