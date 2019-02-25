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
		/// How the request path is extracted
		/// </summary>
		public RequestPathExtraction PathExtraction { get; set; } = RequestPathExtraction.Path;

		/// <summary>
		/// A factory method which is invoked whenever a timer is needed
		/// </summary>
		public Func<string, Tag[], ITimer> TimerFactory = (name, tags) => OkanshiMonitor.Timer(name, tags);
	}

	/// <summary>
    /// How the request path is extracted
    /// </summary>
	public enum RequestPathExtraction
	{
        /// <summary>
        /// Uses "request.RequestUri.LocalPath" which holds the path including any parameters within the path, e.g. customerId
        /// </summary>
        Path,

        /// <summary>
        /// Uses the canonical representation of an endpoint.
        /// 
        /// Routes must be explicitly annotated using the [Route] attribute for them to show up correctly. If not properly annotated
        /// this code will use an abstract path such as "api/{controller}/{id}" which will be the general configured fall-back path.
        /// </summary>
        CanonicalPath,
	}
}