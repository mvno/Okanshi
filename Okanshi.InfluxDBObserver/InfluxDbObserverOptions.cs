using System;
using System.Collections.Generic;
using System.Linq;

namespace Okanshi.Observers {
    public class InfluxDbObserverOptions {
        public Func<Metric, string> DatabaseSelector { get; set; }

        public string RetentionPolicy { get; set; }

        public Func<Tag, bool> TagToFieldSelector { get; set; } = x => false;

        public IEnumerable<string> TagsToIgnore { get; set; } = Enumerable.Empty<string>();

        public InfluxDbObserverOptions(string databaseName) {
            DatabaseSelector = _ => databaseName;
        }
    }
}