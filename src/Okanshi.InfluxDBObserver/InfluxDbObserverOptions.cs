﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Okanshi.Observers {
    public class InfluxDbObserverOptions {
        public Func<Metric, string> DatabaseSelector { get; set; }

        public Func<Metric, string, string> RetentionPolicySelector { get; set; } = (x, y) => "autogen";

        public Func<Tag, bool> TagToFieldSelector { get; set; } = _ => false;

        public Func<Metric, string> MeasurementNameSelector { get; set; } = metric => metric.Name;

        public IEnumerable<string> TagsToIgnore { get; set; } = Enumerable.Empty<string>();

        public InfluxDbObserverOptions(string databaseName) {
            DatabaseSelector = _ => databaseName;
        }
    }
}