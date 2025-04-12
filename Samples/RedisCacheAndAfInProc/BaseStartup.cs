using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisCacheAndAfInProc
{
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Microsoft.Azure.WebJobs.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    

    namespace RedisCacheAndAfInProc
    {
        public class BaseStartup : FunctionsStartup, IWebJobsStartup
        {
            private ILoggerFactory _loggerFactory;

            // Do not Override this
            public override void Configure(IFunctionsHostBuilder builder)
            {
                ConfigureServices(builder.Services);

                // use this to add services
                DoConfigureServices(builder.Services);

                // used this to add swagger
                DoConfigureBuilder(builder);
            }

            /// <summary>
            /// https://github.com/Azure/azure-functions-host/issues/4464
            /// </summary>
            /// <param name="services"></param>
            public void ConfigureServices(IServiceCollection services)
            {
                var providers = new List<IConfigurationProvider>();

                foreach (var descriptor in services.Where(descriptor => descriptor.ServiceType == typeof(IConfiguration)).ToList())
                {
                    var existingConfiguration = descriptor.ImplementationInstance as IConfigurationRoot;
                    if (existingConfiguration is null)
                    {
                        continue;
                    }
                    providers.AddRange(existingConfiguration.Providers);
                    services.Remove(descriptor);
                }

                var executioncontextoptions = services.BuildServiceProvider()
                    .GetService<IOptions<ExecutionContextOptions>>().Value;
                var currentDirectory = executioncontextoptions.AppDirectory;

                var config = new ConfigurationBuilder();
                config.SetBasePath(currentDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)  // secrets go here. This file is excluded from source control.
                    .AddEnvironmentVariables();

                services.AddLogging();

                providers.AddRange(config.Build().Providers);

                var configRoot = new ConfigurationRoot(providers);
                services.AddSingleton<IConfiguration>(configRoot);
                services.AddHttpContextAccessor();

            }

            void IWebJobsStartup.Configure(IWebJobsBuilder builder)
            {
                Configure(builder);
            }
            protected virtual void DoConfigureBuilder(IFunctionsHostBuilder builder)
            {
            }

            protected virtual void DoConfigureServices(IServiceCollection services)
            {
            }
        }
    }

}
