using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Okanshi.Owin
{
    /// <summary>
    /// The Okanshi middleware
    /// </summary>
    public class OkanshiMiddleware
    {
        private readonly AppFunc next;
        private readonly OkanshiOwinOptions options;

        public OkanshiMiddleware(AppFunc next, OkanshiOwinOptions options)
        {
            this.next = next;
            this.options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var timer = Stopwatch.StartNew();

            await next.Invoke(environment);

            timer.Stop();

            var tags = new List<Tag>();
            if (options.AddStatusCodeTag)
            {
                object responseCode;
                var found = environment.TryGetValue("owin.ResponseStatusCode", out responseCode);
                if (found)
                {
                    tags.Add(new Tag("responseCode", responseCode.ToString()));
                }
            }

            tags.Add(new Tag("path", environment["owin.RequestPath"].ToString()));
            tags.Add(new Tag("method", environment["owin.RequestMethod"].ToString()));
            var okanshiTimer = OkanshiMonitor.Timer(options.MetricName, tags.ToArray());
            okanshiTimer.Register(timer.ElapsedMilliseconds);
        }
    }
}