namespace Cosmos.EventsProcessing.Core
{
    using Microsoft.Extensions.Configuration;
    using System.Collections.Generic;
    using System.Linq;

    public class ConfigHelper
    {
        public static CosmosDBSettings LoadCosmosProducerSettings(IConfigurationRoot configuration)
        {
            CosmosDBSettings cosmosDBSettings = new CosmosDBSettings();
            cosmosDBSettings.Producer = GetCosmosDBAccount("producer", configuration);
            cosmosDBSettings.Lease = GetCosmosDBAccount("lease", configuration);
            return cosmosDBSettings;
        }

        public static CosmosDBSettings LoadCosmosConsumerSettings(IConfigurationRoot configuration)
        {
            CosmosDBSettings cosmosDBSettings = new CosmosDBSettings();
            cosmosDBSettings.Consumer = GetCosmosDBAccount("consumer", configuration);
            return cosmosDBSettings;
        }

        public static ConsumerRouterSettings LoadCosmosConsumerRouterSettings(IConfigurationRoot configuration)
        {
            ConsumerRouterSettings cosmosConsumerRouterSettings = new ConsumerRouterSettings();
            cosmosConsumerRouterSettings.IsIndividualTopicsRoutingEnabled = bool.Parse(configuration["EnableIndividualTopicsRouting"]);
            cosmosConsumerRouterSettings.WhiteListedTopies = configuration["WhiteListedTopics"].Split(new char[] { '|' }).ToList();
            cosmosConsumerRouterSettings.Host = configuration["Host"];
            return cosmosConsumerRouterSettings;
        }

        public static CosmosDBAccount GetCosmosDBAccount(string prefix, IConfigurationRoot configuration)
        {
            CosmosDBAccount cosmosDBAccount = new CosmosDBAccount();
            cosmosDBAccount.Endpoint = configuration[$"{prefix}-Endpoint"];
            cosmosDBAccount.AuthKey = configuration[$"{prefix}-AuthKey"];
            cosmosDBAccount.Database = configuration[$"{prefix}-Database"];
            cosmosDBAccount.Container = configuration[$"{prefix}-Container"];
            cosmosDBAccount.PartitionKeyPath = configuration[$"{prefix}-PartitionKeyPath"];
            cosmosDBAccount.CreateIfDatabaseNotExists = bool.Parse(configuration[$"{prefix}-CreateIfDatabaseNotExists"]);
            cosmosDBAccount.CreateIfContainerNotExists = bool.Parse(configuration[$"{prefix}-CreateIfContainerNotExists"]);
            cosmosDBAccount.DeleteIfDatabaseExists = bool.Parse(configuration[$"{prefix}-DeleteIfDatabaseExists"]);
            cosmosDBAccount.DeleteIfContainerExists = bool.Parse(configuration[$"{prefix}-DeleteIfContainerExists"]);
            return cosmosDBAccount;
        }

        public static EventHubKafkaAccount GetEventHubKafkaAccount(IConfigurationRoot configuration)
        {
            EventHubKafkaAccount eventHubKafkaAccount = new EventHubKafkaAccount();
            eventHubKafkaAccount.BrokerList = configuration["consumer-brokerList-EH_FQDN"];
            eventHubKafkaAccount.SaslPassword = configuration["consumer-SaslPassword-EH_CONNECTION_STRING"];
            eventHubKafkaAccount.Topic = configuration["consumer-Default-Topic-EH_NAME"];
            eventHubKafkaAccount.ConsumberGroup = configuration["consumer-Group"];
            eventHubKafkaAccount.CaCertLocation = configuration["consumer-CA_CERT_LOCATION"];
            return eventHubKafkaAccount;
        }
    }
}
