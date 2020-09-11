namespace Cosmos.EventsProcessing.Core
{
    using System.Collections.Generic;
    public class ConsumerRouterSettings
    {
        public bool IsIndividualTopicsRoutingEnabled { get; set; }
        public List<string> WhiteListedTopies { get; set; }

        public string Host { get; set; }
    }
}
