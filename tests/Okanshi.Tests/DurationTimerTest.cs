using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class DurationTimerTest
	{
		private readonly DurationTimer _timer;

		public DurationTimerTest()
		{
			_timer = new DurationTimer(MonitorConfig.Build("Test"));
		}

		[Fact]
		public void Intial_number_of_active_tasks_is_zero()
		{
			var numberOfActiveTasks = _timer.GetNumberOfActiveTasks();

			numberOfActiveTasks.Should().Be(0);
		}

		[Fact]
		public void Intial_duration_is_zero()
		{
			var duration = _timer.GetDurationInSeconds();

			duration.Should().Be(0);
		}

		[Fact]
		public void Intial_value_is_zero()
		{
			var value = _timer.GetValue();

			value.Should().Be(0);
		}

		[Fact]
		public void Recording_a_task_increments_the_number_of_active_tasks()
		{
			var task = Task.Run(() => _timer.Record(() => Thread.Sleep(1000)));
			Thread.Sleep(100);

			var numberOfActiveTasks = _timer.GetNumberOfActiveTasks();

			task.Wait();
			numberOfActiveTasks.Should().Be(1);
		}

		[Fact]
		public void Recording_a_task_updates_the_duration()
		{
			var task = Task.Run(() => _timer.Record(() => Thread.Sleep(1000)));
			Thread.Sleep(500);

			var duration = _timer.GetDurationInSeconds();

			duration.Should().BeApproximately(0.5, 0.1);
			task.Wait();
		}

		[Fact]
		public void Config_is_suffixed_with_duration()
		{
			var monitorConfig = _timer.Config;

			monitorConfig.Name.Should().EndWith(".duration");
		}
	}
}