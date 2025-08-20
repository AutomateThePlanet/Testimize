using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Testimize.MCP.Server.Services;

namespace Testimize.MCP.Server.Controllers;

[ApiController]
[Route("api")]
public class UtilityController : ControllerBase
{
    private readonly IUtilityService _utilityService;

    public UtilityController(IUtilityService utilityService)
    {
        _utilityService = utilityService;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        var result = _utilityService.GetHealth();
        return Ok(result);
    }

    /// <summary>
    /// Get current UTC time in multiple formats
    /// </summary>
    [HttpGet("time")]
    public IActionResult GetTime()
    {
        var result = _utilityService.GetTime();
        return Ok(result);
    }

    /// <summary>
    /// Generate a new random GUID
    /// </summary>
    [HttpGet("guid")]
    public IActionResult GenerateGuid()
    {
        var result = _utilityService.GenerateGuid();
        return Ok(result);
    }

    /// <summary>
    /// Echo the posted JSON payload
    /// </summary>
    [HttpPost("echo")]
    public IActionResult Echo([FromBody] JsonElement body)
    {
        return Ok(new { received = body });
    }

    /// <summary>
    /// Root endpoint - redirects to health
    /// </summary>
    [HttpGet("/")]
    public IActionResult Root()
    {
        return Redirect("/api/health");
    }
}