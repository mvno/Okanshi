using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Okanshi.Observers
{
    /// <summary> A MetricsObserver sending metrics to splunk </summary>
    public class SplunkObserver : IMetricObserver
    {
        private readonly IHttpPoster _poster;
        private readonly Func<object, string> _jsonSerializer;
        private readonly Func<Metric, Dictionary<string, object>> _metricConverter;

        /// <summary> A MetricsObserver sending metrics to splunk </summary>
        public SplunkObserver(IHttpPoster poster, Func<object, string> jsonSerializer) : this(poster, MeasurementToDictionary, jsonSerializer)
        {}

        /// <summary> A MetricsObserver sending metrics to splunk </summary>
        public SplunkObserver(IHttpPoster poster, Func<Metric, Dictionary<string, object>> metricConverter, Func<object, string> jsonSerializer)
        {
            _poster = poster ?? throw new ArgumentNullException(nameof(poster));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _metricConverter = metricConverter ?? throw new ArgumentNullException(nameof(metricConverter)); ;
        }

        /// <summary> process </summary>
        public async Task Update(IEnumerable<Metric> metrics)
        {
            try
            {
                var events = ConvertMeasurementsToJson(metrics);
                var noEventsPresent = string.IsNullOrWhiteSpace(events);
                if (noEventsPresent)
                    return;

                _poster.SendToSplunk(events);
            }
            catch (Exception e)
            {
                Logger.Error("Exception while sending metrics to Splunk", e);
            }
        }

        /// <summary>
        /// Override this in subclasses if you want to control how measurements are sent to Splunk
        /// </summary>
        string ConvertMeasurementsToJson(IEnumerable<Metric> metrics)
        {
            var splunkEvents = metrics
                .Select(x => _metricConverter(x))
                .Select(x => _jsonSerializer(new SplunkEventWrapper(x)))
                .ToArray();
            var events = string.Join(" ", splunkEvents);

            return events;
        }

        /// <summary>
        /// A general helper to turn a metric into a structure that become json
        /// </summary>
        static Dictionary<string, object> MeasurementToDictionary(Metric metric)
        {
            var result = new Dictionary<string, object>
            {
                {"name", metric.Name},
                {"timeStamp", metric.Timestamp.LocalDateTime},
            };

            const string valuesKey = "values";
            const string tagsKey = "tags";
            var values = new Dictionary<string, object>();
            var tags = new Dictionary<string, object>();

            AddTags();
            AddValues();

            void AddTags()
            {
                foreach (var tag in metric.Tags)
                {
                    var name = tag.Key;
                    var destination = result.ContainsKey(name) || name == tagsKey || name == valuesKey ? tags : result;
                    destination.Add(name, tag.Value);
                }

                if (tags.Any())
                    result.Add(tagsKey, tags);
            }

            void AddValues()
            {
                foreach (var measurement in metric.Values)
                {
                    var name = measurement.Name;
                    var destination = result.ContainsKey(name) || name == tagsKey || name == valuesKey ? values : result;
                    destination.Add(name, measurement.Value);
                }

                if (values.Any())
                    result.Add(valuesKey, values);
            }

            return result;
        }

        public void Dispose()
        { }
    }
}