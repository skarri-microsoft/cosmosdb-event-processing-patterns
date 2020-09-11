namespace Cosmos.EventsProcessing.Core
{
    using Microsoft.Azure.Cosmos;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class CosmosExtension
    {
        public static async Task<Database> SetupDatabaseAsync(
           string databaseId,
           bool createIfNotExists,
           bool deleteIfExists,
           CosmosClient client)
        {
            Database database;

            if (deleteIfExists)
            {
                try
                {
                    database = await client.GetDatabase(databaseId).ReadAsync();
                    await database.DeleteAsync();
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                }
            }
            if (createIfNotExists)
            {
                return await client.CreateDatabaseIfNotExistsAsync(databaseId);
            }
            throw new Exception($"Missing database : {databaseId}");

        }

        public static async Task<Container> SetupCollectionAsync(
          Database database,
          string collectionName,
          string partitionkeyPath,
          bool createIfNotExists,
          bool deleteIfExists)

        {

            Container container;

            if (deleteIfExists)
            {
                try
                {
                    container = database.GetContainer(collectionName);
                    await container.DeleteContainerAsync();
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                }
            }
            if (createIfNotExists)
            {
                return await database.CreateContainerIfNotExistsAsync(new ContainerProperties(collectionName, partitionkeyPath));
            }

            throw new Exception($"Missing container : {collectionName}");
        }

        public static async Task SetupCosmosDBEntitiesAsync(CosmosDBAccount cosmosDBAccount)
        {
            Database database = await CosmosExtension.SetupDatabaseAsync(cosmosDBAccount.Database, cosmosDBAccount.CreateIfContainerNotExists, cosmosDBAccount.DeleteIfDatabaseExists, CosmosClientPool.GetCosmosClient(cosmosDBAccount));
            await CosmosExtension.SetupCollectionAsync(database, cosmosDBAccount.Container, cosmosDBAccount.PartitionKeyPath, cosmosDBAccount.CreateIfContainerNotExists, cosmosDBAccount.DeleteIfContainerExists);

        }
    }
}
