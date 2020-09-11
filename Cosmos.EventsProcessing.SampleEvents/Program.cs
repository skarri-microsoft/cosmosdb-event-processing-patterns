namespace Cosmos.EventsProcessing.SampleEvents
{
    using Cosmos.EventsProcessing.Core;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                   .AddJsonFile("cosmosProducerSettings.json")
                   .Build();
            CosmosDBAccount cosmosDBacc = ConfigHelper.GetCosmosDBAccount("producer", configuration);
            CosmosClient cosmosClient = new CosmosClient(cosmosDBacc.Endpoint, cosmosDBacc.AuthKey);
            await CosmosExtension.SetupCosmosDBEntitiesAsync(cosmosDBacc);
            Container container= cosmosClient.GetContainer(cosmosDBacc.Database, cosmosDBacc.Container);
            while(true)
            {
                List<EventDataModel> events = SampleEvents(1);
                foreach(EventDataModel eventDataModel in events)
                {
                   await container.CreateItemAsync<EventDataModel>(eventDataModel, new PartitionKey(eventDataModel.id));
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        static List<EventDataModel> SampleEvents(int batchSize)
        {
            List<EventDataModel> events = new List<EventDataModel>();
            for(int i=0;i<batchSize;i++)
            {
                EventDataModel item = new EventDataModel()
                {

                    id = System.Guid.NewGuid().ToString(),
                    topic = $"topic-{new System.Random().Next(4, 6)}",
                    message = $"message - {System.Guid.NewGuid().ToString()}",
                    createdTime = DateTime.UtcNow
                };
                events.Add(item);

                Console.WriteLine($"\t Generated event - id: {item.id}, created time: {item.createdTime} , topic: {item.topic}, message: {item.message}");
            }
            return events;
        }
    }
}
