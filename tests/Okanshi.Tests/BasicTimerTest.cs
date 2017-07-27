using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class BasicTimerTest
	{
	    private readonly BasicTimer timer;

	    public BasicTimerTest()
	    {
            DefaultMonitorRegistry.Instance.Clear();
	        timer = new BasicTimer(MonitorConfig.Build("Test"));
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
		public void Timing_a_call_sets_count()
		{
			timer.GetCount();

			timer.Record(() => Thread.Sleep(500));

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
		public void Timing_a_call_sets_total_time()
		{
			timer.GetTotalTime();
			timer.Record(() => Thread.Sleep(500));

			var totalTime = timer.GetTotalTime();

			totalTime.Should().BeInRange(480, 520);
		}

		[Fact]
		public void Get_and_reset_resets_count()
		{
			timer.GetCount();
            timer.Record(() => Thread.Sleep(500));

		    timer.GetValueAndReset();

			timer.GetCount().Should().Be(0);
		}

		[Fact]
		public void Get_and_reset_resets_max()
		{
			timer.GetCount();
			timer.Record(() => Thread.Sleep(50));

		    timer.GetValueAndReset();

            timer.GetMax().Should().Be(0);
		}

		[Fact]
		public void Get_and_reset_resets_min()
		{
			timer.GetCount();
			timer.Record(() => Thread.Sleep(50));

		    timer.GetValueAndReset();

            timer.GetMin().Should().Be(0);
		}

		[Fact]
		public void Get_and_reset_resets_total_time()
		{
			timer.GetTotalTime();
			timer.Record(() => Thread.Sleep(500));

		    timer.GetValueAndReset();

            timer.GetTotalTime().Should().Be(0);
		}

		[Fact]
		public void Manual_timing_sets_count()
		{
			timer.GetCount();

			var okanshiTimer = timer.Start();
			Thread.Sleep(50);
			okanshiTimer.Stop();

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
		public void Manual_timing_sets_total_time()
		{
			timer.GetTotalTime();
			var okanshiTimer = timer.Start();
			Thread.Sleep(500);
			okanshiTimer.Stop();

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
        public void Manual_registration_sets_count() {
            const long elapsed = 1000;
            timer.Register(elapsed);

            timer.GetCount().Should().Be(1);
        }

        [Fact]
        public void Manual_registration_sets_total_time() {
            const long elapsed = 1000;

            timer.Register(elapsed);

            timer.GetTotalTime().Should().Be(elapsed);
        }
    }
}