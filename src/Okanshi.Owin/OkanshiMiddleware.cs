using System.Collections.Generic;
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
        private static readonly List<Tag> EmptyTagList = new List<Tag>();

        /// <summary>
        /// Create a new instance.
        /// </summary>
        public OkanshiMiddleware(AppFunc next, OkanshiOwinOptions options)
        {
            this.next = next;
            this.options = options;
        }

        /// <summary>
        /// Invoke the middleware.
        /// </summary>
        public async Task Invoke(IDictionary<string, object> environment)
        {
            long elapsed = 0;
            var timer = OkanshiTimer.StartNew(x => elapsed = x);
            await next.Invoke(environment);
            timer.Stop();
            var tags = EmptyTagList;
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
            var basicTimer = OkanshiMonitor.BasicTimer(options.MetricName, tags.ToArray());
            basicTimer.Register(elapsed);
        }
    }
}