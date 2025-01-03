using System.Net;
using System.Threading.Tasks;
using AzureBlobStorage.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BlobSasTokenGenerationAf
{
    public class SasGenerator
    {
        private readonly ILogger _logger;
        private readonly IBlobStorageService _blobService;
        public SasGenerator(ILoggerFactory loggerFactory
            , IBlobStorageService blobService)
        {
            _logger = loggerFactory.CreateLogger<SasGenerator>();
            _blobService = blobService;
        }

        [Function("GenerateBlobSasToken")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req
            , [FromQuery] string fileName
            , [FromQuery] string containerName)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var body = req.FunctionContext.Items["DecompressedBody"];

            var url = await _blobService.CreateUserDelegationSasAsync(containerName
                , fileName
                , 1
                , null
                , Azure.Storage.Sas.BlobContainerSasPermissions.Write);

            return new OkObjectResult(url);
        }
    }
}
