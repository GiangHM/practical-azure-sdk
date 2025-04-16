using Azure.Identity;
using AzureRedisCache.Models;
using AzureRedisCache.Services;
using AzureRedisCache.Settings;
using Microsoft.Azure.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;



namespace AzureRedisCache.Extensions
{
    public static class AzureRedisExtensions
    {
        public static void AddAzureRedisWithOptions(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();

            services.Configure<RedisOptions>(configuration.GetSection("AzureRedisCache"));
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<RedisOptions>, ValidateRedisOptions>());

            services.AddRedisCache(options =>
            {
                var userConfigs = services.BuildServiceProvider().GetService<IOptions<RedisOptions>>().Value;
                options.InstanceName = userConfigs.InstanceName ?? "DefaultRedisCache";
                options.ConnectionMultiplexerFactory = async () =>
                {
                    var configurationOptions = await GetConfigsAsync(userConfigs);

                    if (userConfigs.RetryOnConnectionFailed)
                        configurationOptions = configurationOptions.EnrichOptionWithRetry(userConfigs);

                    IConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
                    return connection;
                };
            });
        }

        private static IServiceCollection AddRedisCache(this IServiceCollection services, Action<RedisCacheOptions> setupAction)
        {
            services.AddOptions();
            services.Configure(setupAction);

            services.AddSingleton<IWrapperCacheService, WrapperCacheService>();

            return services;
        }

        private static async Task<ConfigurationOptions> GetConfigsAsync(RedisOptions option)
        {
            var cacheHostName = option.CacheHostName;
            switch (option.ConnectionMode)
            {
                case ConnectionOptionConstants.Default:
                    var configurationOptions = await ConfigurationOptions.Parse($"{cacheHostName}:6380")
                        .ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
                    return configurationOptions;
                case ConnectionOptionConstants.UseUserManagedIdentity:
                    var managedIdentityId = option.UserManagedIdentityId;
                    configurationOptions = await ConfigurationOptions.Parse($"{cacheHostName}:6380")
                        .ConfigureForAzureWithUserAssignedManagedIdentityAsync(managedIdentityId!);
                    return configurationOptions;
                case ConnectionOptionConstants.UseSystemManagedIdentity:
                    cacheHostName = "";
                    configurationOptions = await ConfigurationOptions.Parse($"{cacheHostName}:6380")
                        .ConfigureForAzureWithSystemAssignedManagedIdentityAsync();
                    return configurationOptions;
                case ConnectionOptionConstants.UseServicePrincipal:
                    var clientId = option.ClientId;
                    var tenantId = option.TenantId;
                    var secret = option.Secret;

                    configurationOptions = await ConfigurationOptions.Parse($"{cacheHostName}:6380")
                        .ConfigureForAzureWithServicePrincipalAsync(clientId!, tenantId!, secret!);
                    return configurationOptions;
                case ConnectionOptionConstants.UseAccessKey:
                    configurationOptions = ConfigurationOptions.Parse(option.ConnectionString, true);
                    return configurationOptions;
                default:
                    break;
            }
            return new ConfigurationOptions();
        }
        private static ConfigurationOptions EnrichOptionWithRetry(this ConfigurationOptions option, RedisOptions userConfigs)
        {
            var deltaBackOffInMilliseconds = (int)TimeSpan.FromSeconds(userConfigs.DeltaBackOffInMilliseconds).TotalMilliseconds;
            var maxDeltaBackOffInMilliseconds = (int)TimeSpan.FromSeconds(userConfigs.MaxDeltaBackOffInMilliseconds).TotalMilliseconds;
            option.ConnectRetry = userConfigs.ConnectRetry;
            option.ReconnectRetryPolicy = new ExponentialRetry(deltaBackOffInMilliseconds, maxDeltaBackOffInMilliseconds);
            option.ConnectTimeout = userConfigs.ConnectTimeout;
            return option;
        }
    }
    
}
