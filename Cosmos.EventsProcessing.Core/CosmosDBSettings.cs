namespace Cosmos.EventsProcessing.Core
{
    public class CosmosDBSettings
    {
        public CosmosDBAccount Producer { get; set; }

        public CosmosDBAccount Consumer { get; set; }

        public CosmosDBAccount Lease { get; set; }

    }
}
