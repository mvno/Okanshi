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

        [Fact]
        public void Health_check_monitor_has_tag_HealthCheck()
        {
            var healthCheck = new HealthCheck(MonitorConfig.Build("Test"), () => true);

            healthCheck.Config.Tags.Should().Contain(DataSourceType.HealthCheck);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Health_check_monitor_returns_the_value_check(bool expectedValue)
        {
            var healthCheck = new HealthCheck(MonitorConfig.Build("Test"), () => expectedValue);

            healthCheck.GetValues().First().Value.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(5)]
        public void Health_check_monitor_calls_check_function_everytime_the_value_is_fetced(int numberOfTimes)
        {
            var count = 0;
            var healthCheck = new HealthCheck(MonitorConfig.Build("Test"), () =>
            {
                count++;
                return true;
            });

            for (var i = 0; i < numberOfTimes; i++)
            {
                healthCheck.GetValues().ToList();
            }

            count.Should().Be(numberOfTimes);
        }

        [Fact]
        public void Value_is_called_value()
        {
            var healthCheck = new HealthCheck(MonitorConfig.Build("Test"), () => true);
            healthCheck.GetValues().Single().Name.Should().Be("value");
        }
    }
}