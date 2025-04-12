using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureRedisCache.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace RedisCacheAndAfInProc
{
    public class GetTopicDescriptionSample
    {
        private readonly IWrapperCacheService _cache;

        public GetTopicDescriptionSample(IWrapperCacheService cache)
        {
            _cache = cache;
        }
        [FunctionName("GetTopicDescription")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string prefixTopicCode = "TopicCode";
            string topicCode = req.Query["Code"];
            if (string.IsNullOrEmpty(topicCode))
                topicCode = "S01";

            var cacheKey = $"{prefixTopicCode}_{topicCode}";

            log.LogInformation("Cache key: {0}", cacheKey);

            var a = await _cache.GetAsync<string>(cacheKey);
            var valuefromCache = a.Value;

            log.LogInformation("Value from cache: {0}", valuefromCache);

            if (valuefromCache == null)
            {
                valuefromCache = "Decription for" + topicCode;
                await _cache.SetAsync(valuefromCache
                    , cacheKey
                    , () => new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(2)));
                log.LogInformation("Set value since no value from cache");
            }

            return new OkObjectResult(valuefromCache);
        }
    }
}
