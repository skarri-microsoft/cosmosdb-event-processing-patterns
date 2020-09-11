namespace Cosmos.EventsProcessing.KafaWriter
{
    using Confluent.Kafka;
    using Cosmos.EventsProcessing.Core;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class KafkaProducer
    {
        private EventHubKafkaAccount eventHubKafkaAccount;
        private ProducerConfig producerConfig;
        private ConsumerRouterSettings consumerRouterSettings;

        public KafkaProducer(EventHubKafkaAccount eventHubKafkaAccount, ConsumerRouterSettings consumerRouterSettings)
        {
            this.eventHubKafkaAccount = eventHubKafkaAccount;
            this.consumerRouterSettings = consumerRouterSettings;
        }

        public async Task WriteEventsAsync(IReadOnlyCollection<EventDataModel> changes)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = this.eventHubKafkaAccount.BrokerList,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = "$ConnectionString",
                SaslPassword = this.eventHubKafkaAccount.SaslPassword,
                SslCaLocation = this.eventHubKafkaAccount.CaCertLocation,
                //Debug = "security,broker,protocol"        //Uncomment for librdkafka debugging information
            };

            using (var producer = new ProducerBuilder<long, string>(config).SetKeySerializer(Serializers.Int64).SetValueSerializer(Serializers.Utf8).Build())
            {

                foreach (EventDataModel item in changes)
                {
                    string topic = item.topic;
                    if (consumerRouterSettings.IsIndividualTopicsRoutingEnabled)
                    {
                        if (consumerRouterSettings.WhiteListedTopies.Contains(topic))
                        {
                            await this.CreateTopicIfNotExistsAsync(item.topic);
                        }
                        else
                        {
                            topic = this.eventHubKafkaAccount.Topic;
                        }
                    }
                    else
                    {
                        topic = this.eventHubKafkaAccount.Topic;
                    }

                    string msg = $"id: {item.id}, created time: {item.createdTime} , topic: {topic}, message: {item.message}";
                    await producer.ProduceAsync(topic, new Message<long, string> { Key = DateTime.UtcNow.Ticks, Value = msg });
                    Console.WriteLine(msg);
                }
            }
        }

        private async Task CreateTopicIfNotExistsAsync(string topic)
        {
            if(!KafkaClientPool.IsTopicExists(topic))
            {
               await KafkaClientExtension.CreateTopicAsync(this.eventHubKafkaAccount.BrokerList, topic, this.eventHubKafkaAccount.SaslPassword, this.eventHubKafkaAccount.CaCertLocation);

                KafkaClientPool.AddTopic(topic);
            }
        }
    }
}
