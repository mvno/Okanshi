using System;

namespace Okanshi.WebApi
{
	/// <summary>
	/// Okanshi options
	/// </summary>
	public class OkanshiWebApiOptions
	{
		/// <summary>
		/// Should status codes be added as tags. Default value is true.
		/// </summary>
		public bool AddStatusCodeTag { get; set; } = true;

		/// <summary>
		/// The metric name to use. Default values is Request.
		/// </summary>
		public string MetricName { get; set; } = "Request";

		/// <summary>
		/// A factory method which is invoked whenever a timer is needed
		/// </summary>
		public Func<string, Tag[], ITimer> TimerFactory = (name, tags) => OkanshiMonitor.Timer(name, tags);
	}
}