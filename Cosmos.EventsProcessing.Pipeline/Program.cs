namespace Cosmos.EventsProcessing.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Cosmos.EventsProcessing.Core;
    using Cosmos.EventsProcessing.KafaWriter;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    class Program
    {
        private IConfigurationRoot cosmosDBProducerConfiguration = null;
        private IConfigurationRoot cosmosDBConsumerConfiguration = null;
        private IConfigurationRoot consumerRouterConfiguration = null;
        private IConfigurationRoot kafkaConsumerConfiguration = null;


        private CosmosDBSettings cosmosDBProducerSettings = null;
        private CosmosDBSettings cosmosDBConsumerSettings = null;
        private ConsumerRouterSettings cosmosConsumerRouterSettings = null;
        private EventHubKafkaAccount eventHubKafkaAccountSettings = null;

        private CosmosDBEventsReader eventsReader = null;
        private CosmosDBEventsWriter eventsWriter = null;
        private KafkaProducer kafkaProducer = null;
        static async Task Main(string[] args)
        {
            await new Program().RunAsync();

            while (true)
            {
            }
        }

        async Task RunAsync()
        {
            this.cosmosDBProducerConfiguration = new ConfigurationBuilder()
                    .AddJsonFile("cosmosProducerSettings.json")
                    .Build();
            this.consumerRouterConfiguration = new ConfigurationBuilder()
                .AddJsonFile("consumerRouterSettings.json")
                .Build();

            this.cosmosDBProducerSettings = ConfigHelper.LoadCosmosProducerSettings(this.cosmosDBProducerConfiguration);
            this.cosmosConsumerRouterSettings = ConfigHelper.LoadCosmosConsumerRouterSettings(this.consumerRouterConfiguration);

            if (this.cosmosConsumerRouterSettings.Host.ToLower() == "kafka")
            {
                this.InitKafkaConsumer();
            }
            else if (this.cosmosConsumerRouterSettings.Host.ToLower() == "cosmos")
            {
                await InitCosmosDBConsumerAsync();
            }
            else
            {
                throw new Exception($"Unsupported host: {this.cosmosConsumerRouterSettings.Host}");
            }

            this.eventsReader = new CosmosDBEventsReader(cosmosDBProducerSettings);
            await eventsReader.StartAsync(HandleChangesAsync);

        }
        async Task HandleChangesAsync(IReadOnlyCollection<EventDataModel> changes, CancellationToken cancellationToken)
        {
            if (this.cosmosConsumerRouterSettings.Host.ToLower() == "kafka")
            {
                await this.kafkaProducer.WriteEventsAsync(changes);
            }
            else if (this.cosmosConsumerRouterSettings.Host.ToLower() == "cosmos")
            {
                await this.eventsWriter.WriteEvents(changes, this.cosmosConsumerRouterSettings);
            }
            else
            {
                throw new Exception($"Unsupported host: {this.cosmosConsumerRouterSettings.Host}");
            }
        }

        private void InitKafkaConsumer()
        {
            this.kafkaConsumerConfiguration= new ConfigurationBuilder()
           .AddJsonFile("kafkaConsumerSettings.json")
           .Build();

            this.eventHubKafkaAccountSettings = ConfigHelper.GetEventHubKafkaAccount(this.kafkaConsumerConfiguration);
            this.kafkaProducer = new KafkaProducer(this.eventHubKafkaAccountSettings, this.cosmosConsumerRouterSettings);
        }

        private async Task InitCosmosDBConsumerAsync()
        {
            this.cosmosDBConsumerConfiguration = new ConfigurationBuilder()
          .AddJsonFile("cosmosConsumerSettings.json")
          .Build();
            this.cosmosDBConsumerSettings = ConfigHelper.LoadCosmosConsumerSettings(this.cosmosDBConsumerConfiguration);
            this.eventsWriter = new CosmosDBEventsWriter(cosmosDBConsumerSettings);
            await this.eventsWriter.InitClientsAsync();
        }


    }
}
