using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using ST10296771_CLDV7311_POE.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ST10296771_CLDV7311_POE.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            _logger = logger;

            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString("AzureStorage");
            var containerName = configuration["AzureStorage:ContainerName"] ?? "eventimages";

            // Create blob service client
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Create container if it doesn't exist
            _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob).Wait();
        }

        public async Task<string> UploadImageAsync(IFormFile file, string fileName)
        {
            try
            {
                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

                // Get blob reference
                var blobClient = _containerClient.GetBlobClient(uniqueFileName);

                // Upload file
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    });
                }

                _logger.LogInformation($"Image uploaded: {uniqueFileName}");
                return uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading image: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return true;

                var blobClient = _containerClient.GetBlobClient(fileName);
                var response = await blobClient.DeleteIfExistsAsync();

                _logger.LogInformation($"Image deleted: {fileName}, Success: {response.Value}");
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting image: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetImageUrlAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            var blobClient = _containerClient.GetBlobClient(fileName);

            // Check if blob exists
            if (await blobClient.ExistsAsync())
            {
                return blobClient.Uri.ToString();
            }

            return null;
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check file size
            if (file.Length > AzureStorageConfig.MaxFileSize)
                return false;

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AzureStorageConfig.AllowedImageTypes.Contains(extension))
                return false;

            // Check content type
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }
    }
}