using ArticleManagementAPI.Entities;
using ArticleManagementAPI.Models;
using ArticleManagementAPI.Services;
using AutoMapper;
using AzureRedisCache.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArticleManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TopicController : ControllerBase
    {
        private readonly ILogger<TopicController> _logger;
        private readonly ITopicTableService _topicTableService;
        private readonly IMapper _mapper;
        private readonly IWrapperCacheService _cache;
        private readonly string CacheKey_All = "AllTopics";
        private const string TopicPrefix = "TopicId_";


        public TopicController(ILogger<TopicController> logger
            , ITopicTableService topicTableService
            , IMapper mapper
            , IWrapperCacheService cache)
        {
            _logger = logger;
            _topicTableService = topicTableService;
            _mapper = mapper;
            _cache = cache;
        }

        [HttpGet("Topics")]
        public async Task<IEnumerable<TopicResponseModel>> GetAll()
        {
            _logger.LogInformation("Get all topics - logging scenario 3");
            var entities = (await _cache.GetAsync<IEnumerable<TopicEntity>>(CacheKey_All)).Value;
            if (entities == null) 
            {
                entities = await _topicTableService.GetAllData();
                await _cache.SetAsync(entities
                    , CacheKey_All
                    , () => new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(2))
                    );
            }
            var res = _mapper.Map<IEnumerable<TopicEntity>, IEnumerable<TopicResponseModel>>(entities);
            return res;
        }
        [HttpPost("Topic")]
        public async Task<bool> CreateNewTopic(TopicCreationRequestModel model)
        {
            _logger.LogInformation("Create new topic - logging scenario 3");
            var entity = _mapper.Map<TopicEntity>(model);
            var res = await _topicTableService.AddEntity(entity);
            await _cache.RemoveAsync(CacheKey_All);
            return res.TopicName == model.TopicName;
        }
        [HttpGet("Topics/{code}")]
        public async Task<TopicResponseModel> GetByCode([FromRoute]string code)
        {
            _logger.LogInformation("Get by code - logging scenario 3");
            var entity = (await _cache.GetAsync<TopicEntity>(TopicPrefix + code)).Value;
            if (entity == null)
            {
                var entities = await _topicTableService.GetAllData();
                entity = entities?.FirstOrDefault(x=> x.TopicCode == code);
                await _cache.SetAsync(entity
                    , TopicPrefix + code
                    , () => new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(2))
                    );
            }
            var res = _mapper.Map<TopicEntity, TopicResponseModel>(entity);
            return res;
        }
    }
}
