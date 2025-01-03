using System;
using System.Collections.Generic;
using System.Text;

namespace AzureRedisCache.Models
{
    public static class ConnectionOptionConstants
    {
        public const string UseAccessKey = "5";
        public const string Default = "1";
        public const string UseUserManagedIdentity = "2";
        public const string UseSystemManagedIdentity = "3";
        public const string UseServicePrincipal = "4";
    }
}
