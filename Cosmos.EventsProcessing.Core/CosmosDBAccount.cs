namespace Cosmos.EventsProcessing.Core
{
    public class CosmosDBAccount
    {
        public string Endpoint { get; set; }

        public string AuthKey { get; set; }

        public string Database { get; set; }

        public string Container { get; set; }

        public string PartitionKeyPath { get; set; }

        public bool CreateIfDatabaseNotExists { get; set; }

        public bool CreateIfContainerNotExists { get; set; }

        public bool DeleteIfDatabaseExists { get; set; }
        public bool DeleteIfContainerExists { get; set; }

    }
}
