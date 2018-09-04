using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class ApdexTest
	{
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
		public void ctor_fails_when_threshold_tag_is_present()
		{
			var forbiddenTag = new Tag(ApdexConstants.ThresholdKey, "");
			var config = MonitorConfig.Build("test").WithTag(forbiddenTag);

			Action act = () => new ApdexTimer(config, TimeSpan.FromHours(2));
			act.ShouldThrow<ArgumentException>();
		}

		[Fact]
		public void AppdexCalc_zero_registrations_return_all_ok()
		{
			timer.GetApdex().Should().Be(1);
			timer.GetValues().Count(x => x.Name == "apdex").Should().Be(1);
		}

		[Fact]
		public void AppdexCalc_zero_registrations_return_all_ok_several_times()
		{
			timer.GetApdex().Should().Be(1);
			timer.GetApdex().Should().Be(1);
			timer.GetApdex().Should().Be(1);
			timer.GetApdex().Should().Be(1);
		}

		[Fact]
		public void GetApdex_when_1_satisfied_one_call_returns_1()
		{
			timer.Register(TimeSpan.FromMilliseconds(900));

			timer.GetApdex().Should().Be(1.0);
		}

		[Fact]
		public void GetApdex_when_several_satisfied_calls_returns_1()
		{
			timer.Register(TimeSpan.FromMilliseconds(500));
			timer.Register(TimeSpan.FromMilliseconds(600));
			timer.Register(TimeSpan.FromMilliseconds(700));
			timer.Register(TimeSpan.FromMilliseconds(900));

			timer.GetApdex().Should().Be(1.0);
		}

		[Fact]
		public void GetApdex_when_tolerable_one_call_returns_half()
		{
			timer.Register(TimeSpan.FromMilliseconds(1200));

			timer.GetApdex().Should().Be(0.5);
		}

		[Fact]
		public void GetApdex_when_tolerable_many_call_returns_half()
		{
			timer.Register(TimeSpan.FromMilliseconds(1500));
			timer.Register(TimeSpan.FromMilliseconds(2600));
			timer.Register(TimeSpan.FromMilliseconds(3700));
			timer.Register(TimeSpan.FromMilliseconds(3900));

			timer.GetApdex().Should().Be(0.5);
		}

		[Fact]
		public void GetApdex_when_frustrating_one_call_returns_zero()
		{
			timer.Register(TimeSpan.FromMilliseconds(4000));

			timer.GetApdex().Should().Be(0);
		}

		[Fact]
		public void GetApdex_when_frustrating_many_call_returns_zero()
		{
			timer.Register(TimeSpan.FromMilliseconds(4500));
			timer.Register(TimeSpan.FromMilliseconds(5600));
			timer.Register(TimeSpan.FromMilliseconds(6700));
			timer.Register(TimeSpan.FromMilliseconds(7900));

			timer.GetApdex().Should().Be(0);
		}

		[Fact]
		public void GetApdex_one_of_each_kind_returns_half()
		{
			timer.Register(TimeSpan.FromMilliseconds(900));
			timer.Register(TimeSpan.FromMilliseconds(2000));
			timer.Register(TimeSpan.FromMilliseconds(4000));

			timer.GetApdex().Should().Be(0.5);
		}

		[Fact]
		public void GetApdex_rounded_to_2_decimals()
		{
			timer.Register(TimeSpan.FromMilliseconds(900));
			timer.Register(TimeSpan.FromMilliseconds(900));
			timer.Register(TimeSpan.FromMilliseconds(2000));
			timer.Register(TimeSpan.FromMilliseconds(4000));

			timer.GetApdex().Should().Be(0.63);
		}

		[Fact]
		public void GetApdex_when_no_registrations_are_made_the_previous_score_is_returned()
		{
			// zero registrations
			timer.GetApdex().Should().Be(1);
			timer.GetApdex().Should().Be(1);

			// one registration changes the score
			timer.Register(TimeSpan.FromMilliseconds(4000));
			timer.GetApdex().Should().Be(0);
			timer.GetApdex().Should().Be(0);

			// another registration changes the score
			timer.Register(TimeSpan.FromMilliseconds(900));
			timer.GetApdex().Should().Be(0.5);
			timer.GetApdex().Should().Be(0.5);
		}

		[Fact]
		public void Record_func_should_register()
		{
			var i = timer.Record(() => 1);

			i.Should().Be(1);
			timer.GetApdex().Should().Be(1);
		}

		[Fact]
		public void Record_action_should_register()
		{
			int i = 0;
			timer.Record(() => i = 1);

			i.Should().Be(1);
			timer.GetApdex().Should().Be(1);
		}
	}
}