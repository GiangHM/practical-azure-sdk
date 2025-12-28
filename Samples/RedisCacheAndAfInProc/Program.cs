using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using AzureRedisCache.Extensions;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        
        // Add Redis cache services
        services.AddAzureRedisWithOptionsAsync();
    })
    .Build();

host.Run();
