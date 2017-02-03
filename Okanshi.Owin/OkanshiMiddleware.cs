using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Okanshi.Owin
{
    public class OkanshiMiddleware
    {
        private readonly AppFunc next;
        private readonly OkanshiOwinOptions options;
        private static readonly Tag[] EmptyTagArray = new Tag[0];

        public OkanshiMiddleware(AppFunc next, OkanshiOwinOptions options)
        {
            this.next = next;
            this.options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            long elapsed = 0;
            var timer = OkanshiTimer.StartNew(x => elapsed = x);
            await next.Invoke(environment);
            timer.Stop();
            var tags = EmptyTagArray;
            if (options.AddStatusCodeTag)
            {
                object responseCode;
                var found = environment.TryGetValue("owin.ResponseStatusCode", out responseCode);
                if (found)
                {
                    var tagList = tags.ToList();
                    tagList.Add(new Tag("responseCode", responseCode.ToString()));
                    tags = tagList.ToArray();
                }
            }
            var basicTimer = OkanshiMonitor.BasicTimer("Request", options.StepSize ?? OkanshiMonitor.DefaultStep, tags);
            basicTimer.Register(elapsed);
        }
    }
}