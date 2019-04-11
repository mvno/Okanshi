using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Okanshi.Test
{
    public class EvictingRegistryTest
    {
        private readonly EvictingRegistry _evictingRegistry = new EvictingRegistry();

        [Fact]
        public void No_monitors_are_registered_by_default()
        {
            var monitors = _evictingRegistry.GetRegisteredMonitors();

            monitors.Should().BeEmpty();
        }

        [Fact]
        public void Monitor_is_not_registered_when_not_added_to_the_list_of_registered_monitors()
        {
            var monitor = new FakeMonitor();

            var isRegistered = _evictingRegistry.IsRegistered(monitor);

            isRegistered.Should().BeFalse();
        }

        [Fact]
        public void Get_or_add_two_monitors_of_same_type_with_same_tags_result_in_one_registration()
        {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor();
            _evictingRegistry.GetOrAdd(monitor.Config, _ => monitor);

            _evictingRegistry.GetOrAdd(anotherMonitor.Config, _ => anotherMonitor);

            _evictingRegistry.GetRegisteredMonitors().Should().HaveCount(1);
        }

        [Fact]
        public void Get_or_add_two_monitors_of_same_type_with_same_tags_returns_the_first_registrated_instance()
        {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor();
            _evictingRegistry.GetOrAdd(monitor.Config, _ => monitor);

            var result = _evictingRegistry.GetOrAdd(anotherMonitor.Config, _ => anotherMonitor);

            result.Should().BeSameAs(monitor);
        }

        [Fact]
        public void Get_or_add_two_monitors_of_same_type_with_different_tags_result_in_two_registrations()
        {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor(new[] { new Tag("Test", "Test"), });
            _evictingRegistry.GetOrAdd(monitor.Config, _ => monitor);

            _evictingRegistry.GetOrAdd(anotherMonitor.Config, _ => anotherMonitor);

            _evictingRegistry.GetRegisteredMonitors().Should().HaveCount(2);
        }

        [Fact]
        public void Get_or_add_two_monitors_of_same_type_with_differrent_tags_returns_the_newly_registrated_instance()
        {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor(new[] { new Tag("Test", "Test"), });
            _evictingRegistry.GetOrAdd(monitor.Config, _ => monitor);

            var result = _evictingRegistry.GetOrAdd(anotherMonitor.Config, _ => anotherMonitor);

            result.Should().BeSameAs(anotherMonitor);
        }

        [Fact]
        public void Get_or_add_two_monitors_of_different_types_result_in_two_registrations()
        {
            var monitor = new FakeMonitor();
            var anotherMonitor = new FakeMonitor2();

            _evictingRegistry.GetOrAdd(monitor.Config, _ => monitor);
            _evictingRegistry.GetOrAdd(anotherMonitor.Config, _ => anotherMonitor);

            _evictingRegistry.GetRegisteredMonitors().Should().HaveCount(2);
        }

        [Fact]
        public void Monitor_can_be_unregistered()
        {
            var monitor = new FakeMonitor();
            _evictingRegistry.GetOrAdd(monitor.Config, _ => monitor);

            _evictingRegistry.Unregister(monitor);

            _evictingRegistry.GetRegisteredMonitors().Should().BeEmpty();
        }

        [Fact]
        public void Monitor_can_be_unregistered_when_recreated()
        {
            var monitor = new FakeMonitor();
            _evictingRegistry.GetOrAdd(monitor.Config, _ => monitor);

            _evictingRegistry.Unregister(new FakeMonitor());

            _evictingRegistry.GetRegisteredMonitors().Should().BeEmpty();
        }

        [Fact]
        public void Monitor_can_be_unrgistered_when_registry_is_used_as_interface()
        {
            IMonitorRegistry registry = new EvictingRegistry();
            var monitor = new FakeMonitor();
            registry.GetOrAdd(monitor.Config, _ => monitor);

            registry.Unregister(monitor);

            registry.GetRegisteredMonitors().Should().BeEmpty();
        }

        [Fact]
        public void Monitor_is_removed_when_garbage_collected()
        {
            IMonitorRegistry registry = new EvictingRegistry();
            var isolator = new Action(() =>
            {
                var monitor = new FakeMonitor();
                registry.GetOrAdd(monitor.Config, _ => monitor);
            });
            isolator();

            GC.Collect();

            registry.GetRegisteredMonitors().Should().BeEmpty();
        }

        [Fact]
        public void Monitor_is_automatically_removed_when_garbage_collected()
        {
            var registry = new EvictingRegistry(TimeSpan.FromSeconds(1));
            var isolator = new Action(() =>
            {
                var monitor = new FakeMonitor();
                registry.GetOrAdd(monitor.Config, _ => monitor);
            });
            isolator();

            GC.Collect();

            Thread.Sleep(TimeSpan.FromSeconds(5));
            registry.GetAllRegisteredMonitors().Should().BeEmpty();
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

            public IEnumerable<IMeasurement> GetValues()
            {
                throw new System.NotImplementedException();
            }

            public IEnumerable<IMeasurement> GetValuesAndReset()
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

            public IEnumerable<IMeasurement> GetValues()
            {
                throw new System.NotImplementedException();
            }

            public IEnumerable<IMeasurement> GetValuesAndReset()
            {
                throw new System.NotImplementedException();
            }

            public MonitorConfig Config { get; private set; }
        }
    }
}