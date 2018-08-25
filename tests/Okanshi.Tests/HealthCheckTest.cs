using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class HealthCheckTest
    {
        public HealthCheckTest()
        {
            HealthChecks.Clear();
            DefaultMonitorRegistry.Instance.Clear();
        }

        [Fact]
        public void When_check_fails_a_fail_is_returned()
        {
            const string name = "test";
            const bool expectedResult = false;
            HealthChecks.Add(name, () => expectedResult);

            var results = HealthChecks.RunAll();

            results.Should().Contain(name, expectedResult);
        }

        [Fact]
        public void When_check_succeeds_a_success_is_returned()
        {
            const string name = "test";
            const bool expectedResult = true;
            HealthChecks.Add(name, () => expectedResult);

            var results = HealthChecks.RunAll();

            results.Should().Contain(name, expectedResult);
        }

        [Fact]
        public void Adding_check_makes_it_available()
        {
            const string name = "test";
            HealthChecks.Add(name, () => false);

            var exists = HealthChecks.Exists(name);

            exists.Should().BeTrue();
        }

        [Fact]
        public void Checking_for_unavailable_check_returns_false()
        {
            var exists = HealthChecks.Exists("unavilable");

            exists.Should().BeFalse();
        }
    }
}