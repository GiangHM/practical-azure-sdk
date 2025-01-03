using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BlobSasTokenGenerationAf.Middlewares.RequestDecompression
{
    internal sealed class RequestDecompressionMiddleware(IDecompressionProvider gzipProvider
            , ILogger<RequestDecompressionMiddleware> logger) : IFunctionsWorkerMiddleware
    {

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var requestData = await context.GetHttpRequestDataAsync();
            requestData.Headers.TryGetValues("Content-Encoding", out var encoding);

            if (encoding == null)
                logger.LogDebug("The Content-Encoding header is empty or not specified. Skipping request decompression.");

            if (encoding != null && encoding.Count() > 1)
                logger.LogDebug("Request decompression is not supported for multiple Content-Encodings.");

            if (encoding.FirstOrDefault() == "gzip")
            {
                var compressionBody = await requestData.ReadAsStringAsync();
                var res = gzipProvider.GetDecompression(compressionBody);
                context.Items.Add("DecompressedBody", res);
                await next(context);
            }

            await next(context);
        }
    }
}
