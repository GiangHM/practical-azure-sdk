using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DemoFunctionCallAPI;

public class DemoFunction
{
    private readonly ILogger<DemoFunction> _logger;
    private IHttpClientFactory _httpClientFactory;
    private IConfiguration _configuration;

    public DemoFunction(ILogger<DemoFunction> logger
        , IHttpClientFactory httpClientFactory
        , IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [Function("Function1")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        _logger.LogInformation("Request token");
        var clientId = _configuration["clientid"];
        var clientSecret = _configuration["clientsecret"];
        var scope = _configuration["scope"];

        var jsonToken = await GetTokenAsync(clientId
            , clientSecret
            , scope);

        _logger.LogWarning("Token: {0}", jsonToken);

        var token = JsonSerializer.Deserialize<TokenInformation>(jsonToken);

        var response = await CallVictorNetCoreDemoApi(token);

        return new OkObjectResult(response);
    }

    private async Task<string> CallVictorNetCoreDemoApi(TokenInformation token)
    {
        using (var client = _httpClientFactory.CreateClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44317/Secure/GetString");
            request.Headers.Add("Authorization", $"{token.token_type} {token.access_token}");
            var response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
    }
    private async Task<string> GetTokenAsync(string clientId, string clientSecret, string scope)
    {
        using (var client = _httpClientFactory.CreateClient())
        {
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "scope", scope }
            };

            var content = new FormUrlEncodedContent(parameters);

            var tenantId = _configuration["tenantid"];
            var response = await client.PostAsync($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token", content);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            return json; // returns full token response as JSON
        }
    }

    class TokenInformation
    {
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        public string access_token { get; set; }
    }
}