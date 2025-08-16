using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// This app instance should be a long-lived instance because
// it maintains the in-memory token cache
// If it is azure function, we should use another way to cache the token

var config = builder.Configuration;
var clientId = config["clientid"];
var clientSecret = config["clientsecret"];
var authority = $"https://login.microsoftonline.com/{config["TenantId"]}/v2.0";

IConfidentialClientApplication msalClient = ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();

builder.Services.AddSingleton(msalClient);

builder.Services.AddHttpClient();

builder.Build().Run();
