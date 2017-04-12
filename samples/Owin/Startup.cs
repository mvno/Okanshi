using System.Web.Http;
using Okanshi.Endpoint;
using Okanshi.Owin;

namespace Owin {
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var monitorEndpoint = new MonitorEndpoint();
            monitorEndpoint.Start();

            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name : "Default API",
                routeTemplate : "api/{controller}/{id}",
                defaults : new { id = RouteParameter.Optional }
            );

            appBuilder.UseOkanshi(new OkanshiOwinOptions { AddStatusCodeTag = false });
            appBuilder.UseWebApi(config);
        }
    }
}