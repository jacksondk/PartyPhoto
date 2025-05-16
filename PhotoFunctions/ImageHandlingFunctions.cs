using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace PhotoFunctions
{
    public class ImageHandlingFunctions(ILogger<ImageHandlingFunctions> logger)
    {
        private readonly ILogger<ImageHandlingFunctions> _logger = logger;

        [Function("Upload")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            // Check if content type is multipart/form-data
            if (!req.HasFormContentType)
            {
                return new BadRequestObjectResult("Form content is required.");
            }

            var form = await req.ReadFormAsync();
            var file = form.Files["file"];

            if (file == null)
            {
                return new BadRequestObjectResult("File is required.");
            }

            // Get file info
            _logger.LogInformation("Received file: {file.FileName}, Size: {file.Length} bytes", file.FileName, file.Length);

            var filenameExtension = Path.GetExtension(file.FileName);
            var filename = string.Format("{0:yyyyMMddHHmmss}_{1}", DateTime.UtcNow, filenameExtension);
            var blogServiceUrl = Environment.GetEnvironmentVariable("BlobServiceUrl");
            var sasToken = Environment.GetEnvironmentVariable("BlobSasToken");
            var blobServiceClient = new BlobServiceClient(new Uri($"{blogServiceUrl}?{sasToken}"), null);
            var containerClient = blobServiceClient.GetBlobContainerClient("photos");
            await containerClient.UploadBlobAsync(filename, file.OpenReadStream());
            
            // Return success response
            return new OkObjectResult(new { message = $"File uploaded successfully" });

        }

        [Function("GetRandomImage")]
        public async Task<IActionResult> GetRandomImage([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var blogServiceUrl = Environment.GetEnvironmentVariable("BlobServiceUrl");
            var sasToken = Environment.GetEnvironmentVariable("BlobSasToken");
            _logger.LogInformation("BlobServiceUrl: {url}, BlobSasToken: {token}", blogServiceUrl, sasToken);
            var blobServiceClient = new BlobServiceClient(new Uri($"{blogServiceUrl}?{sasToken}"), null);
            var containerClient = blobServiceClient.GetBlobContainerClient("photos");

            // Get a random image from the container
            // List all blobs in the container
            var blobItems = new List<BlobItem>();
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                blobItems.Add(blobItem);
            }
            
            var random = new Random();
            var blobList = blobItems.ToList();
            _logger.LogDebug("Blob list count: {count}", blobList.Count);
            if (blobList.Count == 0)
            {
                return new NotFoundObjectResult("No images found.");
            }
            var randomBlob = blobList[random.Next(blobList.Count)];

            // Generate a URL for the blob
            var blobClient = containerClient.GetBlobClient(randomBlob.Name);
            var stream = await blobClient.OpenReadAsync();

            // Return the URL of the random image
            _logger.LogDebug("Blob type {count}", randomBlob.Properties.ContentType);
            return new FileStreamResult(stream, randomBlob.Properties.ContentType)
            {
                FileDownloadName = randomBlob.Name
            };
        }
    }
}
