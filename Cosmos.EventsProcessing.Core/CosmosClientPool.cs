using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.EventsProcessing.Core
{
    public class CosmosClientPool
    {
        private static readonly object lockObject = new object();
        private const int defaultTimeoutInMilliseconds = 4000;
        private static ConcurrentDictionary<string,CosmosClient> cosmosClients = new ConcurrentDictionary<string, CosmosClient>();
        private static ConcurrentBag<string> topics = new ConcurrentBag<string>();

        public static void CreateClient(CosmosDBAccount cosmosDBAccount)
        {
            using (var lockKey = Lockkey.GetLock(lockObject, defaultTimeoutInMilliseconds))
            {
                if (!cosmosClients.ContainsKey(cosmosDBAccount.Endpoint))
                {
                    cosmosClients[cosmosDBAccount.Endpoint] = new CosmosClient(cosmosDBAccount.Endpoint, cosmosDBAccount.AuthKey);
                }
            }
        }

        public static void AddTopic(string topic)
        {
            using (var lockKey = Lockkey.GetLock(lockObject, defaultTimeoutInMilliseconds))
            {
                if (!topics.Contains(topic))
                {
                    topics.Add(topic);
                }
            }
        }

        public static bool IsTopicExists(string topic)
        {
            if (!topics.Contains(topic))
            {
                return false;
            }
            return true;
        }

        public static CosmosClient GetCosmosClient(CosmosDBAccount cosmosDBAccount)
        {
            if (!cosmosClients.ContainsKey(cosmosDBAccount.Endpoint))
            {
                CreateClient(cosmosDBAccount);
            }
            return cosmosClients[cosmosDBAccount.Endpoint];
        }
    }
}
