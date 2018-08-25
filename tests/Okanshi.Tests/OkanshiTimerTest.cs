using System;
using System.Threading;
using FluentAssertions;
using Xunit;
using NSubstitute;

namespace Okanshi.Test
{
    public class OkanshiTimerTest
    {
        private readonly IStopwatch _stopwatch = Substitute.For<IStopwatch>();
        private readonly OkanshiTimer _timer;

        public OkanshiTimerTest() {
            _timer = new OkanshiTimer(x => {}, () => _stopwatch);
        }

        [Fact]
        public void Cannot_be_started_multiple_times()
        {
            _timer.Start();
            _stopwatch.IsRunning.Returns(true);

            Action start = () => _timer.Start();

            start.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void Cannot_be_stopped_multiple_times()
        {
            _timer.Start();
            _stopwatch.IsRunning.Returns(true);
            _timer.Stop();
            _stopwatch.IsRunning.Returns(false);

            Action stop = () => _timer.Stop();

            stop.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void Cannot_be_stopped_if_not_started()
        {
            Action stop = () => _timer.Stop();

            stop.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void Cannot_be_started_if_already_used_once()
        {
            _timer.Start();
            _stopwatch.IsRunning.Returns(true);
            _timer.Stop();
            _stopwatch.IsRunning.Returns(false);

            Action start = () => _timer.Start();

            start.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void Timer_calls_the_callback_with_elapsed_milliseconds()
        {
            var elapsed = TimeSpan.Zero;
            var timer = new OkanshiTimer(x => elapsed = x);

            timer.Start();
            Thread.Sleep(500);
            timer.Stop();

            elapsed.TotalMilliseconds.Should().BeInRange(400, 700);
        }
    }
}