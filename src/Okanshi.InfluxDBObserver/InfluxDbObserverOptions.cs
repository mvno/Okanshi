using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.WriteOnly;

namespace Okanshi.InfluxDbObserver
{
    /// <summary>
    /// Options for the InfluxDB observer
    /// </summary>
    public class InfluxDbObserverOptions
    {
        /// <summary>
        /// Database selector. Selects the database based on the metric.
        /// Default is the database name passed into constructor.
        /// </summary>
        public Func<Metric, string> DatabaseSelector { get; set; }

        /// <summary>
        /// Retention policy selector. Selects the retention policy based on the metric and database name.
        /// Default is "autogen", as this is the default retention policy in newer versions of InfluxDB.
        /// </summary>
        public Func<Metric, string, string> RetentionPolicySelector { get; set; } = (x, y) => "autogen";

        /// <summary>
        /// Predicate for converting tags to fields.
        /// </summary>
        public Func<Tag, bool> TagToFieldSelector { get; set; } = _ => false;

        /// <summary>
        /// Function used to select to measurement name. Default is the metric name.
        /// </summary>
        public Func<Metric, string> MeasurementNameSelector { get; set; } = metric => metric.Name;

        /// <summary>
        /// List of tag keys to ignore.
        /// </summary>
        public IEnumerable<string> TagsToIgnore { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Create a new instance of the options.
        /// </summary>
        /// <param name="databaseName">The default database name used</param>
        public InfluxDbObserverOptions(string databaseName)
        {
            DatabaseSelector = _ => databaseName;
        }

        /// <summary>
        /// Function used to convert field types. Default is no conversion.
        /// </summary>
        public Func<object, object> ConvertFieldType { get; set; } = x => x;
    }
}