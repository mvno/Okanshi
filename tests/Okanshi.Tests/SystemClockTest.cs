using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class SystemClockTest
    {
        private readonly SystemClock systemClock;

        public SystemClockTest()
        {
            systemClock = new SystemClock();
        }

        [Fact]
        public void Now_returns_utc_now()
        {
            var now = systemClock.Now();

            now.Should().BeCloseTo(DateTime.UtcNow);
        }

        [Fact]
        public void NowTicks_returns_utc_now_ticks()
        {
            var now = systemClock.NowTicks();

            now.Should().BeInRange(DateTime.UtcNow.AddSeconds(-1).Ticks, DateTime.UtcNow.AddSeconds(1).Ticks);
        }

        [Fact]
        public void Freeze_freezes_the_clock()
        {
            var now = DateTime.UtcNow;
            systemClock.Freeze();

            var frozenTime = systemClock.Now();

            Thread.Sleep(1000);
            frozenTime.Should().BeCloseTo(now);
        }

        [Fact]
        public void Unfreeze_starts_the_clock_again()
        {
            systemClock.Freeze();
            Thread.Sleep(1000);

            systemClock.Unfreeze();
            systemClock.Now().Should().BeCloseTo(DateTime.UtcNow);
        }
    }
}