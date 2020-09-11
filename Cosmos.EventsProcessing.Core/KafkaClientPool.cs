namespace Cosmos.EventsProcessing.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    public class KafkaClientPool
    {
        private static readonly object lockObject = new object();
        private const int defaultTimeoutInMilliseconds = 4000;
        private static ConcurrentBag<string> topics = new ConcurrentBag<string>();

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

    }
}
