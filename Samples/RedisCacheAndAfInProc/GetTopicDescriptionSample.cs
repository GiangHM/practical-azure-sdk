using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using AzureRedisCache.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RedisCacheAndAfInProc
{
    public class GetTopicDescriptionSample
    {
        private readonly IWrapperCacheService _cache;

        public GetTopicDescriptionSample(IWrapperCacheService cache)
        {
            _cache = cache;
        }
        
        [Function("GetTopicDescription")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger("GetTopicDescription");
            log.LogInformation("C# HTTP trigger function processed a request.");

            string prefixTopicCode = "TopicCode";
            
            // Parse query parameters from HttpRequestData
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string topicCode = query["Code"];
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
                await _cache.SetAsync(cacheKey
                    , valuefromCache
                    , () => new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(2.0)));
                log.LogInformation("Set value since no value from cache");
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(valuefromCache);
            return response;
        }

        [Function("GetMultipleTopicDescription")]
        public async Task<HttpResponseData> Run1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger("GetMultipleTopicDescription");
            log.LogInformation("C# HTTP trigger function processed a request.");

            string prefixTopicCode = "TopicCode";
            
            // Parse query parameters from HttpRequestData
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string topicCode = query["Code"];
            if (string.IsNullOrEmpty(topicCode))
                topicCode = "S01";

            var cacheKey = $"{prefixTopicCode}_{topicCode}";
            var cacheKey2 = $"{prefixTopicCode}_S05";
            var inputKeys = new List<string>()
            {
                cacheKey,
                cacheKey2
            };

            log.LogInformation("Cache key: {0}", cacheKey);

            var a = await _cache.GetAsync<string>(inputKeys);
            var valuefromCache = "";
            var valuefromCache2 = "";
            if (!a.Issuccess)
            {
                valuefromCache = "Decription for" + topicCode;
                valuefromCache2 = "Decription for" + topicCode + "aaa";

                var inputDict = new Dictionary<string, string>()
                {
                    {cacheKey, valuefromCache},
                    {cacheKey2, valuefromCache2}
                };
                await _cache.SetAsync(inputDict
                    , () => new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(2.0)));
                log.LogInformation("Set value since no value from cache");
            }
            else
            {
                valuefromCache = a.Value.FirstOrDefault(x => x.Key == cacheKey).Value;
                valuefromCache2 = a.Value.FirstOrDefault(x => x.Key == cacheKey).Value;

                log.LogInformation("Value from cache: {0}", valuefromCache);
                log.LogInformation("Value 2 from cache: {0}", valuefromCache2);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(valuefromCache);
            return response;
        }
    }
}
