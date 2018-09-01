using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Xunit;
using NSubstitute;

namespace Okanshi.Test
{
    public class TimerTest
    {
        private readonly IStopwatch stopwatch = Substitute.For<IStopwatch>();
        private readonly Timer timer;

        public TimerTest()
        {
            DefaultMonitorRegistry.Instance.Clear();
            timer = new Timer(MonitorConfig.Build("Test"), () => stopwatch);
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
        public void Manual_registration_with_long_sets_total_time()
        {
            const long elapsed = 1000;

            timer.Register(TimeSpan.FromMilliseconds(elapsed));

            timer.GetTotalTime().Value.Should().Be(elapsed);
        }

        [Fact]
        public void Manual_registration_with_timespan_sets_total_time()
        {
            timer.Register(TimeSpan.FromSeconds(1));

            timer.GetTotalTime().Value.Should().Be(1000);
        }


        [Fact]
        public void Manual_registration_with_stopwatch_sets_total_time()
        {
            var sw = Stopwatch.StartNew();
            Thread.Sleep(50);
            sw.Stop();

            timer.RegisterElapsed(sw);

            timer.GetTotalTime().Value.Should().Be(sw.ElapsedMilliseconds);
        }

        [Fact]
        public void Manual_registrations_accumulate_total_time()
        {
            const long elapsed = 1000;

            timer.Register(TimeSpan.FromMilliseconds(elapsed));
            timer.Register(TimeSpan.FromMilliseconds(elapsed));

            timer.GetTotalTime().Value.Should().Be(2 * elapsed);
        }

        [Fact]
        public void Values_are_correct()
        {
            var values = timer.GetValues().Select(x => x.Name);
            values.Should().BeEquivalentTo("value", "max", "min", "count", "totalTime");
        }
    }
}