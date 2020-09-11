namespace Cosmos.EventsProcessing.Core
{
    using Microsoft.Azure.Cosmos;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using static Microsoft.Azure.Cosmos.Container;

    public class CosmosDBEventsWriter
    {
        private CosmosDBSettings cosmosDBSettings;

        public CosmosDBEventsWriter(CosmosDBSettings settings)
        {
            this.cosmosDBSettings = settings;
        }

        public async Task InitClientsAsync()
        {
            CosmosClientPool.CreateClient(this.cosmosDBSettings.Consumer);
        
            await CosmosExtension.SetupCosmosDBEntitiesAsync(this.cosmosDBSettings.Consumer);
        }

        public async Task WriteEvents(IReadOnlyCollection<EventDataModel> changes, ConsumerRouterSettings consumerRouterSettings)
        {
            foreach (EventDataModel item in changes)
            {
                Console.WriteLine($"\tReceived id: {item.id}, created time: {item.createdTime} , topic: {item.topic}, message: {item.message}");
                PartitionKey destinationPartitionkey = new PartitionKey(item.id);
                Container destination = await GetDestinationContainer(consumerRouterSettings, item.topic);
                Console.WriteLine($"\t sending it to account: {this.cosmosDBSettings.Consumer.Endpoint}, database: {this.cosmosDBSettings.Consumer.Database} , collection: {destination.Id}, topic: {item.topic}");
                await destination.CreateItemAsync<EventDataModel>(item, destinationPartitionkey);
            }
        }

        private async Task<Container> GetDestinationContainer(ConsumerRouterSettings consumerRouterSettings,string topic)
        {
            if(consumerRouterSettings.IsIndividualTopicsRoutingEnabled)
            {
                if(consumerRouterSettings.WhiteListedTopies.Contains(topic))
                {
                    // Route these to specific container
                    // Make sure topic container exists
                   if(!CosmosClientPool.IsTopicExists(topic))
                    {
                        Database database = CosmosClientPool.GetCosmosClient(this.cosmosDBSettings.Consumer).GetDatabase(this.cosmosDBSettings.Consumer.Database);
                        // Create a new container in the same database.
                        Container container = await CosmosExtension.SetupCollectionAsync(database, topic, this.cosmosDBSettings.Consumer.PartitionKeyPath, this.cosmosDBSettings.Consumer.CreateIfContainerNotExists, this.cosmosDBSettings.Consumer.DeleteIfContainerExists);
                        CosmosClientPool.AddTopic(topic);
                        return container;
                    }

                    return CosmosClientPool.GetCosmosClient(this.cosmosDBSettings.Consumer).GetContainer(this.cosmosDBSettings.Consumer.Database, topic);

                }
            }

            // Default container
           return CosmosClientPool.GetCosmosClient(this.cosmosDBSettings.Consumer).GetContainer(this.cosmosDBSettings.Consumer.Database, this.cosmosDBSettings.Consumer.Container);
        }
    }
}
