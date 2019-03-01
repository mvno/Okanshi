using System;

namespace Okanshi.Autofac
{
    /// <summary>
    /// Okanshi options
    /// </summary>
    public class OkanshiAutofacOptions
    {
        public MeasurementStyleKind MeasurementStyle { get; set; } = MeasurementStyleKind.None;

        /// <summary>
        /// The metric name to use. Default values is Request.
        /// </summary>
        public string MetricName { get; set; } = "Autofac instantiation";

        /// <summary>
        /// A factory method which is invoked whenever a timer is needed for <see cref="MeasurementStyleKind.CountAndTimeInstantiations"/>
        /// </summary>
        public Func<string, Tag[], ITimer> TimerFactory = (name, tags) => OkanshiMonitor.Timer(name, tags);

        /// <summary>
        /// A factory method which is invoked whenever a counter is needed for <see cref="MeasurementStyleKind.CountInstantiations"/>
        /// </summary>
        public Func<string, Tag[], ICounter<long>> CountFactory = (name, tags) => OkanshiMonitor.Counter(name, tags);
    }
}