using System;
using System.Linq;
using InfluxDB.WriteOnly;

namespace Okanshi.Observers
{
    public class InfluxDbObserver : IMetricObserver
    {
        private readonly IMetricPoller poller;
        private readonly IInfluxDbClient client;
        private readonly InfluxDbObserverOptions options;

        public InfluxDbObserver(IMetricPoller poller, IInfluxDbClient client, InfluxDbObserverOptions options) {
            if (poller == null) {
                throw new ArgumentNullException(nameof(poller));
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

        public void Update(Metric[] metrics) {
            var points = metrics.Select(m => {
                var tags = m.Tags.Where(x => !options.TagToFieldSelector(x)).Select(t => new InfluxDB.WriteOnly.Tag(t.Key, t.Value));
                var fields = m.Tags.Where(options.TagToFieldSelector).Select(t => new Field(t.Key, Convert.ToSingle(t.Value))).ToList();
                fields.Add(new Field("value", Convert.ToSingle(m.Value)));
                return new Point {
                    Measurement = m.Name,
                    Timestamp = DateTime.UtcNow,
                    Fields = fields,
                    Tags = tags,
                };
            });
            client.WriteAsync(options.RetentionPolicy, options.DatabaseName, points);
        }

        public Metric[][] GetObservations() {
            throw new NotSupportedException("This observer doesn't support getting observations");
        }
    }
}
