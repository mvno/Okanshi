using System;

namespace Okanshi.Owin
{
    /// <summary>
    /// Okanshi options
    /// </summary>
    public class OkanshiOwinOptions
    {
        /// <summary>
        /// The step size to use when creating timers. Default value is 1 minute.
        /// </summary>
        public TimeSpan StepSize { get; set; } = OkanshiMonitor.DefaultStep;

        /// <summary>
        /// Should status codes be added as tags. Default value is true.
        /// </summary>
        public bool AddStatusCodeTag { get; set; } = true;

        /// <summary>
        /// The metric name to use. Default values is Request.
        /// </summary>
        public string MetricName { get; set; } = "Request";
    }
}