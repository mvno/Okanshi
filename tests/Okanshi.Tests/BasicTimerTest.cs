using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class BasicTimerTest
	{
	    private readonly BasicTimer timer;
	    private readonly ManualClock manualClock = new ManualClock();

	    public BasicTimerTest()
	    {
	        timer = new BasicTimer(MonitorConfig.Build("Test"), TimeSpan.FromSeconds(1), manualClock);
	    }

		[Fact]
		public void Initial_max_value_is_zero()
		{
			var max = timer.GetMax();

			max.Should().Be(0);
		}

		[Fact]
		public void Initial_min_value_is_zero()
		{
			var min = timer.GetMin();

			min.Should().Be(0);
		}

		[Fact]
		public void Initial_count_value_is_zero()
		{
			var count = timer.GetCount();

			count.Should().Be(0);
		}

		[Fact]
		public void Initial_total_time_is_zero()
		{
			var totalTime = timer.GetTotalTime();

			totalTime.Should().Be(0);
		}

		[Fact]
		public void Initial_value_is_zero()
		{
			var value = timer.GetValue();

			value.Should().Be(0);
		}

		[Fact]
		public void Timing_a_call_sets_count_per_second_when_step_is_crossed()
		{
			timer.GetCount();

			timer.Record(() => Thread.Sleep(500));

            manualClock.Advance(TimeSpan.FromSeconds(1));
			timer.GetCount().Should().Be(1);
		}

		[Fact]
		public void Timing_a_call_sets_max()
		{
			timer.GetCount();
			timer.Record(() => Thread.Sleep(50));

			var max = timer.GetMax();

			max.Should().BeInRange(40, 70);
		}

		[Fact]
		public void Timing_a_call_sets_min()
		{
			timer.GetCount();
			timer.Record(() => Thread.Sleep(50));

			var min = timer.GetMin();

			min.Should().BeInRange(40, 70);
		}

		[Fact]
		public void Timing_a_call_sets_total_time_per_second_when_step_is_crossed()
		{
			timer.GetTotalTime();
			timer.Record(() => Thread.Sleep(500));
            manualClock.Advance(TimeSpan.FromMilliseconds(1100));

			var totalTime = timer.GetTotalTime();

			totalTime.Should().BeInRange(480, 520);
		}

		[Fact]
		public void Manual_timing_sets_count_per_second_when_step_is_crossed()
		{
			timer.GetCount();

			var okanshiTimer = timer.Start();
			Thread.Sleep(50);
			okanshiTimer.Stop();

            manualClock.Advance(TimeSpan.FromMilliseconds(1100));
            timer.GetCount().Should().Be(1);
		}

		[Fact]
		public void Manual_timing_sets_max()
		{
			timer.GetCount();
			var okanshiTimer = timer.Start();
			Thread.Sleep(50);
			okanshiTimer.Stop();

			var max = timer.GetMax();

			max.Should().BeInRange(40, 70);
		}

		[Fact]
		public void Manual_timing_sets_min()
		{
			timer.GetCount();
			var okanshiTimer = timer.Start();
			Thread.Sleep(50);
			okanshiTimer.Stop();

			var min = timer.GetMin();

			min.Should().BeInRange(40, 70);
		}

		[Fact]
		public void Manual_timing_sets_total_time_per_second_when_step_is_crossed()
		{
			timer.GetTotalTime();
			var okanshiTimer = timer.Start();
			Thread.Sleep(500);
			okanshiTimer.Stop();
            manualClock.Advance(TimeSpan.FromMilliseconds(1100));

			var totalTime = timer.GetTotalTime();

			totalTime.Should().BeInRange(480, 520);
		}

	    [Fact]
	    public void Manual_registration_updates_sets_max()
	    {
	        const long elapsed = 1000;

            timer.Register(elapsed);

	        timer.GetMax().Should().Be(elapsed);
	    }

	    [Fact]
	    public void Manual_registration_updates_sets_min()
	    {
	        const long elapsed = 1000;

            timer.Register(elapsed);

	        timer.GetMin().Should().Be(elapsed);
        }

        [Fact]
        public void Manual_registration_sets_count_per_second_when_step_is_crossed() {
            const long elapsed = 1000;
            timer.Register(elapsed);

            manualClock.Advance(TimeSpan.FromMilliseconds(1100));
            timer.GetCount().Should().Be(1);
        }

        [Fact]
        public void Manual_registration_sets_total_time_per_second_when_step_is_crossed() {
            const long elapsed = 1000;

            timer.Register(elapsed);

            manualClock.Advance(TimeSpan.FromMilliseconds(1100));
            timer.GetTotalTime().Should().Be(elapsed);
        }
    }
}