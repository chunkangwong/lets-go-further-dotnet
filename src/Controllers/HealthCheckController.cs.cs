using Asp.Versioning;
using controller_api_test.src.Models;
using Microsoft.AspNetCore.Mvc;

namespace controller_api_test.src.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("v{version:apiVersion}/[controller]")]
public class HealthCheckController : ControllerBase
{
    [HttpGet(Name = "GetHealthCheck")]
    public HealthCheckResponse Get()
    {
        return new HealthCheckResponse
        {
            Status = "available",
            SystemInfo = new SystemInfoResponse
            {
                Environment = "development",
                Version = "0.0.0"
            }
        };
    }
}
