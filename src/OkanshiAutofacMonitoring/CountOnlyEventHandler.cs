using System;
using System.Collections.Generic;
using Autofac.Core;

namespace Okanshi.Autofac
{
    class CountOnlyEventHandler
    {
        private readonly OkanshiAutofacOptions options;
        readonly Dictionary<Type, ICounter<long>> counters = new Dictionary<Type, ICounter<long>>();

        public CountOnlyEventHandler(OkanshiAutofacOptions options)
        {
            this.options = options;
        }

        internal void CountActivatingFast(object sender, ActivatingEventArgs<object> args)
        {
            if (!counters.TryGetValue(args.Component.Activator.LimitType, out var counter))
            {
                var tags = new Tag[] { new Tag("Type", args.Component.Activator.LimitType.ToString()) };
                counter = options.CountFactory(options.MetricName, tags);
                counters[args.Component.Activator.LimitType] = counter;
            }

            counter.Increment();
        }
    }
}