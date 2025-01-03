using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzureBlobStorage.Extensions;
using Microsoft.Extensions.Configuration;
using BlobSasTokenGenerationAf.Middlewares.RequestDecompression;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker =>
    {
        worker.UseRequestDecompression();
    })
    .ConfigureAppConfiguration(config => config.AddJsonFile("appsettings.json"))
    .ConfigureServices(services =>
    {
        services.AddCustomRequestDecompression();
        services.AddAzureBlobStorageWithManagedIdentity();
    })
    .Build();

host.Run();

