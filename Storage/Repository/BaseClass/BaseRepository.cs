using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Table;
using PaceMe.Storage.Utilities;
using System;

namespace PaceMe.Storage.Repository 
{
    public interface IBaseRepository<T>
    {
        Task Create(T record);
        Task Update(T record);
        Task Delete(T record);
    }

    public abstract class BaseRepository<T>
    {
        private readonly string _tablename;
        private readonly string _storageAccountConnectionString;

        public BaseRepository(string tableName, IConfiguration configuration){
            _tablename = tableName;
            _storageAccountConnectionString = configuration[ConnectionStringConstants.PaceMeStorageAccount];
        }

        public async Task<IEnumerable<T>> GetForParentId(Guid partitionKey)
        {
            return await GetForParentId(partitionKey.ToString());
        }

        public abstract DynamicTableEntity GenerateEntityFromRecord(T record);

        public async Task<IEnumerable<T>> GetForParentId(string partitionKey)
        {
            CloudTable trainingPlansTable = await GetTable();

            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where($"PartitionKey eq '{partitionKey}'");
            var trainingPlanEntities = await FindEntityList(trainingPlansTable, query, null, new List<DynamicTableEntity>());
            return trainingPlanEntities.Select(x => TableEntity.ConvertBack<T>(x.Properties, new OperationContext())).ToList();
        }


        public async Task<T> GetById(Guid rowId)
        {
            return await GetById(rowId.ToString());
        }
        
        public async Task<T> GetById(string rowId)
        {
            CloudTable trainingPlansTable = await GetTable();

            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where($"RowKey eq '{rowId}'");
            var trainingPlanEntities = await FindEntityList(trainingPlansTable, query, null, new List<DynamicTableEntity>());
            var storedEntity = trainingPlanEntities.FirstOrDefault();
            return storedEntity == null ? default(T) : TableEntity.ConvertBack<T>(storedEntity.Properties, new OperationContext());
        }

        protected static async Task<DynamicTableEntity> FindFirstEntity(CloudTable table, TableQuery<DynamicTableEntity> query, TableContinuationToken token){
            TableQuerySegment<DynamicTableEntity> seg = await table.ExecuteQuerySegmentedAsync<DynamicTableEntity>(query, token);
            token = seg.ContinuationToken;

            if(seg.Results.Count == 1)
            {
                return seg.Results.First();
            }

            if(seg.ContinuationToken == null)
            {
                return default(DynamicTableEntity);
            }

            return await FindFirstEntity(table, query, seg.ContinuationToken);
        }


        public async Task Create(T record)
        {   
            var entity = GenerateEntityFromRecord(record);
            CloudTable trainingPlansTable = await GetTable();

            TableOperation insertOperation = TableOperation.Insert(entity);

            await trainingPlansTable.ExecuteAsync(insertOperation);
        }

        public async Task Update(T record){
            var entity = GenerateEntityFromRecord(record);

            CloudTable trainingPlansTable = await GetTable();

            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where($"PartitionKey eq '{entity.PartitionKey}' and RowKey eq '{entity.RowKey}'").Take(1);

            DynamicTableEntity storageEntity = await FindFirstEntity(trainingPlansTable, query, null);

            storageEntity.Properties = entity.Properties;
            storageEntity.ETag = "*";

            var replaceOperation = TableOperation.Replace(storageEntity);
            await trainingPlansTable.ExecuteAsync(replaceOperation);
        }

        public async Task Delete(T record){
            var entity = GenerateEntityFromRecord(record);

            CloudTable trainingPlansTable = await GetTable();

            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where($"PartitionKey eq '{entity.PartitionKey}' and RowKey eq '{entity.RowKey}'").Take(1);

            DynamicTableEntity storageEntity = await FindFirstEntity(trainingPlansTable, query, null);
            storageEntity.ETag = "*";

            var deleteOperation = TableOperation.Delete(storageEntity);
            await trainingPlansTable.ExecuteAsync(deleteOperation);
        }

        protected static async Task<List<DynamicTableEntity>> FindEntityList(CloudTable table, TableQuery<DynamicTableEntity> query, TableContinuationToken token, List<DynamicTableEntity> results)
        {
            TableQuerySegment<DynamicTableEntity> segment = await table.ExecuteQuerySegmentedAsync<DynamicTableEntity>(query, token);

            if(segment.Results.Count > 0)
            {
                results.AddRange(segment.Results);
            }

            if(segment.ContinuationToken == null)
            {
                return results;
            }

            return await FindEntityList(table, query, segment.ContinuationToken, results);
        }

        protected async Task<CloudTable> GetTable(){
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageAccountConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            
            CloudTable table = tableClient.GetTableReference(_tablename);

            await table.CreateIfNotExistsAsync();

            return table;
        }

    }
}