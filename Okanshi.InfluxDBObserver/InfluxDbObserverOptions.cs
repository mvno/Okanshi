using System;
using System.Collections;
using System.Collections.Generic;

namespace Okanshi.Observers {
    public class InfluxDbObserverOptions {
        public string DatabaseName { get; }

        public string RetentionPolicy { get; set; }

        public Func<Tag, bool> TagToFieldSelector { get; set; }

        public IEnumerable<string> TagsToIgnore { get; set; }

        public InfluxDbObserverOptions(string databaseName) {
            DatabaseName = databaseName;
        }
    }
}