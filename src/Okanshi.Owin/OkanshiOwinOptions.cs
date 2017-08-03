using System;

namespace Okanshi.Owin
{
    /// <summary>
    /// Okanshi options
    /// </summary>
    public class OkanshiOwinOptions
    {
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