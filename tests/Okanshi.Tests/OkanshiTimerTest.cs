using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class OkanshiTimerTest
	{
		private readonly OkanshiTimer _timer = new OkanshiTimer(x => { });

		public OkanshiTimerTest()
		{
		}

		[Fact]
		public void Cannot_be_started_multiple_times()
		{
			_timer.Start();

			Action start = () => _timer.Start();

			start.ShouldThrow<InvalidOperationException>();
		}

		[Fact]
		public void Cannot_be_stopped_multiple_times()
		{
			_timer.Start();
			_timer.Stop();

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
			_timer.Stop();

			Action start = () => _timer.Start();

			start.ShouldThrow<InvalidOperationException>();
		}

		[Fact]
		public void Timer_calls_the_callback_with_elapsed_milliseconds()
		{
			var elapsedMilliseconds = 0L;
			var timer = new OkanshiTimer(x => elapsedMilliseconds = x);

			timer.Start();
			Thread.Sleep(500);
			timer.Stop();

			elapsedMilliseconds.Should().BeInRange(400, 700);
		}
	}
}