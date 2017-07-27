using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class OkanshiMonitorRegistryTest
    {
        private readonly OkanshiMonitorRegistry _okanshiMonitorRegistry;

        public OkanshiMonitorRegistryTest()
        {
            _okanshiMonitorRegistry = new OkanshiMonitorRegistry();
        }

        [Fact]
        public void No_monitors_are_registered_by_default()
        {
            var monitors = _okanshiMonitorRegistry.GetRegisteredMonitors();

            monitors.Should().BeEmpty();
        }

        [Fact]
        public void Registering_a_monitor_adds_it_to_the_list_of_registered_monitors()
        {
            var monitor = new FakeMonitor();

            _okanshiMonitorRegistry.Register(monitor);

            _okanshiMonitorRegistry
                .GetRegisteredMonitors().Single()
                .Should().Be(monitor);
        }

        [Fact]
        public void Unregistering_a_monitor_removes_it_from_the_list_of_registered_monitors()
        {
            var monitor = new FakeMonitor();
            _okanshiMonitorRegistry.Register(monitor);

            _okanshiMonitorRegistry.Unregister(monitor);

            _okanshiMonitorRegistry.GetRegisteredMonitors()
                .Should().BeEmpty();
        }

        [Fact]
        public void Monitor_is_not_registered_when_not_added_to_the_list_of_registered_monitors()
        {
            var monitor = new FakeMonitor();

            var isRegistered = _okanshiMonitorRegistry.IsRegistered(monitor);

            isRegistered.Should().BeFalse();
        }

        [Fact]
        public void Monitor_is_registered_when_added_to_the_list_of_registered_monitors()
        {
            var monitor = new FakeMonitor();
            _okanshiMonitorRegistry.Register(monitor);

            var isRegistered = _okanshiMonitorRegistry.IsRegistered(monitor);

            isRegistered.Should().BeTrue();
        }

        [Fact]
        public void Registering_two_monitors_of_same_type_with_same_tags_results_in_one_registrations()
        {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor();
            _okanshiMonitorRegistry.Register(monitor);

            _okanshiMonitorRegistry.Register(anotherMonitor);

            _okanshiMonitorRegistry.GetRegisteredMonitors().Should().HaveCount(1);
        }

        [Fact]
        public void Registering_two_monitors_of_same_type_with_different_tags_results_in_two_registrations()
        {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor(new[] { new Tag("Test", "Test"), });
            _okanshiMonitorRegistry.Register(monitor);

            _okanshiMonitorRegistry.Register(anotherMonitor);

            _okanshiMonitorRegistry.GetRegisteredMonitors().Should().HaveCount(2);
        }

        [Fact]
        public void Registering_monitors_multiple_times_results_in_a_single_registration()
        {
            var monitor = new FakeMonitor("Firstname");
            _okanshiMonitorRegistry.Register(monitor);

            _okanshiMonitorRegistry.Register(monitor);

            _okanshiMonitorRegistry.GetRegisteredMonitors()
                .Should().HaveCount(1);
        }

        [Fact]
        public void Get_or_add_two_monitors_of_same_type_with_same_tags_result_in_one_registration()
        {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor();
            _okanshiMonitorRegistry.GetOrAdd(monitor.Config, _ => monitor);

            _okanshiMonitorRegistry.GetOrAdd(anotherMonitor.Config, _ => anotherMonitor);

            _okanshiMonitorRegistry.GetRegisteredMonitors().Should().HaveCount(1);
        }

        [Fact]
        public void Get_or_add_two_monitors_of_same_type_with_same_tags_returns_the_first_registrated_instance()
        {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor();
            _okanshiMonitorRegistry.GetOrAdd(monitor.Config, _ => monitor);

            var result = _okanshiMonitorRegistry.GetOrAdd(anotherMonitor.Config, _ => anotherMonitor);

            result.Should().BeSameAs(monitor);
        }

        [Fact]
        public void Get_or_add_two_monitors_of_same_type_with_different_tags_result_in_two_registrations()
        {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor(new[] { new Tag("Test", "Test"),  });
            _okanshiMonitorRegistry.GetOrAdd(monitor.Config, _ => monitor);

            _okanshiMonitorRegistry.GetOrAdd(anotherMonitor.Config, _ => anotherMonitor);

            _okanshiMonitorRegistry.GetRegisteredMonitors().Should().HaveCount(2);
        }

        [Fact]
        public void Get_or_add_two_monitors_of_same_type_with_differrent_tags_returns_the_newly_registrated_instance()
        {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor(new[] { new Tag("Test", "Test"), });
            _okanshiMonitorRegistry.GetOrAdd(monitor.Config, _ => monitor);

            var result = _okanshiMonitorRegistry.GetOrAdd(anotherMonitor.Config, _ => anotherMonitor);

            result.Should().BeSameAs(anotherMonitor);
        }

        [Fact]
        public void Get_or_add_two_monitors_of_different_types_result_in_two_registrations() {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor2();

            _okanshiMonitorRegistry.GetOrAdd(monitor.Config, _ => monitor);
            _okanshiMonitorRegistry.GetOrAdd(anotherMonitor.Config, _ => anotherMonitor);

            _okanshiMonitorRegistry.GetRegisteredMonitors().Should().HaveCount(2);
        }

        private class FakeMonitor : IMonitor
        {
            public FakeMonitor(IEnumerable<Tag> tags = null)
            {
                Config = MonitorConfig.Build("test");
                if (tags != null)
                {
                    Config = Config.WithTags(tags);
                }
            }

            public FakeMonitor(string name)
            {
                Config = MonitorConfig.Build(name);
            }

            public object GetValue()
            {
                throw new System.NotImplementedException();
            }

            public object GetValueAndReset()
            {
                throw new System.NotImplementedException();
            }

            public MonitorConfig Config { get; private set; }
        }

        private class FakeMonitor2 : IMonitor
        {
            public FakeMonitor2(IEnumerable<Tag> tags = null)
            {
                Config = MonitorConfig.Build("test");
                if (tags != null)
                {
                    Config = Config.WithTags(tags);
                }
            }

            public FakeMonitor2(string name)
            {
                Config = MonitorConfig.Build(name);
            }

            public object GetValue()
            {
                throw new System.NotImplementedException();
            }

            public object GetValueAndReset()
            {
                throw new System.NotImplementedException();
            }

            public MonitorConfig Config { get; private set; }
        }
    }
}