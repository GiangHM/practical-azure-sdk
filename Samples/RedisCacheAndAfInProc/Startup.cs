using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using RedisCacheAndAfInProc;
using RedisCacheAndAfInProc.RedisCacheAndAfInProc;
using AzureRedisCache.Extensions;


[assembly: FunctionsStartup(typeof(Startup))]
namespace RedisCacheAndAfInProc
{
    public class Startup : BaseStartup
    {
        protected override void DoConfigureServices(IServiceCollection services)
        {
            services.AddAzureRedisWithOptions();
        }
    }
}
