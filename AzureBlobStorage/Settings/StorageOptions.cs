using System;
using System.Collections.Generic;
using System.Text;

namespace AzureBlobStorage.Settings
{
    public class StorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Container { get; set; } = string.Empty;
        public string StorageName { get; set; } = string.Empty;
        public int MaxRetry { get; set; } = 3;
        public int MaxDelay { get; set; }
        public string StorageUri { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public bool IsUseProxy { get; set; }
        public string ProxyEndPoint { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
