using Microsoft.AspNetCore.Mvc;

namespace Wcs.WebApi.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("summary")]
    public IActionResult GetSummary()
    {
        return Ok(new
        {
            service = "Wcs.WebApi",
            targetFramework = "net8.0",
            migrationStage = "backend-foundation",
            hubs = new[]
            {
                "/hubs/device",
                "/hubs/task",
                "/hubs/alarm"
            },
            openApi = "/openapi/v1.json"
        });
    }
}
