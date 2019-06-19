using System.Collections.Generic;
using System.Diagnostics;
using Autofac.Core;

namespace Okanshi.Autofac
{
    class CountAndTimingEventHandler
    {
        private readonly OkanshiAutofacOptions options;
        private readonly Dictionary<IInstanceActivator, TimerInfo> timers = new Dictionary<IInstanceActivator, TimerInfo>();

        public CountAndTimingEventHandler(OkanshiAutofacOptions options)
        {
            this.options = options;
        }

        internal void TimerActicating(object sender, ActivatingEventArgs<object> args)
        {
            var activator = args.Component.Activator;
            if (timers.TryGetValue(activator, out var timerInfo))
            {
                timerInfo.Watch.Reset();
            }
            else
            {
                var tags = new[] { new Tag("Type", activator.LimitType.ToString()) };
                var okanshiTimer = options.TimerFactory(options.MetricName, tags);
                timers[activator] = new TimerInfo(okanshiTimer);
            }
        }

        internal void TimerActivated(object sender, ActivatedEventArgs<object> args)
        {
            var a = args.Component.Activator;
            if (timers.TryGetValue(a, out var watch))
            {
                watch.Timer.RegisterElapsed(watch.Watch);
            }
        }

        class TimerInfo
        {
            public readonly Stopwatch Watch = Stopwatch.StartNew();
            public readonly ITimer Timer;

            public TimerInfo(ITimer timer)
            {
                Timer = timer;
            }
        }
    }
}