using Azure;
using Azure.Data.Tables;
using AzureTableStorage.Settings;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTableStorage.Services
{
    public abstract class TableStorageServiceBase<T> where T: class, ITableEntity
    {
        private readonly IAzureClientFactory<TableServiceClient> _azureClientFactory;
        private readonly IOptions<TableStorageOption> _options;
        readonly TableClient _tableClient;
        protected TableStorageServiceBase(IAzureClientFactory<TableServiceClient> azureClientFactory
            , IOptions<TableStorageOption> options)
        {
            _azureClientFactory = azureClientFactory;
            _options = options;
            _tableClient = GetTableClient();
        }
        private TableClient GetTableClient()
        {
            var tableName = GetTableName();
            var tableServiceClient = _azureClientFactory.CreateClient(_options.Value.ServiceName ?? "AzureTableStorage");
            var tableClient = tableServiceClient.GetTableClient(tableName);
            tableClient.CreateIfNotExists();
            return tableClient;
        }
        public abstract string GetTableName();

        public async Task<T> InsertOrUpadteEntityAsync( T entity, int updateMode = 0)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var tableUpdateMode = updateMode == 0 ? TableUpdateMode.Merge : TableUpdateMode.Replace;
            await _tableClient.UpsertEntityAsync(entity, tableUpdateMode);
            return entity;
        }
        public async Task<T> GetEntityAsync( string partitionKey, string rowKey, IEnumerable<string> select = null, CancellationToken cancellationToken = default)
        {
            if (partitionKey == null || rowKey == null)
                throw new ArgumentNullException($"There is a parameter is null: rowkey {rowKey} or partition key {partitionKey}");
            return await _tableClient.GetEntityAsync<T>(partitionKey, rowKey, select, cancellationToken);
        }
        public async Task<bool> DeleteEntityAsync(string partitionKey, string rowKey)
        {
            if (partitionKey == null || rowKey == null)
                throw new ArgumentNullException($"There is a parameter is null: rowkey {rowKey} or partition key {partitionKey}");
            var res = await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
            return res.Status == 200;
        }
        public async Task<IEnumerable<T>> QueryEntityAsync( Expression<Func<T, bool>> filter,
            IEnumerable<string> select = null,
            CancellationToken cancellationToken = default)
        {
            var queryResultsMaxPerPage = _tableClient.QueryAsync<T>(filter, null, select, cancellationToken);

            List<T> response = new List<T>();

            await foreach (Page<T> page in queryResultsMaxPerPage.AsPages())
            {
                foreach (T qEntity in page.Values)
                {
                    response.Add(qEntity);
                }
            }
            return response;
        }
        public async Task AddMultipleEntity(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            List<TableTransactionAction> actions = new List<TableTransactionAction>();
            actions.AddRange(entities.Select(x => new TableTransactionAction(TableTransactionActionType.Add, x)));
            _ = await _tableClient.SubmitTransactionAsync(actions).ConfigureAwait(false);

        }

        public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> filterParam = null)
        {
            if (filterParam == null)
                filterParam = p => true;

            var queryResultsMaxPerPage = _tableClient.QueryAsync<T>(filterParam, null);

            List<T> response = new List<T>();

            await foreach (Page<T> page in queryResultsMaxPerPage.AsPages())
            {
                foreach (T qEntity in page.Values)
                {
                    response.Add(qEntity);
                }
            }
            return response;
        }
    }
}
