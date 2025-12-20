# Practical Azure SDK

A collection of .NET libraries and samples for working with Azure services, providing simplified wrappers and practical examples for Azure Blob Storage, Azure Table Storage, Azure Redis Cache, and OpenTelemetry integration.

## 📚 Libraries

This repository contains four main libraries:

### 1. AzureBlobStorage

A .NET Standard 2.1 library that provides a simplified interface for working with Azure Blob Storage.

**Features:**
- Upload files with block-based uploading
- Download blob content and files
- Delete blobs
- Update blob content
- Copy blobs across storage accounts
- Generate SAS tokens (both service SAS and user delegation SAS)

**NuGet Package:** `AzureBlobStorage` v1.0.1

### 2. AzureTableStorage

A .NET Standard 2.1 library that wraps Azure Table Storage operations with a simple, type-safe interface.

**Features:**
- Insert or update entities (upsert operations)
- Get single entities by partition key and row key
- Query entities with LINQ expressions
- Delete entities
- Batch operations for adding multiple entities
- Retrieve all entities with optional filtering

**NuGet Package:** `AzureTableStorage` v1.0.1

### 3. AzureRedisCache

A .NET Standard 2.1 library that provides a wrapper service for Azure Redis Cache with built-in serialization.

**Features:**
- Set and get cached data with automatic serialization/deserialization
- Support for custom cache expiration policies
- Distributed cache interface implementation
- Result pattern for cache operations

### 4. OpenTelemetryDistroExtension

A .NET 8.0 library that simplifies the integration of OpenTelemetry with Azure Monitor.

**Features:**
- Easy OpenTelemetry configuration with Azure Monitor
- Support for managed identity authentication
- Configurable sampling rates
- Live metrics support
- Custom resource attributes for service identification

## 🚀 Sample Applications

The repository includes several sample applications demonstrating how to use these libraries:

### ArticleManagementAPI

A .NET 8.0 Web API that demonstrates the integration of all three storage libraries (Blob Storage, Table Storage, and Redis Cache).

**Features:**
- RESTful API for article management
- Integration with Azure Table Storage for data persistence
- Redis caching for improved performance
- Blob Storage integration for file handling
- CORS configuration
- Swagger documentation

### BlobSasTokenGenerationAf

An Azure Function (v4) application that generates SAS tokens for blob storage access.

**Features:**
- HTTP-triggered Azure Function
- SAS token generation for secure blob access
- Application Insights integration

### ArticleManagementWebApp

A .NET 8.0 ASP.NET Core web application for managing articles.

### ArticleWebApp

A Vue.js web application providing a frontend interface for article management.

## 🛠️ Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code
- Azure subscription (for deploying and testing with actual Azure services)
- Node.js (for the Vue.js frontend application)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/GiangHM/PracticalAzureSDK.git
cd PracticalAzureSDK
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

## 📖 Usage Examples

### Using AzureBlobStorage

```csharp
// In your Program.cs or Startup.cs
services.AddAzureBlobStorage();

// Inject and use the service
public class MyService
{
    private readonly IBlobStorageService _blobService;
    
    public MyService(IBlobStorageService blobService)
    {
        _blobService = blobService;
    }
    
    public async Task<Uri> GenerateSasTokenAsync(string containerName, string fileName)
    {
        return await _blobService.CreateUserDelegationSasAsync(
            containerName, 
            fileName, 
            expireMinute: 60);
    }
}
```

### Using AzureTableStorage

```csharp
// In your Program.cs or Startup.cs
services.AddAzureTableStorage();

// Create a service that inherits from TableStorageServiceBase
public class TopicTableService : TableStorageServiceBase<TopicEntity>
{
    public TopicTableService(
        IAzureClientFactory<TableServiceClient> azureClientFactory,
        IOptions<TableStorageOption> options) 
        : base(azureClientFactory, options)
    {
    }
    
    public override string GetTableName() => "Topics";
}

// Use the service
var entity = await topicService.GetEntityAsync(partitionKey, rowKey);
await topicService.InsertOrUpadteEntityAsync(newEntity);
```

### Using AzureRedisCache

```csharp
// In your Program.cs or Startup.cs
services.AddAzureRedisWithOptions();

// Inject and use the service
public class MyService
{
    private readonly IWrapperCacheService _cacheService;
    
    public MyService(IWrapperCacheService cacheService)
    {
        _cacheService = cacheService;
    }
    
    public async Task CacheDataAsync<T>(string key, T data)
    {
        await _cacheService.SetAsync(data, key);
    }
    
    public async Task<Result<T>> GetCachedDataAsync<T>(string key)
    {
        return await _cacheService.GetAsync<T>(key);
    }
}
```

### Using OpenTelemetryDistroExtension

```csharp
// In your Program.cs
services.AddOpenTelemetryDistro(options =>
{
    options.AIRoleName = "MyService";
    options.ClientId = "your-managed-identity-client-id";
    options.IsUseSampling = true;
    options.FixSampling = 0.1f; // 10% sampling
    options.LiveMetric = true;
});
```

## ⚙️ Configuration

Each library requires specific configuration settings. Here's an example `appsettings.json`:

```json
{
  "AzureBlobStorage": {
    "ServiceName": "BlobStorageClient",
    "ConnectionString": "your-connection-string-or-endpoint"
  },
  "AzureTableStorage": {
    "ServiceName": "TableStorageClient",
    "ConnectionString": "your-connection-string-or-endpoint"
  },
  "RedisOptions": {
    "ConnectionString": "your-redis-connection-string",
    "InstanceName": "MyApp_"
  },
  "AIMonitor": {
    "AIRoleName": "MyService",
    "ClientId": "your-managed-identity-client-id",
    "IsUseSampling": true,
    "FixSampling": 0.1,
    "LiveMetric": true
  }
}
```

## 🏗️ Project Structure

```
PracticalAzureSDK/
├── AzureBlobStorage/          # Blob Storage library
├── AzureTableStorage/         # Table Storage library
├── AzureRedisCache/           # Redis Cache library
├── OpenTelemetryDistroExtension/ # OpenTelemetry integration
├── Samples/
│   ├── ArticleManagementAPI/  # Sample Web API
│   ├── ArticleManagementWebApp/ # Sample ASP.NET Core app
│   ├── ArticleWebApp/         # Sample Vue.js app
│   └── BlobSasTokenGenerationAf/ # Sample Azure Function
└── PracticalAzureSDKs.sln     # Solution file
```

## 📝 License

This project is licensed under the MIT License.

## 👤 Author

**GiangHM**

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!

## 🔗 Related Resources

- [Azure Blob Storage Documentation](https://docs.microsoft.com/en-us/azure/storage/blobs/)
- [Azure Table Storage Documentation](https://docs.microsoft.com/en-us/azure/storage/tables/)
- [Azure Redis Cache Documentation](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/)
- [OpenTelemetry Documentation](https://opentelemetry.io/)
- [Azure Monitor OpenTelemetry](https://docs.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-overview)
