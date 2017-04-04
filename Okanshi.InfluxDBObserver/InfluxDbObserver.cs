using System;
using System.Linq;
using InfluxDB.WriteOnly;

namespace Okanshi.Observers
{
    public class InfluxDbObserver : IMetricObserver
    {
        private readonly IMetricPoller poller;
        private readonly IInfluxDbClient client;
        private readonly string databaseName;

        public InfluxDbObserver(IMetricPoller poller, IInfluxDbClient client, string databaseName) {
            if (poller == null) {
                throw new ArgumentNullException(nameof(poller));
            }

            this.poller = poller;
            this.client = client;
            this.databaseName = databaseName;
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
            var points = metrics.Select(m => new Point {
                Measurement = m.Name,
                Timestamp = DateTime.UtcNow,
                Fields = new[] {
                    new Field("value", Convert.ToSingle(m.Value)),
                },
                Tags = m.Tags.Select(t => new InfluxDB.WriteOnly.Tag(t.Key, t.Value)),
            });
            client.WriteAsync(databaseName, points);
        }

        public Metric[][] GetObservations() {
            throw new NotSupportedException("This observer doesn't support getting observations");
        }
    }
}
