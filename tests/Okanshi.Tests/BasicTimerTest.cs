using System;
using System.Linq;
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

            timer.Record(() => Thread.Sleep(500));

            timer.GetCount().Value.Should().Be(1);
        }

        [Fact]
        public void Timing_a_call_sets_max()
        {
            timer.GetCount();
            timer.Record(() => Thread.Sleep(50));

            var max = timer.GetMax();

            max.Value.Should().BeInRange(40, 70);
        }

        [Fact]
        public void Timing_a_call_sets_min()
        {
            timer.GetCount();
            timer.Record(() => Thread.Sleep(50));

            var min = timer.GetMin();

            min.Value.Should().BeInRange(40, 70);
        }

        [Fact]
        public void Timing_a_call_sets_total_time()
        {
            timer.GetTotalTime();
            timer.Record(() => Thread.Sleep(500));

            var totalTime = timer.GetTotalTime();

            totalTime.Value.Should().BeInRange(480, 520);
        }

        [Fact]
        public void Get_and_reset_resets_count()
        {
            timer.GetCount();
            timer.Record(() => Thread.Sleep(500));

            timer.GetValuesAndReset().ToList();

            timer.GetCount().Value.Should().Be(0);
        }

        [Fact]
        public void Get_and_reset_resets_max()
        {
            timer.GetCount();
            timer.Record(() => Thread.Sleep(50));

            timer.GetValuesAndReset();

            timer.GetMax().Value.Should().Be(0);
        }

        [Fact]
        public void Get_and_reset_resets_min()
        {
            timer.GetCount();
            timer.Record(() => Thread.Sleep(50));

            timer.GetValuesAndReset();

            timer.GetMin().Value.Should().Be(0);
        }

        [Fact]
        public void Get_and_reset_resets_total_time()
        {
            timer.GetTotalTime();
            timer.Record(() => Thread.Sleep(500));

            timer.GetValuesAndReset();

            timer.GetTotalTime().Value.Should().Be(0);
        }

        [Fact]
        public void Manual_timing_sets_count()
        {
            timer.GetCount();

            var okanshiTimer = timer.Start();
            Thread.Sleep(50);
            okanshiTimer.Stop();

            timer.GetCount().Value.Should().Be(1);
        }

        [Fact]
        public void Manual_timing_sets_max()
        {
            timer.GetCount();
            var okanshiTimer = timer.Start();
            Thread.Sleep(50);
            okanshiTimer.Stop();

            var max = timer.GetMax();

            max.Value.Should().BeInRange(40, 70);
        }

        [Fact]
        public void Manual_timing_sets_min()
        {
            timer.GetCount();
            var okanshiTimer = timer.Start();
            Thread.Sleep(50);
            okanshiTimer.Stop();

            var min = timer.GetMin();

            min.Value.Should().BeInRange(40, 70);
        }

        [Fact]
        public void Manual_timing_sets_total_time()
        {
            timer.GetTotalTime();
            var okanshiTimer = timer.Start();
            Thread.Sleep(500);
            okanshiTimer.Stop();

            var totalTime = timer.GetTotalTime();

            totalTime.Value.Should().BeInRange(480, 520);
        }

        [Fact]
        public void Manual_registration_updates_sets_max()
        {
            const long elapsed = 1000;

            timer.Register(elapsed);

            timer.GetMax().Value.Should().Be(elapsed);
        }

        [Fact]
        public void Manual_registration_updates_sets_min()
        {
            const long elapsed = 1000;

            timer.Register(elapsed);

            timer.GetMin().Value.Should().Be(elapsed);
        }

        [Fact]
        public void Manual_registration_sets_count()
        {
            const long elapsed = 1000;
            timer.Register(elapsed);

            timer.GetCount().Value.Should().Be(1);
        }

        [Fact]
        public void Manual_registration_sets_total_time()
        {
            const long elapsed = 1000;

            timer.Register(elapsed);

            timer.GetTotalTime().Value.Should().Be(elapsed);
        }
    }
}