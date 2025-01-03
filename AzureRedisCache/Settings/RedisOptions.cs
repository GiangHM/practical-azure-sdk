using System;
using System.ComponentModel.DataAnnotations;

namespace AzureRedisCache.Settings
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
        public bool RetryOnConnectionFailed { get; set; }
        public int DeltaBackOffInMilliseconds {  get; set; }
        public int MaxDeltaBackOffInMilliseconds { get; set; }
        public int ConnectRetry { get; set; }
        public int ConnectTimeout { get; set; }
        public string CacheHostName { get; set; }
        public string UserManagedIdentityId { get; set; }
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string Secret { get; set; }
        public string ConnectionMode { get; set; }
    }
}
