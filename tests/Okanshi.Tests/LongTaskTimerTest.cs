using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class LongTaskTimerTest
    {
        private readonly LongTaskTimer timer;

        public LongTaskTimerTest()
        {
            DefaultMonitorRegistry.Instance.Clear();
            timer = new LongTaskTimer(MonitorConfig.Build("Test"));
        }

        [Fact]
        public void Intial_number_of_active_tasks_is_zero()
        {
            var numberOfActiveTasks = timer.GetNumberOfActiveTasks();

            numberOfActiveTasks.Value.Should().Be(0);
        }

        [Fact]
        public void Intial_duration_is_zero()
        {
            var duration = timer.GetDurationInSeconds();

            duration.Value.Should().Be(0);
        }

        [Fact]
        public void Intial_value_is_zero()
        {
            var value = timer.GetValues();

            value.First().Value.Should().Be(0);
        }

        [Fact]
        public void Recording_a_task_increments_the_number_of_active_tasks()
        {
            var task = Task.Run(() => timer.Record(() => Thread.Sleep(2000)));
            Thread.Sleep(100);

            var numberOfActiveTasks = timer.GetNumberOfActiveTasks();

            task.Wait();
            numberOfActiveTasks.Value.Should().Be(1);
        }

        [Fact]
        public void Recording_a_task_updates_the_duration()
        {
            var task = Task.Run(() => timer.Record(() => Thread.Sleep(1000)));
            Thread.Sleep(500);

            var duration = timer.GetDurationInSeconds();

            duration.Value.Should().BeApproximately(0.5, 0.3);
            task.Wait();
        }

        [Fact]
        public void Config_is_suffixed_with_duration()
        {
            var monitorConfig = timer.Config;

            monitorConfig.Name.Should().EndWith(".duration");
        }

        [Fact]
        public void Manual_timing_of_a_task_increments_the_number_of_active_tasks()
        {
            var task = Task.Run(() =>
            {
                var okanshiTimer = timer.Start();
                Thread.Sleep(1000);
                okanshiTimer.Stop();
            });
            Thread.Sleep(100);

            var numberOfActiveTasks = timer.GetNumberOfActiveTasks();

            task.Wait();
            numberOfActiveTasks.Value.Should().Be(1);
        }

        [Fact]
        public void Manual_timing_of_a_task_updates_the_duration()
        {
            var task = Task.Run(() =>
            {
                var okanshiTimer = timer.Start();
                Thread.Sleep(1000);
                okanshiTimer.Stop();
            });
            Thread.Sleep(500);

            var duration = timer.GetDurationInSeconds();

            duration.Value.Should().BeApproximately(0.5, 0.3);
            task.Wait();
        }

        [Fact]
        public void Manual_registration_is_not_supported()
        {
            Action action = () => timer.Register(1000);

            action.ShouldThrow<NotSupportedException>();
        }

        [Fact]
        public void Average_value_is_called_value()
        {
            var values = timer.GetValues();

            values.Should().Contain(x => x.Name == "duration");
            values.Should().Contain(x => x.Name == "activeTasks");
        }
    }
}