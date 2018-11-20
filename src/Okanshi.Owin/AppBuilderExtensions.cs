using System;
using Owin;

namespace Okanshi.Owin
{
    /// <summary>
    /// IAppBuilder extensions
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Use Okanshi middleware for timing all the requests.
        /// </summary>
        /// <param name="appBuilder">The AppBuilder to extend</param>
        /// <param name="options">The options</param>
        public static void UseOkanshi(this IAppBuilder appBuilder, OkanshiOwinOptions options = null)
        {
            appBuilder.Use(
                typeof(OkanshiMiddleware), 
                options ?? new OkanshiOwinOptions()
                );
        }
    }
}