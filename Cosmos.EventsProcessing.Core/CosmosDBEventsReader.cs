namespace Cosmos.EventsProcessing.Core
{
    using Microsoft.Azure.Cosmos;
    using System;
    using System.Threading.Tasks;
    using static Microsoft.Azure.Cosmos.Container;

    public class CosmosDBEventsReader
    {
        private CosmosDBSettings cosmosDBSettings;
        private ChangeFeedProcessor changeFeedProcessor;
        ChangesHandler<EventDataModel> changesHandler;
        public CosmosDBEventsReader(CosmosDBSettings settings)
        {
            this.cosmosDBSettings = settings;
        }

        public async Task StartAsync(ChangesHandler<EventDataModel> changesHandler)
        {
            this.changesHandler = changesHandler;
            await this.InitClientsAsync();
        }

        public async Task StopAsync()
        {
            if (this.changeFeedProcessor != null)
            {
                await changeFeedProcessor.StopAsync();
            }
        }

        private async Task InitClientsAsync()
        {
            CosmosClientPool.CreateClient(this.cosmosDBSettings.Producer);
            CosmosClientPool.CreateClient(this.cosmosDBSettings.Lease);

            //await CosmosExtension.SetupCosmosDBEntitiesAsync(this.cosmosDBSettings.Producer);

            await CosmosExtension.SetupCosmosDBEntitiesAsync(this.cosmosDBSettings.Lease);

            Microsoft.Azure.Cosmos.Container leaseContainer = CosmosClientPool.GetCosmosClient(this.cosmosDBSettings.Lease).GetContainer(this.cosmosDBSettings.Lease.Database, this.cosmosDBSettings.Lease.Container);
            Microsoft.Azure.Cosmos.Container sourceContainer = CosmosClientPool.GetCosmosClient(this.cosmosDBSettings.Producer).GetContainer(this.cosmosDBSettings.Producer.Database, this.cosmosDBSettings.Producer.Container);
            this.changeFeedProcessor = sourceContainer
                .GetChangeFeedProcessorBuilder<EventDataModel>("changeFeedBeginning", this.changesHandler)
                    .WithInstanceName("EventprocessingConsole")
                    .WithLeaseContainer(leaseContainer)
                    .WithStartTime(DateTime.MinValue.ToUniversalTime())
                    .Build();
            await this.changeFeedProcessor.StartAsync();
        }

    
    }
}
