using System;
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
		public void Registering_two_different_monitors_results_in_two_registrations()
		{
			var monitor = new FakeMonitor();
			var anotherMonitor = new FakeMonitor();
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
		public void Generating_keys_for_two_different_monitors_with_different_tags_results_in_different_keys()
		{
			var first = OkanshiMonitor.GetMonitorKey("test", typeof(int), new[] { new Tag("name", "test12") });
			var second = OkanshiMonitor.GetMonitorKey("test", typeof(int), new[] { new Tag("name", "") });
			first.Should().NotBe(second);
		}

		[Fact]
		public void Generating_keys_for_two_monitors_with_same_name_and_tags_results_in_equal_keys()
		{
			var first = OkanshiMonitor.GetMonitorKey("test", typeof(int), new[] { new Tag("name", "test12") });
			var second = OkanshiMonitor.GetMonitorKey("test", typeof(int), new[] { new Tag("name", "test12") });
			first.Should().Be(second);
		}

		private class FakeMonitor : IMonitor
		{
			public FakeMonitor()
			{
				Config = MonitorConfig.Build("test");
			}

			public FakeMonitor(string name)
			{
				Config = MonitorConfig.Build(name);
			}

			public object GetValue()
			{
				throw new System.NotImplementedException();
			}

			public MonitorConfig Config { get; private set; }
		}
	}
}
