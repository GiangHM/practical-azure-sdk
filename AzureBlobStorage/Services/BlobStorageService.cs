using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using AzureBlobStorage.Helpers;
using AzureBlobStorage.Settings;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace AzureBlobStorage.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _privateStorageWithRetry;
        private readonly ILogger _logger;
        private readonly IOptions<StorageOptions> _options;
        public BlobStorageService(ILogger<IBlobStorageService> logger
            , IAzureClientFactory<BlobServiceClient> clientFactory
            , IOptions<StorageOptions> options)
        {
            _privateStorageWithRetry = clientFactory.CreateClient(options.Value.StorageName ?? "AzureBlobStorage");
            _logger = logger;
            _options = options;
        }

        /// <summary>
        /// You can use this method to enhance performance by uploading blocks in parallel.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="prefixFileName"></param>
        /// <param name="fileStream"></param>
        /// <param name="blockSize"></param>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public async Task UploadBlocksAsync(string fileName, string prefixFileName, FileStream fileStream, int blockSize, string fileExtension)
        {
            try
            {

                var options = _options.Value;
                var (fullContainerName, blobName) = await FilenameHelper.GenerateNames(_privateStorageWithRetry, options.Container, "test-client-provide-folder", prefixFileName, fileExtension);

                var blobContainerClient = _privateStorageWithRetry.GetBlobContainerClient(fullContainerName);
                BlockBlobClient blobClient = blobContainerClient.GetBlockBlobClient(blobName);

                ArrayList blockIDArrayList = new ArrayList();
                byte[] buffer;

                var bytesLeft = (fileStream.Length - fileStream.Position);

                while (bytesLeft > 0)
                {
                    if (bytesLeft >= blockSize)
                    {
                        buffer = new byte[blockSize];
                        await fileStream.ReadAsync(buffer, 0, blockSize);
                    }
                    else
                    {
                        buffer = new byte[bytesLeft];
                        await fileStream.ReadAsync(buffer, 0, Convert.ToInt32(bytesLeft));
                        bytesLeft = (fileStream.Length - fileStream.Position);
                    }

                    using (var stream = new MemoryStream(buffer))
                    {
                        string blockID = Convert.ToBase64String(
                            Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

                        blockIDArrayList.Add(blockID);
                        await blobClient.StageBlockAsync(blockID, stream);
                    }
                    bytesLeft = (fileStream.Length - fileStream.Position);
                }

                string[] blockIDArray = (string[])blockIDArrayList.ToArray(typeof(string));

                await blobClient.CommitBlockListAsync(blockIDArray);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while uploading {fileName}");
                throw;
            }
        }

        /// <summary>
        /// Upload with transfer option can enhance performance
        /// </summary>
        /// <param name="prefixFileName"></param>
        /// <param name="content"></param>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public async Task UploadBlobWithTransferOption(string prefixFileName, string content, string fileExtension)
        {
            var options = _options.Value;
            var (fullContainerName, blobName) = await FilenameHelper.GenerateNames(_privateStorageWithRetry, options.Container, "test-client-provide-folder", prefixFileName, fileExtension);

            var blobContainerClient = _privateStorageWithRetry.GetBlobContainerClient(fullContainerName);
            BlockBlobClient blobClient = blobContainerClient.GetBlockBlobClient(blobName);

            // Performance tuning by using StorageTransferOptions 
            var transferOptions = new StorageTransferOptions
            {
                // Set the maximum number of parallel transfer workers
                // The effectiveness of this value is subject to connection pool limits in .NET
                // See: https://devblogs.microsoft.com/azure-sdk/net-framework-connection-pool-limits/
                MaximumConcurrency = 2,

                // Set the initial transfer length to 8 MiB
                // only applies for uploads when using a seekable stream
                // non-seekable stream is ignore
                InitialTransferSize = 8 * 1024 * 1024,

                // Set the maximum length of a transfer to 4 MiB
                MaximumTransferSize = 4 * 1024 * 1024
            };

            // You can specify transfer validation options to help ensure that data is uploaded properly
            // and hasn't been tampered with during transit
            var validationOptions = new UploadTransferValidationOptions
            {
                // Recommended
                ChecksumAlgorithm = StorageChecksumAlgorithm.Auto
            };

            BlobUploadOptions uploadOption = new BlobUploadOptions
            {
                TransferOptions = transferOptions,
                TransferValidation = validationOptions,
            };

            // create stream
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            // Upload from stream to use InitialTransferSize option
            await blobClient.UploadAsync(stream, uploadOption);

        }

        /// <summary>
        /// Download blob to local file with transfer option
        /// </summary>
        /// <param name="blobClient"></param>
        /// <returns></returns>
        public async Task DownloadBlobWithTransferOptionsAsync(string url, string localFilePath)
        {
            FileStream fileStream = File.OpenWrite(localFilePath);

            var transferOptions = new StorageTransferOptions
            {
                // Set the maximum number of parallel transfer workers
                MaximumConcurrency = 2,

                // Set the initial transfer length to 8 MiB
                InitialTransferSize = 8 * 1024 * 1024,

                // Set the maximum length of a transfer to 4 MiB
                MaximumTransferSize = 4 * 1024 * 1024
            };

            BlobDownloadToOptions downloadOptions = new BlobDownloadToOptions()
            {
                TransferOptions = transferOptions
            };

            BlobUriBuilder builder = new BlobUriBuilder(new Uri(url));
            var containerClient = _privateStorageWithRetry.GetBlobContainerClient(builder.BlobContainerName);
            var blobClient = containerClient.GetBlobClient(builder.BlobName);
            await blobClient.DownloadToAsync(fileStream, downloadOptions);

            fileStream.Close();
        }

        /// <summary>
        /// Download blob content
        /// </summary>
        /// <param name="url"></param>
        /// <returns>string content</returns>
        public async Task<string> DownloadContentAsync(string url)
        {
            BlobUriBuilder builder = new BlobUriBuilder(new Uri(url));
            var containerClient = _privateStorageWithRetry.GetBlobContainerClient(builder.BlobContainerName);
            var blobClient = containerClient.GetBlobClient(builder.BlobName);
            var response = await blobClient.DownloadContentAsync();
            return response.Value.Content.ToString();
        }

        /// <summary>
        /// Delete blob by provided url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task DeleteAsync(string url)
        {
            BlobClient blobClient = GetBlobClientFromUrl(url);
            await blobClient.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Update content of blob file
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task UpdateContentAsync(string url, string content)
        {
            BlobClient blobClient = GetBlobClientFromUrl(url);
            await blobClient.UploadAsync(new BinaryData(content), true);
        }

        /// <summary>
        /// Copy blob file from url to another storage account
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="prefixFileName"></param>
        /// <param name="sourceUrl"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string> CopyAcrossStorageAccountsFromUrlAsync(string topicName, string prefixFileName, string sourceUrl, string extension)
        {
            if (string.IsNullOrWhiteSpace(sourceUrl)) throw new ArgumentException($"{nameof(sourceUrl)} cannot be null or empty", nameof(sourceUrl));
            var options = _options.Value;
            var (fullContainerName, blobName) = await FilenameHelper.GenerateNames(_privateStorageWithRetry, options.Container, topicName, prefixFileName, extension);

            var containerClient = _privateStorageWithRetry.GetBlobContainerClient(fullContainerName);
            var dentinationBlobClient = containerClient.GetBlobClient(blobName);

            var sourceBlobClient = GetBlobClientFromUrl(sourceUrl);
            BlobLeaseClient sourceBlobLease = new BlobLeaseClient(sourceBlobClient);
            try
            {
                await sourceBlobLease.AcquireAsync(BlobLeaseClient.InfiniteLeaseDuration);

                var copyOptions = new BlobCopyFromUriOptions();
                CopyFromUriOperation copyOperation = await dentinationBlobClient.StartCopyFromUriAsync(new Uri(sourceUrl), copyOptions).ConfigureAwait(false);
                await copyOperation.WaitForCompletionAsync();

                return dentinationBlobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while uploading {sourceUrl}");
                throw;
            }
        }

        /// <summary>
        /// Generate SAS for blob
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <param name="expireMinute"></param>
        /// <param name="storedPolicyName"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public Uri CreateServiceSASBlob(string containerName
            , string fileName
            , int expireMinute = 1
            , string storedPolicyName = null
            , BlobContainerSasPermissions permission = BlobContainerSasPermissions.Read)
        {
            var blobContainerClient = _privateStorageWithRetry.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(fileName);
            // Check if BlobContainerClient object has been authorized with Shared Key
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one minute
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expireMinute);
                    sasBuilder.SetPermissions(permission);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasURI = blobClient.GenerateSasUri(sasBuilder);

                return sasURI;
            }
            else
            {
                // Client object is not authorized via Shared Key
                return null;
            }
        }

        /// <summary>
        /// Generate SAS for container
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="expireMinute"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public Uri CreateServiceSASContainer(string containerName, int expireMinute = 1, BlobContainerSasPermissions permission = BlobContainerSasPermissions.Read)
        {
            BlobSasBuilder blobSasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                Resource = "c",
                ExpiresOn = DateTime.Now.AddMinutes(expireMinute),
            };

            blobSasBuilder.SetPermissions(permission);

            var blobContainerClient = _privateStorageWithRetry.GetBlobContainerClient(containerName);
            var sasUri = blobContainerClient.GenerateSasUri(blobSasBuilder);

            return sasUri;
        }

        /// <summary>
        /// Create user delegation sas token for blob
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <param name="expireMinute"></param>
        /// <param name="storedPolicyName"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public async Task<Uri> CreateUserDelegationSasAsync(string containerName
            , string fileName
            , int expireMinute = 1
            , string storedPolicyName = null
            , BlobContainerSasPermissions permission = BlobContainerSasPermissions.Read)
        {
            try
            {

                UserDelegationKey userDelegationKey = await _privateStorageWithRetry.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow
                    , DateTimeOffset.UtcNow.AddDays(1));

                var blobContainerClient = _privateStorageWithRetry.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(fileName);
                // Create a SAS token that's valid for one minute
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expireMinute);
                    sasBuilder.SetPermissions(permission);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }
                var accountName = blobClient.GetParentBlobContainerClient().GetParentBlobServiceClient().AccountName;
                var sas = sasBuilder.ToSasQueryParameters(userDelegationKey, accountName);
                BlobUriBuilder uriBuidler = new BlobUriBuilder(blobClient.Uri, trimBlobNameSlashes: true)
                {
                    Sas = sas
                };

                return uriBuidler.ToUri();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        private BlobClient GetBlobClientFromUrl(string url)
        {
            BlobUriBuilder builder = new BlobUriBuilder(new Uri(url));
            var containerClient = _privateStorageWithRetry.GetBlobContainerClient(builder.BlobContainerName);
            var blobClient = containerClient.GetBlobClient(builder.BlobName);
            return blobClient;
        }
    }
}
