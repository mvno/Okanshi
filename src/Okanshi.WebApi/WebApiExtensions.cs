using System.Web.Http;

namespace Okanshi.WebApi
{
	public static class WebApiExtensions
	{
		public static void UseOkanshi(this HttpConfiguration configuration, OkanshiWebApiOptions options = null)
		{
			var apiOptions = options ?? new OkanshiWebApiOptions();
			var middleware = new OkanshiMiddleware(apiOptions);

			configuration.MessageHandlers.Add(middleware);
		}
	}
}