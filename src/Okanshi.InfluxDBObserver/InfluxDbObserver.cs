using System;
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
        public InfluxDbObserver(IMetricPoller poller, IInfluxDbClient client, InfluxDbObserverOptions options) {
            if (poller == null) {
                throw new ArgumentNullException(nameof(poller));
            }

            if (client == null) {
                throw new ArgumentNullException(nameof(client));
            }

            if (options == null) {
                throw new ArgumentNullException(nameof(options));
            }

            this.poller = poller;
            this.client = client;
            this.options = options;
            poller.MetricsPolled +=  OnMetricsPolled;
        }

        private void OnMetricsPolled(object sender, MetricEventArgs args) {
            Update(args.Metrics);
        }

        public void Dispose() {
            poller.MetricsPolled -= OnMetricsPolled;
            poller.Stop();
        }

        /// <summary>
        /// Method used to write metrics to InfluxDB. This method are not meant to be used externally.
        /// </summary>
        /// <param name="metrics"></param>
        public void Update(Metric[] metrics) {
            var groupedMetrics = metrics.GroupBy(options.DatabaseSelector);
            foreach (var metricGroup in groupedMetrics) {
                var groupedByRetention = metricGroup.GroupBy(x => options.RetentionPolicySelector(x, metricGroup.Key));
                foreach (var retentionGroup in groupedByRetention) {
                    client.WriteAsync(retentionGroup.Key, metricGroup.Key, metricGroup.Select(ConvertToPoint));
                }
            }
        }

        private Point ConvertToPoint(Metric metric) {
            var metricTags = metric.Tags.Where(x => !options.TagsToIgnore.Contains(x.Key) && !x.Key.Equals("dataSource", StringComparison.OrdinalIgnoreCase)).ToArray();
            var tags = metricTags.Where(x => !options.TagToFieldSelector(x)).Select(t => new InfluxDB.WriteOnly.Tag(t.Key, t.Value));
            var fields = metricTags.Where(options.TagToFieldSelector).Select(t => new Field(t.Key, Convert.ToSingle(t.Value))).ToList();
            fields.Add(new Field("value", Convert.ToSingle(metric.Value)));
            return new Point {
                Measurement = options.MeasurementNameSelector(metric),
                Timestamp = DateTime.UtcNow,
                Fields = fields,
                Tags = tags,
            };
        }

        /// <summary>
        /// Get observations. This is not supported in this observer.
        /// </summary>
        public Metric[][] GetObservations() {
            throw new NotSupportedException("This observer doesn't support getting observations");
        }
    }
}
