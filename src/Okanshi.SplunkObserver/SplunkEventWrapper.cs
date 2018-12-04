using System.Collections.Generic;

namespace Okanshi.Observers
{
    /// <summary>
    /// Events sendt to the splunk http endpoint needs to be wrapped in an "event"
    /// </summary>
    public class SplunkEventWrapper
    {
        /// <summary> wrapper </summary>
        public Dictionary<string, object> @event { get; }

        /// <summary>
        /// Events sendt to the splunk http endpoint needs to be wrapped
        /// </summary>
        public SplunkEventWrapper(Dictionary<string, object> @event)
        {
            this.@event = @event;
        }
    }
}