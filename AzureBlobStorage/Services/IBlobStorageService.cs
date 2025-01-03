using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobStorage.Services
{
    public interface IBlobStorageService
    {
        Task UploadBlocksAsync(string fileName, string prefixFileName, FileStream fileStream, int blockSize, string fileExtension);
        Task UploadBlobWithTransferOption(string prefixFileName, string content, string fileExtension);
        Task DownloadBlobWithTransferOptionsAsync(string url, string localFilePath);
        Task<string> DownloadContentAsync(string url);
        Task DeleteAsync(string url);
        Task UpdateContentAsync(string url, string content);
        Task<string> CopyAcrossStorageAccountsFromUrlAsync(string topicName, string prefixFileName, string sourceUrl, string extension);
        public Uri CreateServiceSASBlob(string containerName
            , string fileName
            , int expireMinute = 1
            , string storedPolicyName = null
            , BlobContainerSasPermissions permission = BlobContainerSasPermissions.Read);
        Uri CreateServiceSASContainer(string containerName, int expireMinute = 1, BlobContainerSasPermissions permission = BlobContainerSasPermissions.Read);
        Task<Uri> CreateUserDelegationSasAsync(string containerName
            , string fileName
            , int expireMinute = 1
            , string storedPolicyName = null
            , BlobContainerSasPermissions permission = BlobContainerSasPermissions.Read);
    }
}
