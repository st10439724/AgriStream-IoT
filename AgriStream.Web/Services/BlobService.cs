using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AgriStream.Web.Services
{
    public record BlobInfo(string Name, long? Size, DateTimeOffset? Modified);

    public class BlobService
    {
        private readonly BlobContainerClient _container;

        public BlobService(IConfiguration config)
        {
            var conn = config["StorageConnection"]
                ?? throw new InvalidOperationException("StorageConnection is not configured.");
            _container = new BlobContainerClient(conn, "drone-images");
        }

        public async Task UploadAsync(Stream content, string blobName, string contentType)
        {
            await _container.CreateIfNotExistsAsync();
            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };
            await _container.GetBlobClient(blobName).UploadAsync(content, options);
        }

        public async Task<List<BlobInfo>> ListAsync()
        {
            var list = new List<BlobInfo>();
            await foreach (var item in _container.GetBlobsAsync())
            {
                list.Add(new BlobInfo(item.Name, item.Properties.ContentLength, item.Properties.LastModified));
            }
            return list.OrderByDescending(b => b.Modified).ToList();
        }
    }
}