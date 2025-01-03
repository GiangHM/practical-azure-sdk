using AzureRedisCache.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureRedisCache.Settings
{
    internal sealed class ValidateRedisOptions : IValidateOptions<RedisOptions>
    {
        public ValidateRedisOptions (IConfiguration configuration)
        {
            options = configuration.GetSection("AzureRedisCache")
              .Get<RedisOptions>();
        }

        public RedisOptions? options {get; private set;} 
        public ValidateOptionsResult Validate(string? name, RedisOptions options)
        {
            StringBuilder? failure = null;
            if (options == null)
                (failure ??= new StringBuilder()).AppendLine($"Redis configuration cannot be null");
            if (options?.ConnectionMode == ConnectionOptionConstants.UseAccessKey
                &&  string.IsNullOrEmpty(options.ConnectionString))
                (failure ??= new StringBuilder()).AppendLine($"Connection string cannot be null");
            if (options?.ConnectionMode == ConnectionOptionConstants.UseUserManagedIdentity
                && string.IsNullOrEmpty(options.UserManagedIdentityId))
                (failure ??= new StringBuilder()).AppendLine($"Connection string cannot be null");
            if (options?.ConnectionMode == ConnectionOptionConstants.UseServicePrincipal
                && (string.IsNullOrEmpty(options.ClientId) || string.IsNullOrEmpty(options.Secret) || string.IsNullOrEmpty(options.TenantId)))
                (failure ??= new StringBuilder()).AppendLine($"Connection string cannot be null");

            return !string.IsNullOrEmpty(failure?.ToString()) ?
                ValidateOptionsResult.Fail(failure.ToString())
                : ValidateOptionsResult.Success;
        }
    }
}
