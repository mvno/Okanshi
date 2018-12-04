using System.Collections.Generic;

namespace Okanshi.Observers
{
    /// <summary>
    /// Events sendt to the splunk http endpoint needs to be wrapped
    /// </summary>
    public class SplunkEventWrapper
    {
        public Dictionary<string, object> Event { get; }

        /// <summary>
        /// Events sendt to the splunk http endpoint needs to be wrapped
        /// </summary>
        public SplunkEventWrapper(Dictionary<string, object> @event)
        {
            Event = @event;
        }
    }
}