using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobStorage.Helpers
{
    public static class FilenameHelper
    {
        private static async Task<string> CreateContainerIfNotExists(BlobServiceClient blobServiceClient, string containerNamePrefix, string containerNameSuffix)
        {
            if (string.IsNullOrEmpty(containerNameSuffix))
            {
                //if no suffix provided, date is used
                containerNameSuffix = DateTime.UtcNow.ToString("yyyy-MM-dd");
            }
            if (!string.IsNullOrWhiteSpace(containerNamePrefix))
            {
                containerNameSuffix = $"{containerNamePrefix}{containerNameSuffix}";
            }
            var containerClient = blobServiceClient.GetBlobContainerClient(containerNameSuffix);
            await containerClient.CreateIfNotExistsAsync();

            return containerNameSuffix;
        }


        public static async Task<(string, string)> GenerateNames(BlobServiceClient blobServiceClient
            , string containerNamePrefix
            , string containerNameSuffix
            , string prefixFileName
            , string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) extension = string.Empty;
            //ensure that prefixFileName is not null
            if (string.IsNullOrEmpty(prefixFileName)) prefixFileName = string.Empty;

            var fullContainerName = await CreateContainerIfNotExists(blobServiceClient, containerNamePrefix, containerNameSuffix);
            var blobName = $"{prefixFileName}_{Guid.NewGuid()}{extension}";
            return (fullContainerName, blobName);
        }
    }
}
