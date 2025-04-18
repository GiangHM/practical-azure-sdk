using ArticleManagementAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzureTableStorage.Extensions;
using ArticleManagementAPI.Services;
using System;
using Microsoft.Extensions.Configuration;
using AzureBlobStorage.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.ApplicationInsights;
using AzureRedisCache.Extensions;


var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureTableStorage();
builder.Services.AddTransient<ITopicTableService, TopicTableService>();
builder.Services.AddAzureRedisWithOptionsAsync();
var sasServiceBaseAddress = builder.Configuration.GetValue<string>("SasTokenService:BaseAddress");

builder.Services.AddHttpClient("SasGeneratorService", httpClient =>
{
    httpClient.BaseAddress = new Uri(sasServiceBaseAddress);
});

builder.Services.AddAutoMapper(config =>
{
    config.AddProfile(new AutoMapperProfile());
});

var frontUrl = builder.Configuration.GetValue<string>("FrontUrl");
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(frontUrl)
                          .AllowCredentials()
                          .AllowAnyHeader()
                          .AllowAnyHeader()
                          .SetIsOriginAllowed((host) => true);
                      });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Log configuration Examples
// Use Microsoft.Extensions.Logging.AzureAppServices provider
//builder.Logging.AddAzureWebAppDiagnostics();
//// Default: It will take the setting of Application logging (File system)
//// If we would like to override the setting of log level => Create Filter Or Override setting.json file
//builder.Services.Configure<AzureFileLoggerOptions>(options =>
//{
////TODO: Should have a config setting
//    options.FileName = "Victest-";
//    options.FileSizeLimit = 50 * 1024;
//    options.RetainedFileCountLimit = 5;
//});
//builder.Services.Configure<AzureBlobLoggerOptions>(options =>
//{
//    options.BlobName = "log.txt";
//});


//Use package Microsoft.Extensions.Logging.ApplicationInsights provider
//builder.Logging.AddApplicationInsights(
//        configureTelemetryConfiguration: (config) =>
//            config.ConnectionString = builder.Configuration.GetConnectionString(""),
//            configureApplicationInsightsLoggerOptions: (options) => { }
//    );

//builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("TopicController", LogLevel.Trace);

#endregion Log configuration Examples

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
