using System;
using NSubstitute;
using Xunit;
using System.Linq;
using Okanshi;
using FluentAssertions;

namespace Okanshi.Test
{
	public class SlaTimerTest
	{
		private readonly IStopwatch stopwatch = Substitute.For<IStopwatch>();
		private SlaTimer timer;

		public SlaTimerTest()
		{
			DefaultMonitorRegistry.Instance.Clear();
			timer = new SlaTimer(MonitorConfig.Build("Test"), () => stopwatch, TimeSpan.FromSeconds(3));
		}

		[Fact]
		public void Can_get_timer_from_okanshimonitor()
		{
			timer = OkanshiMonitor.SlaTimer("name", TimeSpan.FromSeconds(2));
			timer.Should().NotBeNull();
		}

		[Fact]
		public void Threshold_is_set_as_label()
		{
			timer = OkanshiMonitor.SlaTimer("name", TimeSpan.FromSeconds(2));
			var thresholdTag = timer.Config.Tags.Single(x => x.Key == SlaTimerConstants.ThresholdKey).Value;
			thresholdTag.Should().Be("2000");
		}

		IMeasurement[] emptyMeasurement = new IMeasurement[] 
		{
			new Measurement<double>("within.sla.value", 0),
			new Measurement<long>("within.sla.totalTime", 0),
			new Measurement<long>("within.sla.count", 0),
			new Measurement<long>("within.sla.max", 0),
			new Measurement<long>("within.sla.min", 0),

			new Measurement<double>("above.sla.value", 0),
			new Measurement<long>("above.sla.totalTime", 0),
			new Measurement<long>("above.sla.count", 0),
			new Measurement<long>("above.sla.max", 0),
			new Measurement<long>("above.sla.min", 0)
		};

		[Fact]
		public void When_no_recordings_all_measurements_are_0()
		{
			var values = timer.GetValues().ToArray();

			values.ShouldAllBeEquivalentTo(emptyMeasurement);
		}

		[Fact]
		public void When_call_GetValuesAndReset_reset_internals()
		{
			timer.Register(TimeSpan.FromSeconds(1));
			var values = timer.GetValuesAndReset().ToArray();
			
			values = timer.GetValuesAndReset().ToArray();

			values.ShouldAllBeEquivalentTo(emptyMeasurement);
		}


		[Fact]
		public void When_recordings_above_SLA_then_above_values_are_incremented()
		{
			timer.Register(TimeSpan.FromSeconds(1));
			timer.Register(TimeSpan.FromSeconds(3));

			var values = timer.GetValues().ToArray();

			var expected = new IMeasurement[] 
			{
				new Measurement<double>("within.sla.value", 2000),
				new Measurement<long>("within.sla.totalTime", 4000),
				new Measurement<long>("within.sla.count", 2),
				new Measurement<long>("within.sla.max", 3000),
				new Measurement<long>("within.sla.min", 1000),
				
				new Measurement<double>("above.sla.value", 0),
				new Measurement<long>("above.sla.totalTime", 0),
				new Measurement<long>("above.sla.count", 0),
				new Measurement<long>("above.sla.max", 0),
				new Measurement<long>("above.sla.min", 0)
			};
			values.ShouldAllBeEquivalentTo(expected);
		}

		[Fact]
		public void When_recordings_below_SLA_then_below_values_are_incremented()
		{
			timer.Register(TimeSpan.FromSeconds(4));
			timer.Register(TimeSpan.FromSeconds(5));

			var values = timer.GetValues().ToArray();

			var expected = new IMeasurement[] 
			{
				new Measurement<double>("within.sla.value", 0),
				new Measurement<long>("within.sla.totalTime", 0),
				new Measurement<long>("within.sla.count", 0),
				new Measurement<long>("within.sla.max", 0),
				new Measurement<long>("within.sla.min", 0),
				
				new Measurement<double>("above.sla.value", 4500),
				new Measurement<long>("above.sla.totalTime", 9000),
				new Measurement<long>("above.sla.count", 2),
				new Measurement<long>("above.sla.max", 5000),
				new Measurement<long>("above.sla.min", 4000)
			};
			values.ShouldAllBeEquivalentTo(expected);
		}
	}
}