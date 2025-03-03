using Helper;
using Helpers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Helper
{
    public sealed class TableStorageHelper
    {
        private static readonly string RexIntegrationStorageAccount = ConfigurationHelper.Value("RexStorage");
        private static readonly CloudTableClient ClientRex = null;

        private readonly CloudTableClient Client = null;

        static TableStorageHelper()
        {
            bool execute = false;
            bool.TryParse(ConfigurationHelper.Value("Execute"), out execute);
            if (execute)
            {
                ClientRex = GenerateCloudTableClient(RexIntegrationStorageAccount, false, ConfigurationHelper.Value("RexStorageName"));
            }
        }

        public TableStorageHelper()
        {
            Client = ClientRex;
        }

        /// <summary>
        /// Genera un Cloud Table Client
        /// </summary>
        /// <param name="storageAccount">La cadena de conexión o datos del account</param>
        /// <param name="isDevelopment">Si el ambiente es development. Si es así, no larga excepción si hay error al crear el cliente</param>
        /// <param name="storageName">El nombre del storage para mostrarlo en la excepción</param>
        /// <returns></returns>
        private static CloudTableClient GenerateCloudTableClient(string storageAccount, bool isDevelopment, string storageName)
        {
            if (CloudStorageAccount.TryParse(storageAccount, out var cloudStorageAccount))
            {
                return cloudStorageAccount.CreateCloudTableClient();
            }
            else if (!isDevelopment)
            {
                throw new Exception($"Invalid {storageName} connection string");
            }
            return null;
        }

        public static T GetData<T>(string tableName, string partitionKey, string rowKey, string storageAccount) where T : ITableEntity
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storage = CloudStorageAccount.Parse(storageAccount);

            // Create the table client.
            CloudTableClient tableClient = storage.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference(tableName);

            // Create the table if it doesn't exist.
            table.CreateIfNotExistsAsync().Wait();

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);

            // Execute the operation.
            TableResult retrievedResult = AsyncHelper.RunSync(() => table.ExecuteAsync(retrieveOperation));

            return (T)retrievedResult.Result;
        }

        public void Upsert<T>(T entity, string tableName) where T : ITableEntity
        {
            if (entity == null)
            {
                throw new Exception("TableEntity no debe ser null");
            }

            if (string.IsNullOrWhiteSpace(entity.PartitionKey) || string.IsNullOrWhiteSpace(entity.RowKey))
            {
                throw new Exception("Debe indicar un PartitionKey y un RowKey");
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new Exception("Debe indicar un tableName");
            }

            if (Client == null)
            {
                return;
            }

            // Create the table client.
            CloudTableClient tableClient = Client;

            CloudTable table = tableClient.GetTableReference(tableName);

            if (!AsyncHelper.RunSync(() => table.ExistsAsync()))
            {
                AsyncHelper.RunSync(() => table.CreateIfNotExistsAsync());
            }

            TableOperation insertOperation = TableOperation.InsertOrMerge(entity);

            AsyncHelper.RunSync(() => table.ExecuteAsync(insertOperation));

        }

        /// <summary>
        /// Update or Insert mutiple records in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record"></param>
        /// <param name="tableName"></param>
        /// <param name="storageAccount"></param>
        public void Upsert<T>(List<T> record, string tableName) where T : TableEntity
        {
            if (record == null || record.Count == 0 || Client == null)
            {
                return;
            }
            CloudTableClient tableClient = Client;

            CloudTable table = tableClient.GetTableReference(tableName);

            if (!AsyncHelper.RunSync(() => table.ExistsAsync()))
            {
                AsyncHelper.RunSync(() => table.CreateIfNotExistsAsync());
            }

            var PartionKeys = record.Select(x => x.PartitionKey).Distinct();
            foreach (var iterKey in PartionKeys)
            {
                var batch = new TableBatchOperation();
                int cont = 0;
                var iterRecordList = record.Where(x => x.PartitionKey == iterKey);
                batch = new TableBatchOperation();
                foreach (var iterRecord in iterRecordList)
                {
                    cont++;
                    batch.InsertOrMerge(iterRecord);

                    if (cont % 100 == 0)
                    {
                        AsyncHelper.RunSync(() => table.ExecuteBatchAsync(batch));
                        batch = new TableBatchOperation();
                    }
                }
                if (batch.Count > 0)
                {
                    // submit
                    AsyncHelper.RunSync(() => table.ExecuteBatchAsync(batch));
                }
            }
        }
    }
}
