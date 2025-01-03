using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobSasTokenGenerationAf.Middlewares.RequestDecompression
{
    public static class RequestDecompressionBuilderExtensions
    {
        public static IFunctionsWorkerApplicationBuilder UseRequestDecompression(this IFunctionsWorkerApplicationBuilder builder)
        {
            builder.UseMiddleware<RequestDecompressionMiddleware>();
            return builder;
        }
    }
}
