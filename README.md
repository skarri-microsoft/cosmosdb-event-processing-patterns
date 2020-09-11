# CosmosDB Event Processing Patterns using Eventhub Kafka and CosmosDB

One of the common scenarios of CosmosDB customers is processing events like Kafka using Change feed which is enabled by default on each CosmosDB SQL API account. Though there are multiple approaches to build an event processing pipeline, following are the 2 we have seen with respect to Cosmos DB:
- Cosmos DB to Cosmos DB
- Cosmos DB to Kafka/Event hub

#### Let’s start with a generic event model which simulates Kafka event:
### Example:
```sh
public class EventDataModel
{
	public string id { get; set; }
	public string topic { get; set; }
	public string message { get; set; }
	public DateTime createdTime { get; set; }
}
```
The model is self-explanatory and contains a 'topic property' which is primarily used by both producers and consumers. The idea here is a producer produces a message which contains a 'topic property' and a consumer consumes an event for subscribed topics. Here a producer can be a CosmosDB account or an EventHub, the same applies to consumer also.
#### Here is what’s covered in the sample:
##### Cosmos.EventsProcessing.Producer: 
A .net core application which writes events to the Cosmos DB, think of this has a master repository and services in your application writes events with topic name which can be service name. In case of Kafka and Even hub uses different SDKS to send this data.
##### Cosmos.EventsProcessing.Pipeline: 
A .net core application which reads the data from the Cosmos DB using change feed and writes to different topics. The topic definition depends on the technology you choose, for Cosmos DB it’s a Container and for KafkaEventhub it’s an eventhub. And a pipeline can have rules to route the topics. For example following are the rules added in this demo:
```sh
{
  "EnableIndividualTopicsRouting": true, //  for false case it goes to default Cosmos DB container or Kafka event hub
  "WhiteListedTopics": "topic-1|topic-2",//  for all other topcies it goes to default Cosmos DB container or Kafka event hub

  "Host": "Kafka"
}
```
Based on the 'Host' type data get’s redirected to KafkaEventHub or Cosmos. So, consumers can read from events from the respecive setting host setting.
#### How to pick one vs the other?
There is no one solution which fits for all scenarios. It depends on what an application needs and how we want to use the data. Here are the couple areas which can help with decision making:

* **Throughput Needs:** How many messages and topics?
* **Query:** Do you need query support to do aggregation etc.?
* **Ops Log:** Cosmos doesn’t have ops log now but it’s on road map so application may miss intermediate messages. Is this important?
* **Distributed aspects**: HA, Scale Data consistency and Backups etc.

#### Running the sample:
##### Prerequisites:
1.	CosmosDB Sql API account, if don’t have one please install local emulator using this [download link](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator-release-notes)
2.	Kafka Event hub if you plan to use writes to Kafka which uses Confluent SDK.
* Clone this repo and build it in Visual studio 2019 community version which requires .Net Core.
* After successful build, open a command prompt and run the producer exe: Cosmos.EventsProcessing.Producer.exe (will located in this folder: CosmosDBEventProcessingPatterns\Cosmos.EventsProcessing.Producer\bin\Debug\netcoreapp3.1)
* Now the run the pipeline project from the Visual studio itself to see end to end.
#### Settings:
* **cosmosProducerSettings** – Events source information.
* **consumerRouterSettings** - Router settings which includes redirecting data to Cosmos or Kafka Eventhub.
* **cosmosConsumerSettings** – Events from Produces goes to this location. From there, based on the permissions application can consume the data.
* **kafkaConsumerSettings** – Eventhub kafka settings.
#### Notes:
- For every pipeline run it starts reading data from the beginning
- For each new topic if it is whitelisted it creates a new Cosmos DB container or a new Event hub.
