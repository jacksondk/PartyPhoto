using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PhotoFunctions
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

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
            _logger.LogInformation($"Received file: {file.FileName}, Size: {file.Length} bytes");
            
            // Read file bytes
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                
                // Now you have the file bytes in the fileBytes variable
                // You can process them or save them as needed
                _logger.LogInformation($"Successfully read {fileBytes.Length} bytes");
                
                // Return success response
                return new OkObjectResult(new { message = $"File uploaded successfully - {fileBytes.Length}", fileName = file.FileName, size = fileBytes.Length });
            }
        }
    }
}
