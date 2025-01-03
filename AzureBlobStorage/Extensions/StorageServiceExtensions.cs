using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Storage.Blobs;
using AzureBlobStorage.Services;
using AzureBlobStorage.Settings;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client.AppConfig;
using System;
using System.Net;
using System.Net.Http;

namespace AzureBlobStorage.Extensions
{
    public static class StorageServiceExtensions
    {
        /// <summary>
        /// Add service client to DI container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        public static void AddAzureBlobStorageWithManagedIdentity(this IServiceCollection services
             , Action<StorageOptions>? configure = null)
        {
            services.Configure<StorageOptions>((options) =>
            {
                var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
                configuration.Bind("blobStorage", options);
                configure?.Invoke(options);
            });
            services.AddAzureClients(builder =>
            {
                var options = services.BuildServiceProvider().GetService<IOptions<StorageOptions>>();
                var storageName = string.IsNullOrEmpty(options?.Value.StorageName) ? "AzureBlobStorage" : options?.Value.StorageName;

                var managedIdentity = options?.Value.ClientId ?? null;

                // Add a Storage account client
                builder.AddBlobServiceClient(new Uri(options.Value.StorageUri))
                .WithName(storageName)
                .ConfigureOptions(option =>
                {
                    if (options?.Value.IsUseProxy ?? false)
                        option.Transport = new HttpClientTransport(ConfigureProxy(options));

                    option.Retry.Mode = Azure.Core.RetryMode.Exponential;
                    option.Retry.MaxRetries = options?.Value.MaxRetry ?? 3;
                    option.Retry.MaxDelay = TimeSpan.FromSeconds(options?.Value.MaxDelay ?? 3);
                });

                builder.UseCredential(new ChainedTokenCredential(
                    new VisualStudioCredential(),
                    new ManagedIdentityCredential(managedIdentity)));
            });
            services.AddTransient<IBlobStorageService, BlobStorageService>();
        }
        /// <summary>
        /// Add service client to DI container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        public static void AddAzureBlobStorage(this IServiceCollection services
             , Action<StorageOptions>? configure = null)
        {
            services.Configure<StorageOptions>((options) =>
            {
                var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
                configuration.Bind("blobStorage", options);
                configure?.Invoke(options);
            });
            services.AddAzureClients(builder =>
            {
                var options = services.BuildServiceProvider().GetService<IOptions<StorageOptions>>();
                var storageName = string.IsNullOrEmpty(options?.Value.StorageName) ? "AzureBlobStorage" : options?.Value.StorageName;
                // Add a Storage account client

                builder.AddBlobServiceClient(options.Value.ConnectionString)
                .WithName(storageName)
                .ConfigureOptions(option =>
                {
                    if (options?.Value.IsUseProxy ?? false)
                        option.Transport = new HttpClientTransport(ConfigureProxy(options));

                    option.Retry.Mode = Azure.Core.RetryMode.Exponential;
                    option.Retry.MaxRetries = options.Value.MaxRetry;
                    option.Retry.MaxDelay = TimeSpan.FromSeconds(options.Value.MaxDelay);
                });
            });
            services.AddTransient<IBlobStorageService, BlobStorageService>();
        }

        private static HttpClientHandler ConfigureProxy(IOptions<StorageOptions>? options)
        {
            return new HttpClientHandler
            {
                Proxy = new WebProxy
                {
                    Address = new Uri(options?.Value.ProxyEndPoint),
                    BypassProxyOnLocal = true,
                    Credentials = new NetworkCredential
                    {
                        UserName = options?.Value.UserName,
                        Password = options?.Value.Password
                    },
                    UseDefaultCredentials = false,
                }
            };
        }
    }
}
