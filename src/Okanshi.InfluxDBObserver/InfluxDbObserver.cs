using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.WriteOnly;

namespace Okanshi.Observers
{
    /// <summary>
    /// Observer for posting metrics to InfluxDB.
    /// </summary>
    public class InfluxDbObserver : IMetricObserver
    {
        private readonly IMetricPoller poller;
        private readonly IInfluxDbClient client;
        private readonly InfluxDbObserverOptions options;

        /// <summary>
        /// Creates a new instance of the observer.
        /// </summary>
        /// <param name="poller">The poller.</param>
        /// <param name="client">The InfluxDB client.</param>
        /// <param name="options">The observer options.</param>
        public InfluxDbObserver(IMetricPoller poller, IInfluxDbClient client, InfluxDbObserverOptions options)
        {
            if (poller == null)
            {
                throw new ArgumentNullException(nameof(poller));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.poller = poller;
            this.client = client;
            this.options = options;
            poller.MetricsPolled += OnMetricsPolled;
        }

        private void OnMetricsPolled(object sender, MetricEventArgs args)
        {
            Update(args.Metrics);
        }

        public void Dispose()
        {
            poller.MetricsPolled -= OnMetricsPolled;
        }

        /// <summary>
        /// Method used to write metrics to InfluxDB. This method are not meant to be used externally.
        /// </summary>
        /// <param name="metrics"></param>
        public void Update(Metric[] metrics)
        {
            var groupedMetrics = metrics.GroupBy(options.DatabaseSelector);
            foreach (var metricGroup in groupedMetrics)
            {
                var groupedByRetention = metricGroup.GroupBy(x => options.RetentionPolicySelector(x, metricGroup.Key));
                foreach (var retentionGroup in groupedByRetention)
                {
                    var points = ConvertToPoints(retentionGroup);
                    client.WriteAsync(retentionGroup.Key, metricGroup.Key, points);
                }
            }
        }

        private IEnumerable<Point> ConvertToPoints(IEnumerable<Metric> metrics)
        {
            var groupedByName = metrics.GroupBy(options.MeasurementNameSelector);
            foreach (var metricGroup in groupedByName)
            {
                if (metricGroup.SelectMany(x => x.Tags).Any(x => x.Key.Equals("statistic", StringComparison.OrdinalIgnoreCase)))
                {
                    var metricTags = metricGroup.First().Tags
                        .Where(x => !options.TagsToIgnore.Contains(x.Key) && !x.Key.Equals("dataSource", StringComparison.OrdinalIgnoreCase) &&
                                    !x.Key.Equals("statistic", StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                    var tags = metricTags.Where(x => !options.TagToFieldSelector(x)).Select(t => new InfluxDB.WriteOnly.Tag(t.Key, t.Value));
                    var statisticFields = metricGroup
                        .Select(metric => new { metric, statisticsTag = metric.Tags.SingleOrDefault(tag => tag.Key.Equals("statistic")) })
                        .Where(x => x.statisticsTag != null)
                        .Select(x => new Field(x.statisticsTag.Value, Convert.ToSingle(x.metric.Value)))
                        .ToList();
                    var fields = statisticFields.Any() ? statisticFields : new List<Field> { new Field("value", Convert.ToSingle(metricGroup.First().Value)) };
                    fields.AddRange(metricTags.Where(options.TagToFieldSelector).Select(ConvertTagToField));
                    yield return new Point
                    {
                        Measurement = metricGroup.Key,
                        Timestamp = metricGroup.First().Timestamp.DateTime,
                        Fields = fields,
                        Tags = tags
                    };
                } else
                {
                    foreach (var metric in metricGroup)
                    {
                        var metricTags = metric.Tags
                            .Where(x => !options.TagsToIgnore.Contains(x.Key) && !x.Key.Equals("dataSource", StringComparison.OrdinalIgnoreCase) &&
                                        !x.Key.Equals("statistic", StringComparison.OrdinalIgnoreCase))
                            .ToArray();
                        var tags = metricTags.Where(x => !options.TagToFieldSelector(x)).Select(t => new InfluxDB.WriteOnly.Tag(t.Key, t.Value));
                        var fields = new List<Field> { new Field("value", Convert.ToSingle(metric.Value)) };
                        fields.AddRange(metricTags.Where(options.TagToFieldSelector).Select(ConvertTagToField));
                        yield return new Point
                        {
                            Measurement = metricGroup.Key,
                            Timestamp = metric.Timestamp.DateTime,
                            Fields = fields,
                            Tags = tags
                        };
                    }
                }
            }
        }

        private static Field ConvertTagToField(Tag tag)
        {
            int i;
            if (int.TryParse(tag.Value, out i))
            {
                return new Field(tag.Key, i);
            }

            float f;
            if (float.TryParse(tag.Value, out f))
            {
                return new Field(tag.Key, f);
            }

            bool b;
            if (bool.TryParse(tag.Value, out b))
            {
                return new Field(tag.Key, b);
            }

            return new Field(tag.Key, tag.Value);
        }

        /// <summary>
        /// Get observations. This is not supported in this observer.
        /// </summary>
        public Metric[][] GetObservations()
        {
            throw new NotSupportedException("This observer doesn't support getting observations");
        }
    }
}