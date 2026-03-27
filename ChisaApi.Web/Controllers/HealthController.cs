using Microsoft.AspNetCore.Mvc;

namespace ChisaApi.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "Ok", service = "ChisaApi" });
    }
}
