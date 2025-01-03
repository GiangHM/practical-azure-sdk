using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using ArticleManagementWebApp.Models;

namespace ArticleManagementWebApp.Controllers
{    
    public class TopicController : Controller
    {
        private readonly ILogger<TopicController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public TopicController(ILogger<TopicController> logger
            , IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> IndexAsync()
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri("");
                var res = await httpClient.GetAsync("Topic/Topics");
                if (res.IsSuccessStatusCode)
                {
                    var text = await res.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<List<TopicResponseModel>>(text);
                    var topicList = response.Select(x =>
                    new TopicViewModel
                    {
                        Name = x.TopicDescription
                    });
                    ViewBag.Topics = topicList.ToList();
                }
            }
            return View();

        }

        public IActionResult Privacy()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
