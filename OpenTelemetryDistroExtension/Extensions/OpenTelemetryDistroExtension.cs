using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryDistroExtension.Options;

namespace OpenTelemetryDistroExtension.Extensions
{
    public static class OpenTelemetryDistroExtension
    {
        public static void AddOpenTelemetryDistro(this IServiceCollection services, Action<AppInsightOption>? configure = null)
        {
            services.Configure<AppInsightOption>((options) =>
            {
                var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
                configuration.Bind("AIMonitor", options);
                configure?.Invoke(options);
            });

            var option = services.BuildServiceProvider().GetService<IOptions<AppInsightOption>>().Value;

            services.AddOpenTelemetry().UseAzureMonitor(config =>
            {

                config.Credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = option.ClientId
                });

                if (option.IsUseSampling)
                    config.SamplingRatio = option.FixSampling;

                config.EnableLiveMetrics = option.LiveMetric;
            });

            var resourceAttributes = new Dictionary<string, object> {
                { "service.name", option.AIRoleName }};

            // Configure the OpenTelemetry tracer provider to add the resource attributes to all traces.
            services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
            {
                builder.ConfigureResource(config => config.AddAttributes(resourceAttributes));
            });
        }
    }
}
