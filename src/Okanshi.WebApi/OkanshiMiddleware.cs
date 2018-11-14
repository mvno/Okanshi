using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Okanshi.WebApi
{
	/// <summary>
    /// Reference for the implementation https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/http-message-handlers
    /// </summary>
    public class OkanshiMiddleware : DelegatingHandler
    {
        private readonly OkanshiWebApiOptions options;

        public OkanshiMiddleware(OkanshiWebApiOptions options)
        {
            this.options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken);
            timer.Stop();

            var tags = new List<Tag>();
            if (options.AddStatusCodeTag)
            {
                tags.Add(new Tag("responseCode", ((int)response.StatusCode).ToString()));
            }
            tags.Add(new Tag("path", request.RequestUri.LocalPath));
            tags.Add(new Tag("method", request.Method.Method));

            var okanshiTimer = options.TimerFactory(options.MetricName, tags.ToArray());
            okanshiTimer.RegisterElapsed(timer);

            return response;
        }
    }
}