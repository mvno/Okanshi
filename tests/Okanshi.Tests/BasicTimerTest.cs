using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class BasicTimerTest
	{
		[Fact]
		public void Initial_max_value_is_zero()
		{
			var timer = new BasicTimer(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));

			var max = timer.GetMax();

			max.Should().Be(0);
		}

		[Fact]
		public void Initial_min_value_is_zero()
		{
			var timer = new BasicTimer(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));

			var min = timer.GetMin();

			min.Should().Be(0);
		}

		[Fact]
		public void Initial_count_value_is_zero()
		{
			var timer = new BasicTimer(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));

			var count = timer.GetCount();

			count.Should().Be(0);
		}

		[Fact]
		public void Initial_total_time_is_zero()
		{
			var timer = new BasicTimer(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));

			var totalTime = timer.GetTotalTime();

			totalTime.Should().Be(0);
		}

		[Fact]
		public void Initial_value_is_zero()
		{
			var timer = new BasicTimer(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));

			var value = timer.GetValue();

			value.Should().Be(0);
		}

		[Fact]
		public void Timing_a_call_sets_count_per_second_when_step_is_crossed()
		{
			var timer = new BasicTimer(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));
			timer.GetCount();

			timer.Record(() => Thread.Sleep(50));

			Thread.Sleep(600);
			timer.GetCount().Should().Be(2);
		}

		[Fact]
		public void Timing_a_call_sets_max()
		{
			var timer = new BasicTimer(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));
			timer.GetCount();
			timer.Record(() => Thread.Sleep(50));

			var max = timer.GetMax();

			max.Should().BeInRange(40, 60);
		}

		[Fact]
		public void Timing_a_call_sets_min()
		{
			var timer = new BasicTimer(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));
			timer.GetCount();
			timer.Record(() => Thread.Sleep(50));

			var min = timer.GetMin();

			min.Should().BeInRange(40, 60);
		}

		[Fact]
		public void Timing_a_call_sets_total_time_per_second_when_step_is_crossed()
		{
			var timer = new BasicTimer(MonitorConfig.Build("Test"), TimeSpan.FromMilliseconds(500));
			timer.GetTotalTime();
			timer.Record(() => Thread.Sleep(50));
			Thread.Sleep(600);

			var totalTime = timer.GetTotalTime();

			totalTime.Should().BeInRange(90, 110);
		}
	}
}