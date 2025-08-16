using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Text.Json;
namespace DemoFunctionCallAPI;

public class DemoFunction
{
    private readonly ILogger<DemoFunction> _logger;
    private IHttpClientFactory _httpClientFactory;
    private IConfiguration _configuration;
    private readonly IConfidentialClientApplication _msalClient;
    public DemoFunction(ILogger<DemoFunction> logger
        , IHttpClientFactory httpClientFactory
        , IConfiguration configuration
        , IConfidentialClientApplication msalClient)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _msalClient = msalClient;
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

    #region Developer implement Get Token and Call Downstream API by himself
    private async Task<string> CallVictorNetCoreDemoApi(TokenInformation token)
    {
        using (var client = _httpClientFactory.CreateClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7153/Secure/GetString");
            request.Headers.Add("Authorization", $"{token.token_type} {token.access_token}");
            var response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
    }
    private async Task<string> CallVictorNetCoreDemoApi(AuthenticationResult token)
    {
        using (var client = _httpClientFactory.CreateClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7153/OnBehalf/GetWeatherForecastObo");
            request.Headers.Add("Authorization", $"{token.TokenType} {token.AccessToken}");
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
    #endregion

    /// <summary>
    /// This function uses MSAL to acquire a token and calls the API by user implemted method.
    /// We can use the 'Microsoft.Identity.Web and Microsoft.Identity.Web.DownstreamApi' packages:
    /// But this would loss the ability to control the token acquisition process and the API call.
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("Function2UsingMSAL")]
    public async Task<IActionResult> RunFunction2Async([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        _logger.LogInformation("Request token");
        var scope = _configuration["scope"];
        AuthenticationResult msalAuthenticationResult = await _msalClient.AcquireTokenForClient(
            new string[] { scope }).ExecuteAsync();

        var response = await CallVictorNetCoreDemoApi(msalAuthenticationResult);

        return new OkObjectResult(response);
    }
}