using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Xunit;
using NSubstitute;

namespace Okanshi.Test
{
    public class ApdexTestFromTimer
    {
        private readonly IStopwatch stopwatch = Substitute.For<IStopwatch>();
        private readonly ApdexTimer timer;

        public ApdexTestFromTimer()
        {
            DefaultMonitorRegistry.Instance.Clear();
            timer = new ApdexTimer(MonitorConfig.Build("Test"), () => stopwatch, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Initial_max_value_is_zero()
        {
            var max = timer.GetMax();

            max.Value.Should().Be(0);
        }

        [Fact]
        public void Initial_min_value_is_zero()
        {
            var min = timer.GetMin();

            min.Value.Should().Be(0);
        }

        [Fact]
        public void Initial_count_value_is_zero()
        {
            var count = timer.GetCount();

            count.Value.Should().Be(0);
        }

        [Fact]
        public void Initial_total_time_is_zero()
        {
            var totalTime = timer.GetTotalTime();

            totalTime.Value.Should().Be(0);
        }

        [Fact]
        public void Initial_value_is_zero()
        {
            var value = timer.GetValues();

            value.First().Value.Should().Be(0.0);
        }

        [Fact]
        public void Timing_a_call_sets_count()
        {
            timer.GetCount();

            timer.Record(() => { });

            timer.GetCount().Value.Should().Be(1);
        }

        [Fact]
        public void Timing_a_call_sets_max()
        {
            timer.GetCount();
            stopwatch.Time(Arg.Any<Action>()).Returns(TimeSpan.FromMilliseconds(50));
            timer.Record(() => { });

            var max = timer.GetMax();

            max.Value.Should().Be(50);
        }

        [Fact]
        public void Timing_a_call_sets_min()
        {
            timer.GetCount();
            stopwatch.Time(Arg.Any<Action>()).Returns(TimeSpan.FromMilliseconds(50));
            timer.Record(() => { });

            var min = timer.GetMin();

            min.Value.Should().Be(50);
        }

        [Fact]
        public void Timing_a_call_sets_total_time()
        {
            timer.GetTotalTime();
            stopwatch.Time(Arg.Any<Action>()).Returns(TimeSpan.FromMilliseconds(50));
            timer.Record(() => { });

            var totalTime = timer.GetTotalTime();

            totalTime.Value.Should().Be(50);
        }

        [Fact]
        public void Get_and_reset_resets_count()
        {
            timer.GetCount();
            timer.Record(() => { });

            timer.GetValuesAndReset().ToList();

            timer.GetCount().Value.Should().Be(0);
        }

        [Fact]
        public void Get_and_reset_resets_max()
        {
            timer.GetCount();
            timer.Record(() => { });

            timer.GetValuesAndReset();

            timer.GetMax().Value.Should().Be(0);
        }

        [Fact]
        public void Get_and_reset_resets_min()
        {
            timer.GetCount();
            timer.Record(() => { });

            timer.GetValuesAndReset();

            timer.GetMin().Value.Should().Be(0);
        }

        [Fact]
        public void Get_and_reset_resets_total_time()
        {
            timer.GetTotalTime();
            timer.Record(() => { });

            timer.GetValuesAndReset();

            timer.GetTotalTime().Value.Should().Be(0);
        }

        [Fact]
        public void Manual_timing_sets_count()
        {
            timer.GetCount();
            var okanshiTimer = timer.Start();
            stopwatch.IsRunning.Returns(true);
            okanshiTimer.Stop();

            timer.GetCount().Value.Should().Be(1);
        }

        [Fact]
        public void Manual_timing_sets_max()
        {
            timer.GetCount();
            var okanshiTimer = timer.Start();
            stopwatch.IsRunning.Returns(true);
            stopwatch.Elapsed.Returns(TimeSpan.FromMilliseconds(50));
            okanshiTimer.Stop();

            var max = timer.GetMax();

            max.Value.Should().Be(50);
        }

        [Fact]
        public void Manual_timing_sets_min()
        {
            timer.GetCount();
            var okanshiTimer = timer.Start();
            stopwatch.IsRunning.Returns(true);
            stopwatch.Elapsed.Returns(TimeSpan.FromMilliseconds(50));
            okanshiTimer.Stop();

            var min = timer.GetMin();

            min.Value.Should().Be(50);
        }

        [Fact]
        public void Manual_timing_sets_total_time()
        {
            timer.GetTotalTime();
            var okanshiTimer = timer.Start();
            stopwatch.IsRunning.Returns(true);
            stopwatch.Elapsed.Returns(TimeSpan.FromMilliseconds(50));
            okanshiTimer.Stop();

            var totalTime = timer.GetTotalTime();

            totalTime.Value.Should().Be(50);
        }

        [Fact]
        public void Manual_registration_updates_sets_max()
        {
            const long elapsed = 1000;

            timer.Register(TimeSpan.FromMilliseconds(elapsed));

            timer.GetMax().Value.Should().Be(elapsed);
        }

        [Fact]
        public void Manual_registration_updates_sets_min()
        {
            const long elapsed = 1000;

            timer.Register(TimeSpan.FromMilliseconds(elapsed));

            timer.GetMin().Value.Should().Be(elapsed);
        }

        [Fact]
        public void Manual_registration_sets_count()
        {
            const long elapsed = 1000;
            timer.Register(TimeSpan.FromMilliseconds(elapsed));

            timer.GetCount().Value.Should().Be(1);
        }

        [Fact]
        public void Manual_registration_sets_total_time()
        {
            const long elapsed = 1000;

            timer.Register(TimeSpan.FromMilliseconds(elapsed));

            timer.GetTotalTime().Value.Should().Be(elapsed);
        }

        [Fact]
        public void Values_are_correct()
        {
            var values = timer.GetValues().Select(x => x.Name);
            values.Should().BeEquivalentTo("value", "max", "min", "count", "totalTime", "apdex");
        }
    }

	public class ApdexTest
	{
		private readonly IStopwatch stopwatch = Substitute.For<IStopwatch>();
		private readonly ApdexTimer timer;

		public ApdexTest()
		{
			DefaultMonitorRegistry.Instance.Clear();
			timer = new ApdexTimer(MonitorConfig.Build("Test"), TimeSpan.FromSeconds(1));
		}

		[Fact]
		public void ctor_adds_threshold_as_tag_if_absent()
		{
			timer.Config.Tags.Single(x => x.Key == ApdexConstants.ThresholdKey);
		}

		[Fact]
		public void AppdexCalc_zero_calls()
		{
			timer.GetApDex().Should().Be(1);
		}

		[Fact]
		public void AppdexCalc_satisfied_one_call()
		{
			timer.Register(TimeSpan.FromMilliseconds(900));

			timer.GetApDex().Should().Be(1.0);
		}

		[Fact]
		public void AppdexCalc_satisfied_many_calls()
		{
			timer.Register(TimeSpan.FromMilliseconds(500));
			timer.Register(TimeSpan.FromMilliseconds(600));
			timer.Register(TimeSpan.FromMilliseconds(700));
			timer.Register(TimeSpan.FromMilliseconds(900));

			timer.GetApDex().Should().Be(1.0);
		}

		[Fact]
		public void AppdexCalc_tolerable_one_call()
		{
			timer.Register(TimeSpan.FromMilliseconds(1200));

			timer.GetApDex().Should().Be(0.5);
		}

		[Fact]
		public void AppdexCalc_tolerable_many_calls()
		{
			timer.Register(TimeSpan.FromMilliseconds(1500));
			timer.Register(TimeSpan.FromMilliseconds(2600));
			timer.Register(TimeSpan.FromMilliseconds(3700));
			timer.Register(TimeSpan.FromMilliseconds(3900));

			timer.GetApDex().Should().Be(0.5);
		}

		[Fact]
		public void AppdexCalc_frustrated_one_call()
		{
			timer.Register(TimeSpan.FromMilliseconds(4000));

			timer.GetApDex().Should().Be(0);
		}

		[Fact]
		public void AppdexCalc_frustrated_many_calls()
		{
			timer.Register(TimeSpan.FromMilliseconds(4500));
			timer.Register(TimeSpan.FromMilliseconds(5600));
			timer.Register(TimeSpan.FromMilliseconds(6700));
			timer.Register(TimeSpan.FromMilliseconds(7900));

			timer.GetApDex().Should().Be(0);
		}

		[Fact]
		public void AppdexCalc_one_of_each_kind()
		{
			timer.Register(TimeSpan.FromMilliseconds(900));
			timer.Register(TimeSpan.FromMilliseconds(2000));
			timer.Register(TimeSpan.FromMilliseconds(4000));

			timer.GetApDex().Should().Be(0.5);
		}

		[Fact]
		public void AppdexCalc_rounded_to_2_decimals()
		{
			timer.Register(TimeSpan.FromMilliseconds(900));
			timer.Register(TimeSpan.FromMilliseconds(900));
			timer.Register(TimeSpan.FromMilliseconds(2000));
			timer.Register(TimeSpan.FromMilliseconds(4000));

			timer.GetApDex().Should().Be(0.63);
		}

		[Fact]
		public void Record_func_should_register()
		{
			var i = timer.Record(() => 1);

			i.Should().Be(1);
			timer.GetApDex().Should().Be(1);
		}

		[Fact]
		public void Record_action_should_register()
		{
			int i = 0;
			timer.Record(() => i = 1);

			i.Should().Be(1);
			timer.GetApDex().Should().Be(1);
		}

		[Fact]
		public void Values_are_correct()
		{
			var values = timer.GetValues().Select(x => x.Name);
			values.Should().BeEquivalentTo("value", "max", "min", "count", "totalTime", "apdex");
		}
	}
}