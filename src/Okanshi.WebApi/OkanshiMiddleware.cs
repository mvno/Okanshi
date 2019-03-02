using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Routing;

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
                tags.Add(new Tag("responseCode", ((int)response.StatusCode).ToString()));

            tags.Add(new Tag("path", GetPath(request)));

            tags.Add(new Tag("method", request.Method.Method));

            var okanshiTimer = options.TimerFactory(options.MetricName, tags.ToArray());
            okanshiTimer.RegisterElapsed(timer);

            return response;
        }

        private string GetPath(HttpRequestMessage request)
        {
            string path;
            switch (options.PathExtraction)
            {
                case RequestPathExtraction.Path:
                    path = request.RequestUri.LocalPath;
                    break;
                case RequestPathExtraction.CanonicalPath:
                    path = GetCanonicalPath(request);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(options.PathExtraction.ToString());
            }

            return path;
        }

        /// <summary>
        /// Routes must be explicitly annotated using the [Route] attribute for them to show up correctly. If not properly annotated
        /// this code will use an abstract path such as "api/{controller}/{id}" which will be the general configured fall-back path.
        /// </summary>
        private static string GetCanonicalPath(HttpRequestMessage request)
        {
            var httpRouteData = request.GetConfiguration().Routes.GetRouteData(request);
            var routeInfo = httpRouteData.Values.TryGetValue("MS_SubRoutes", out var x)
                ? ((IHttpRouteData[]) x).FirstOrDefault()
                : httpRouteData;

            string path = routeInfo?.Route.RouteTemplate;
            return path;
        }
    }
}