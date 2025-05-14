using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;

namespace Api
{
    public class HealthFunction
    {
        private readonly ILogger<HealthFunction> _logger;

        public HealthFunction(ILogger<HealthFunction> logger)
        {
            _logger = logger;
        }

        [Function("Health")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("Health check endpoint called");
            
            return new OkObjectResult(new { 
                status = "healthy",
                timestamp = DateTimeOffset.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
