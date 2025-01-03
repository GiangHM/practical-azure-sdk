using System;
using System.Collections.Generic;
using System.Text;

namespace AzureTableStorage.Settings
{
    public class TableStorageOption
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public string ServiceName { get; set; }
        public string ClientId { get; set; }
        public string StorageUri { get; set; } = string.Empty;
    }
}
