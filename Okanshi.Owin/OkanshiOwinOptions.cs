using System;

namespace Okanshi.Owin
{
    public class OkanshiOwinOptions
    {
        public TimeSpan? StepSize { get; set; }
        public bool AddStatusCodeTag { get; set; } = true;
        public string MetricName { get; set; } = "Request";
    }
}