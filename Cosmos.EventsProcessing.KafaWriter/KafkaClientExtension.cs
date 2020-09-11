using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.EventsProcessing.KafaWriter
{
    public class KafkaClientExtension
    {
        public static async Task Producer(string brokerList, string connStr, string topic, string cacertlocation)
        {
            try
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = brokerList,
                    SecurityProtocol = SecurityProtocol.SaslSsl,
                    SaslMechanism = SaslMechanism.Plain,
                    SaslUsername = "$ConnectionString",
                    SaslPassword = connStr,
                    SslCaLocation = cacertlocation,
                    //Debug = "security,broker,protocol"        //Uncomment for librdkafka debugging information
                };
                using (var producer = new ProducerBuilder<long, string>(config).SetKeySerializer(Serializers.Int64).SetValueSerializer(Serializers.Utf8).Build())
                {
                    Console.WriteLine("Sending 10 messages to topic: " + topic + ", broker(s): " + brokerList);
                    for (int x = 0; x < 10; x++)
                    {
                        var msg = string.Format("Sample message #{0} sent at {1}", x, DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.ffff"));
                        var deliveryReport = await producer.ProduceAsync(topic, new Message<long, string> { Key = DateTime.UtcNow.Ticks, Value = msg });
                        Console.WriteLine(string.Format("Message {0} sent (value: '{1}')", x, msg));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Exception Occurred - {0}", e.Message));
            }
        }

        public static void Consumer(string brokerList, string connStr, string consumergroup, string topic, string cacertlocation)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = brokerList,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SocketTimeoutMs = 60000,                //this corresponds to the Consumer config `request.timeout.ms`
                SessionTimeoutMs = 30000,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = "$ConnectionString",
                SaslPassword = connStr,
                SslCaLocation = cacertlocation,
                GroupId = consumergroup,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                BrokerVersionFallback = "1.0.0",        //Event Hubs for Kafka Ecosystems supports Kafka v1.0+, a fallback to an older API will fail
                //Debug = "security,broker,protocol"    //Uncomment for librdkafka debugging information
            };

            using (var consumer = new ConsumerBuilder<long, string>(config).SetKeyDeserializer(Deserializers.Int64).SetValueDeserializer(Deserializers.Utf8).Build())
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

                consumer.Subscribe(topic);

                Console.WriteLine("Consuming messages from topic: " + topic + ", broker(s): " + brokerList);

                while (true)
                {
                    try
                    {
                        var msg = consumer.Consume(cts.Token);
                        Console.WriteLine($"Received: '{msg.Value}'");
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Consume error: {e.Error.Reason}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e.Message}");
                    }
                }
            }
        }

        public static async Task CreateTopicAsync(string bootstrapServers, string topicName, string connStr, string cacertlocation)
        {
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = bootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = "$ConnectionString",
                SaslPassword = connStr,
                SslCaLocation = cacertlocation,
            }).Build())
            {
                try
                {
                    await adminClient.CreateTopicsAsync(new TopicSpecification[] {
                        new TopicSpecification { Name = topicName, ReplicationFactor = 1, NumPartitions = 1 } });

                }
                catch (CreateTopicsException e)
                {
                    if (e.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
                    {
                        Console.WriteLine("Topic already exists.");
                    }
                    else
                    {
                        Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
                    }
                }
            }
        }
    }
}
