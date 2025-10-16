using Microsoft.AspNetCore.Mvc;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimpleController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Drashyam API is running!", timestamp = DateTime.UtcNow });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", version = "1.0.0" });
    }
}
