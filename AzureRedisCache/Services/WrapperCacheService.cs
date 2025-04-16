using AzureRedisCache.Helpers;
using AzureRedisCache.Models;
using AzureRedisCache.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureRedisCache.Services
{
    public interface IWrapperCacheService
    {
        Task SetAsync<T>(string cacheKey, T data);
        Task SetAsync<T>(string cacheKey, T data, DistributedCacheEntryOptions options);
        Task SetAsync<T>(string cacheKey, T data, Func<DistributedCacheEntryOptions> funcOption);
        Task SetAsync<T>(Dictionary<string, T> data, Func<DistributedCacheEntryOptions> funcOption);
        Task<Result<T>> GetAsync<T>(string key);
        Task<Result<Dictionary<string, T>>> GetAsync<T>(IReadOnlyList<string> keys);
        Task RemoveAsync(string key);
    }
    public class WrapperCacheService : IWrapperCacheService
    {
        readonly ILogger<WrapperCacheService> _logger;
        private const long NotPresent = -1;
        private const string AbsoluteExpirationKey = "absexp";
        private const string SlidingExpirationKey = "sldexp";
        private const string DataKey = "data";
        private static readonly RedisValue[] _hashMembersAbsoluteExpirationSlidingExpirationData
            = new RedisValue[] { AbsoluteExpirationKey, SlidingExpirationKey, DataKey };
        private static readonly RedisValue[] _hashMembersAbsoluteExpirationSlidingExpiration
            = new RedisValue[] { AbsoluteExpirationKey, SlidingExpirationKey };

        private volatile IDatabase? _cache;
        private bool _disposed;

        private readonly RedisCacheOptions _options;

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private long _lastConnectTicks = DateTimeOffset.UtcNow.Ticks;

        public WrapperCacheService(IOptions<RedisCacheOptions> optionsAccessor
            , ILogger<WrapperCacheService> logger)
        {
            _options = optionsAccessor.Value;
            _logger = logger;
        }

        public async Task SetAsync<T>(string cacheKey, T data)
        {
            _logger.LogInformation("Set cache with key: {0}", cacheKey);
            var byteData = await ConvertData<T>.ObjectToByteArray(data);
            await SetAsync(cacheKey, byteData);
        }
        public async Task SetAsync<T>(string cacheKey, T data, DistributedCacheEntryOptions options)
        {
            _logger.LogInformation("Set cache with key: {0}", cacheKey);
            var byteData = await ConvertData<T>.ObjectToByteArray(data);
            if (options == null)
            {
                _logger.LogWarning("Input cache option is null, using default setting with sliding expiration in 3h");
                options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
            }
            await SetImplAsync(cacheKey, new ReadOnlySequence<byte>(byteData), options);
        }
        public async Task SetAsync<T>(string cacheKey, T data, Func<DistributedCacheEntryOptions> funcOption)
        {
            var options = funcOption();
            if (options == null)
            {
                _logger.LogWarning("Input cache option is null, using default setting with sliding expiration in 3h");
                options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
            }
            var byteData = await ConvertData<T>.ObjectToByteArray(data);
            await SetImplAsync(cacheKey, new ReadOnlySequence<byte>(byteData), options);

        }
        public async Task SetAsync<T>(Dictionary<string, T> data, Func<DistributedCacheEntryOptions> funcOption)
        {

            var setTasks = new List<Task>();

            var cache = await ConnectAsync().ConfigureAwait(false);

            IBatch batch = cache.CreateBatch();

            foreach (var item in data)
            {
                var creationTime = DateTimeOffset.UtcNow;

                var options = funcOption();

                var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);
                var ttl = GetExpirationInSeconds(creationTime, absoluteExpiration, options);
                var redisKey = new RedisKey(item.Key);
                var byteData = await ConvertData<T>.ObjectToByteArray(item.Value);
                var redisValue = GetRedisValue(new ReadOnlySequence<byte>(byteData), out var _);

                if (ttl is null)
                {
                    setTasks.Add(batch.StringSetAsync(redisKey, redisValue));
                }
                else
                {
                    setTasks.AddRange(new List<Task>
                    {
                        batch.StringSetAsync(redisKey, redisValue, TimeSpan.FromSeconds(ttl.Value)),
                    });
                }
            }
            batch.Execute();
            await Task.WhenAll(setTasks.ToArray()).ConfigureAwait(false);

        }
        public async Task<Result<T>> GetAsync<T>(string key)
        {
            _logger.LogInformation("Get cache value with key: {0}", key);
            var response = await GetAndRefreshAsync(key, true);
            if (response == null)
                return Result<T>.Failure("Not Found");

            var data = await ConvertData<T>.ByteArrayToObject(response);
            return Result<T>.Success(data);
        }
        public async Task<Result<Dictionary<string, T>>> GetAsync<T>(IReadOnlyList<string> keys)
        {

            var cache = await ConnectAsync().ConfigureAwait(false);
            var redisKeys = keys.Select(x => new RedisKey(x)).ToArray();
            var result = await cache.StringGetAsync(redisKeys);
            var response = keys.Zip(result, async (k, v) =>
            {
                var convertedValue = v.Box() != null ? await ConvertData<T>.ByteArrayToObject(v) : default(T);
                return new KeyValuePair<string, T>(k, convertedValue);
            });

            var data = ToDictionary(await Task.WhenAll(response));

            if (data == null || !data.Any())
                return Result<Dictionary<string, T>>.Failure("Not Found");

            return Result<Dictionary<string, T>>.Success(data);

        }
        public async Task RemoveAsync(string key)
        {
            _logger.LogInformation("Remove cache with key: {0}", key);
            var cache = await ConnectAsync().ConfigureAwait(false);
            Debug.Assert(cache != null);

            try
            {
                await cache.KeyDeleteAsync(new RedisKey(key)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Remove error for key {0}", key);
                throw;
            }
        }

        #region Private helpers for Get/Set cache
        private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(DistributedCacheEntryOptions.AbsoluteExpiration),
                    options.AbsoluteExpiration.Value,
                    "The absolute expiration value must be in the future.");
            }

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                return creationTime + options.AbsoluteExpirationRelativeToNow;
            }

            return options.AbsoluteExpiration;
        }
        private static long? GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration, DistributedCacheEntryOptions options)
        {
            if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
            {
                return (long)Math.Min(
                    (absoluteExpiration.Value - creationTime).TotalSeconds,
                    options.SlidingExpiration.Value.TotalSeconds);
            }
            else if (absoluteExpiration.HasValue)
            {
                return (long)(absoluteExpiration.Value - creationTime).TotalSeconds;
            }
            else if (options.SlidingExpiration.HasValue)
            {
                return (long)options.SlidingExpiration.Value.TotalSeconds;
            }
            return null;
        }
        private static ReadOnlyMemory<byte> GetRedisValue(in ReadOnlySequence<byte> value, out byte[]? lease)
        {
            // RedisValue only supports single-segment chunks; this will almost never be an issue, but
            // on those rare occasions: use a leased array to harmonize things
            if (value.IsSingleSegment)
            {
                lease = null;
                return value.First;
            }
            var length = checked((int)value.Length);
            lease = ArrayPool<byte>.Shared.Rent(length);
            value.CopyTo(lease);
            return new ReadOnlyMemory<byte>(lease, 0, length);
        }
       
        private static Dictionary<string, T> ToDictionary<T>(KeyValuePair<string, T>[] data)
        {
            var response = new Dictionary<string, T>();
            foreach (var item in data.Where(item => !string.IsNullOrEmpty(item.Key) && item.Value != null))
            {
                response[item.Key] = item.Value;
            }

            return response;
        }
        #endregion
        #region Private Get and Set Cache
        private async Task<byte[]?> GetAndRefreshAsync(string key, bool getData, CancellationToken token = default)
        {

            token.ThrowIfCancellationRequested();

            var cache = await ConnectAsync(token).ConfigureAwait(false);
            Debug.Assert(cache != null);

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            RedisValue result;
            try
            {
                result = await cache.StringGetAsync(new RedisKey(key)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get error for key {0}", key);
                throw;
            }

            return result;
        }
        
        private async Task SetImplAsync(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var cache = await ConnectAsync(token).ConfigureAwait(false);
            Debug.Assert(cache != null);

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            try
            {
                var prefixedKey = new RedisKey(key);
                var ttl = GetExpirationInSeconds(creationTime, absoluteExpiration, options);
                var redisValue = GetRedisValue(value, out var lease);

                if (ttl is null)
                {
                    await cache.StringSetAsync(prefixedKey, redisValue).ConfigureAwait(false);
                }
                else
                {
                    await cache.StringSetAsync(prefixedKey, redisValue, TimeSpan.FromSeconds(ttl.Value)).ConfigureAwait(false);
                }
                Recycle(lease);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Set error for key {0}", key);

                throw;
            }
        }
        #endregion
        #region Connection and Dispose
        private ValueTask<IDatabase> ConnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var cache = _cache;
            if (cache != null)
            {
                Debug.Assert(_cache != null);
                return new ValueTask<IDatabase>(cache);
            }
            return ConnectSlowAsync(token);
        }
        private async ValueTask<IDatabase> ConnectSlowAsync(CancellationToken token)
        {
            await _connectionLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var cache = _cache;
                if (cache is null)
                {
                    IConnectionMultiplexer connection;
                    connection = await _options.ConnectionMultiplexerFactory().ConfigureAwait(false);

                    PrepareConnection(connection);
                    cache = _cache = connection.GetDatabase();
                }
                Debug.Assert(_cache != null);
                return cache;
            }
            finally
            {
                _connectionLock.Release();
            }
        }
        private void PrepareConnection(IConnectionMultiplexer connection)
        {
            WriteTimeTicks(ref _lastConnectTicks, DateTimeOffset.UtcNow);
            TryRegisterProfiler(connection);
        }
        private static void WriteTimeTicks(ref long field, DateTimeOffset value)
        {
            var ticks = value == DateTimeOffset.MinValue ? 0L : value.UtcTicks;
            Volatile.Write(ref field, ticks); // avoid torn values
        }
        private void TryRegisterProfiler(IConnectionMultiplexer connection)
        {
            _ = connection ?? throw new InvalidOperationException($"{nameof(connection)} cannot be null.");

            if (_options.ProfilingSession != null)
            {
                connection.RegisterProfiler(_options.ProfilingSession);
            }
        }
        static void ReleaseConnection(IDatabase? cache)
        {
            var connection = cache?.Multiplexer;
            if (connection != null)
            {
                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            ReleaseConnection(Interlocked.Exchange(ref _cache, null));
        }
        private static void Recycle(byte[]? lease)
        {
            if (lease != null)
            {
                ArrayPool<byte>.Shared.Return(lease);
            }
        }
        #endregion
    }
}
