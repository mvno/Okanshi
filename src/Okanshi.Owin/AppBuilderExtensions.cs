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
        /// Use Okanshi middleware for timining all the requests.
        /// </summary>
        /// <param name="appBuilder">The AppBuilder to extend</param>
        /// <param name="options">The options</param>
        /// <param name="timerFactory">An optional factory method for creating timers. If not set OkanshiMonitor is used.</param>
        public static void UseOkanshi(this IAppBuilder appBuilder, OkanshiOwinOptions options = null, Func<string, Tag[], ITimer> timerFactory = null)
        {
            appBuilder.Use(
                typeof(OkanshiMiddleware), 
                options ?? new OkanshiOwinOptions(),
                timerFactory ?? ((name, tags) => OkanshiMonitor.Timer(name, tags))
                );
        }
    }
}