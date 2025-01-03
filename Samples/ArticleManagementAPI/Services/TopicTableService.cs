using ArticleManagementAPI.Entities;
using ArticleManagementAPI.Models;
using ArticleManagementAPI.Services;
using Azure.Data.Tables;
using AzureTableStorage.Services;
using AzureTableStorage.Settings;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArticleManagementAPI.Services
{
    public class TopicTableService : TableStorageServiceBase<TopicEntity>, ITopicTableService
    {

        public TopicTableService(IAzureClientFactory<TableServiceClient> azureClientFactory
             , IOptions<TableStorageOption> options) : base(azureClientFactory, options) { }

        public async Task<TopicEntity> AddEntity(TopicEntity entity)
        {
            return await base.InsertOrUpadteEntityAsync(entity);
        }

        public Task<IEnumerable<TopicEntity>> GetAllData()
        {
            return GetAll();
        }

        public override string GetTableName()
        {
            return "TopicTable";
        }

        
    }
}
