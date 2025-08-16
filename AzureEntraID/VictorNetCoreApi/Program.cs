using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var config = builder.Configuration.GetSection("AzureAd");

        options.Authority = $"https://login.microsoftonline.com/{config["TenantId"]}/v2.0";
       // options.Audience = config["ClientId"];

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                $"https://login.microsoftonline.com/{config["TenantId"]}/v2.0",
                //$"https://sts.windows.net/{config["TenantId"]}/"
            },
            ValidateAudience = true,
            ValidAudiences = new[]
            {
                config["ClientId"]
                //config["Audience"]
            },
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        // Add Admin role to every valid token
        //options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        //{
        //    OnTokenValidated = context =>
        //    {
        //        if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
        //        {
        //            identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")); // for demo

        //            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        //            logger.LogInformation("Added Admin role to token - for demo!");
        //        }
        //        return Task.CompletedTask;
        //    }
        //};
    });

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    // Require authentication by default
    //options.FallbackPolicy = options.DefaultPolicy;

    //// Custom policies
    //options.AddPolicy(Policies.RequireAdminRole, policy =>
    //    policy.RequireRole(Roles.Admin));

    //options.AddPolicy(Policies.RequireManagerOrAbove, policy =>
    //    policy.RequireRole(Roles.Admin, Roles.Manager));

    //options.AddPolicy(Policies.RequireAnyRole, policy =>
    //    policy.RequireRole(Roles.Admin, Roles.Manager, Roles.Employee));

    //options.AddPolicy(Policies.CanManageTasks, policy =>
    //    policy.RequireAssertion(context =>
    //        context.User.IsInRole(Roles.Admin) ||
    //        context.User.IsInRole(Roles.Manager)));

    //options.AddPolicy(Policies.CanDeleteTasks, policy =>
    //    policy.RequireRole(Roles.Admin));
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API",
        Version = "v1",
        Description = "A secure Task Management API with Microsoft Entra ID authentication",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@company.com"
        }
    });

    // Configure JWT Bearer authentication for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token obtained from Microsoft Entra ID"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.DefaultModelExpandDepth(2);
        c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
        c.RoutePrefix = "swagger";
    });

}
app.UseHttpsRedirection();

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.RequireAuthorization()
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
