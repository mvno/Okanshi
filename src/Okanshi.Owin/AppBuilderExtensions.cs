using Owin;

namespace Okanshi.Owin {
    public static class AppBuilderExtensions {
        public static void UseOkanshi(this IAppBuilder appBuilder, OkanshiOwinOptions options = null)
        {
            appBuilder.Use(typeof(OkanshiMiddleware), options ?? new OkanshiOwinOptions());
        }
    }
}
