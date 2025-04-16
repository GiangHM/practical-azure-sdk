using Microsoft.Extensions.Options;
using StackExchange.Redis.Profiling;
using StackExchange.Redis;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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

    /// <summary>
    /// Configuration options for <see cref="RedisCache"/>.
    /// </summary>
    public class RedisCacheOptions : IOptions<RedisCacheOptions>
    {
        /// <summary>
        /// The configuration used to connect to Redis.
        /// </summary>
        public string? Configuration { get; set; }

        /// <summary>
        /// The configuration used to connect to Redis.
        /// This is preferred over Configuration.
        /// </summary>
        public ConfigurationOptions? ConfigurationOptions { get; set; }

        /// <summary>
        /// Gets or sets a delegate to create the ConnectionMultiplexer instance.
        /// </summary>
        public Func<Task<IConnectionMultiplexer>>? ConnectionMultiplexerFactory { get; set; }

        /// <summary>
        /// The Redis instance name. Allows partitioning a single backend cache for use with multiple apps/services.
        /// If set, the cache keys are prefixed with this value.
        /// </summary>
        public string? InstanceName { get; set; }

        /// <summary>
        /// The Redis profiling session
        /// </summary>
        public Func<ProfilingSession>? ProfilingSession { get; set; }

        RedisCacheOptions IOptions<RedisCacheOptions>.Value
        {
            get { return this; }
        }

        private bool? _useForceReconnect;
        internal bool UseForceReconnect
        {
            get
            {
                return _useForceReconnect ??= GetDefaultValue();
                static bool GetDefaultValue() =>
                    AppContext.TryGetSwitch("Microsoft.AspNetCore.Caching.StackExchangeRedis.UseForceReconnect", out var value) && value;
            }
            set => _useForceReconnect = value;
        }

        internal ConfigurationOptions GetConfiguredOptions()
        {
            var options = ConfigurationOptions ?? ConfigurationOptions.Parse(Configuration!);

            // we don't want an initially unavailable server to prevent DI creating the service itself
            options.AbortOnConnectFail = false;

            return options;
        }
    }
}
