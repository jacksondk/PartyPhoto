using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api
{
    public class UploadFunction
    {
        private readonly ILogger<UploadFunction> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private readonly int _maxFileSizeInMb = 10; // 10MB file size limit

        public UploadFunction(ILogger<UploadFunction> logger)
        {
            _logger = logger;
        }

        [Function("Upload")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed an image upload request.");
            
            try
            {
                // Check if there are any files in the request
                if (req.Form.Files.Count == 0)
                {
                    return new BadRequestObjectResult(new { error = "No files were uploaded." });
                }

                var file = req.Form.Files[0];
                if (file.Length == 0)
                {
                    return new BadRequestObjectResult(new { error = "File is empty." });
                }

                // Validate file size
                if (file.Length > _maxFileSizeInMb * 1024 * 1024)
                {
                    return new BadRequestObjectResult(new { 
                        error = $"File is too large. Maximum file size is {_maxFileSizeInMb}MB." 
                    });
                }

                // Validate file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    return new BadRequestObjectResult(new { 
                        error = $"Invalid file type. Allowed types are: {string.Join(", ", _allowedExtensions)}" 
                    });
                }

                // Process the uploaded file
                // In a real-world scenario, you would save this to blob storage
                // For this example, we'll just return confirmation of the upload
                
                // Generate a unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                
                _logger.LogInformation($"File received: {file.FileName}, Size: {file.Length} bytes, Saved as: {uniqueFileName}");

                return new OkObjectResult(new { 
                    message = "File uploaded successfully",
                    fileName = file.FileName,
                    fileSize = file.Length,
                    savedAs = uniqueFileName,
                    timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded file");
                return new ObjectResult(new { error = "An internal server error occurred." })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
