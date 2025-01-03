using Azure;
using Azure.Data.Tables;
using System;

namespace ArticleManagementAPI.Entities
{
    public class TopicEntity : ITableEntity
    {
        public TopicEntity() { }
        public string TopicName { get; set; }
        public string TopicCode { get; set; }
        public string TopicDescription { get; set; }
        public bool IsActive { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
