using AzureRedisCache.Helpers;
using AzureRedisCache.Models;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace AzureRedisCache.Services
{
    public interface IWrapperCacheService
    {
        Task SetAsync<T>(T data, string cacheKey);
        Task SetAsync<T>(T data, string cacheKey, DistributedCacheEntryOptions options);
        Task SetAsync<T>(T data, string cacheKey, Func<DistributedCacheEntryOptions> funcOption);
        Task<Result<T>> GetAsync<T>(string key);
        Task RemoveAsync(string key);
    }
    public class WrapperCacheService : IWrapperCacheService
    {
        readonly IDistributedCache _distributedCache;

        public WrapperCacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }
        public async Task SetAsync<T>(T data, string cacheKey)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var byteData = await ConvertData<T>.ObjectToByteArray(data);
            await _distributedCache.SetAsync(cacheKey, byteData);
        }
        public async Task SetAsync<T>(T data, string cacheKey, DistributedCacheEntryOptions options)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var byteData = await ConvertData<T>.ObjectToByteArray(data);
            await _distributedCache.SetAsync(cacheKey, byteData, options);
        }
        public async Task SetAsync<T>(T data, string cacheKey, Func<DistributedCacheEntryOptions> funcOption)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var options = funcOption();
            if (options == null)
            {
                options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(2));
            }
            var byteData = await ConvertData<T>.ObjectToByteArray(data);
            await _distributedCache.SetAsync(cacheKey, byteData, options);
        }
        public async Task<Result<T>> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            var response = await _distributedCache.GetAsync(key);
            if (response == null)
                return Result<T>.Failure("Not Found");

            var data = await ConvertData<T>.ByteArrayToObject(response);
            return Result<T>.Success(data);
        }
        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            await _distributedCache.RemoveAsync(key);
        }
    }
}
