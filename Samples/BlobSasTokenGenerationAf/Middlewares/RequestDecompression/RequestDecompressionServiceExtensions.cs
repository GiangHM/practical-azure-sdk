using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobSasTokenGenerationAf.Middlewares.RequestDecompression
{
    public static class RequestDecompressionServiceExtensions
    {
        public static IServiceCollection AddCustomRequestDecompression(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddTransient<IDecompressionProvider, GzipDecompressionProvider>();
            return services;
        }
    }
}
