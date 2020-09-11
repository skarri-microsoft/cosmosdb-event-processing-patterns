namespace Cosmos.EventsProcessing.Core
{
    public class EventHubKafkaAccount
    {
        // Event hub namespace from Azure portal, Ex: skarrievents.servicebus.windows.net:9093
        public string BrokerList { get; set; }

        // Event hub connection string from Azure portal
        public string SaslPassword { get; set; }

        // Event hub name
        public string Topic { get; set; }

        public string ConsumberGroup { get; set; }

        public string CaCertLocation { get; set; }
    }
}
