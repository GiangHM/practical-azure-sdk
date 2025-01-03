using Azure.Identity;
using AzureTableStorage.Services;
using AzureTableStorage.Settings;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace AzureTableStorage.Extensions
{
    public static class TableStorageExtensions
    {
        public static void AddAzureTableStorage(this IServiceCollection services
             , Action<TableStorageOption>? configure = null)
        {
            services.Configure<TableStorageOption>((options) =>
            {
                var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
                configuration.Bind("tableStorage", options);
                configure?.Invoke(options);
            });
            services.AddAzureClients(builder =>
            {
                var options = services.BuildServiceProvider().GetService<IOptionsSnapshot<TableStorageOption>>();
                var connectionString = options.Value.ConnectionString;
                var serviceName = options.Value.ServiceName ?? "AzureTableStorage";
                // We can configure TableClientOption here
                // Refer: https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclientoptions?view=azure-dotnet
                builder.AddTableServiceClient(connectionString)
                .WithName(serviceName);
            });
        }

        public static void AddAzureTableStorageWithCredential(this IServiceCollection services
             , Action<TableStorageOption>? configure = null)
        {
            services.Configure<TableStorageOption>((options) =>
            {
                var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
                configuration.Bind("tableStorage", options);
                configure?.Invoke(options);
            });
            services.AddAzureClients(builder =>
            {
                var options = services.BuildServiceProvider().GetService<IOptionsSnapshot<TableStorageOption>>();
                var serviceUri = options.Value.StorageUri;
                var clientId = options.Value.ClientId;
                var serviceName = options.Value.ServiceName ?? "AzureTableStorage";
                // We can configure TableClientOption here
                // Refer: https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclientoptions?view=azure-dotnet
                builder.AddTableServiceClient(new Uri(serviceUri))
                .WithName(serviceName);

                builder.UseCredential(new DefaultAzureCredential());

            });
        }
    }
}
