using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Testimize.MCP.Server.Services;

namespace Testimize.MCP.Server.Controllers;

[ApiController]
[Route("api/settings")]
public class ABCSettingsController : ControllerBase
{
    private readonly ABCSettingsService _abcSettingsService;
    private readonly IMcpProtocolHandler _mcpProtocolHandler;

    public ABCSettingsController(ABCSettingsService abcSettingsService, IMcpProtocolHandler mcpProtocolHandler)
    {
        _abcSettingsService = abcSettingsService;
        _mcpProtocolHandler = mcpProtocolHandler;
    }

    /// <summary>
    /// Update ABC Settings with a complete settings object
    /// </summary>
    [HttpPost("abc/update")]
    public IActionResult UpdateABCSettings([FromBody] JsonElement abcSettings)
    {
        var changes = _abcSettingsService.UpdateABCSettings(abcSettings);
        return Ok(new { changes });
    }

    /// <summary>
    /// Configure Testimize settings with comprehensive options including plain text instructions
    /// </summary>
    [HttpPost("configure")]
    public IActionResult ConfigureTestimizeSettings([FromBody] JsonElement settingsRequest)
    {
        try
        {
            var result = _abcSettingsService.ConfigureTestimizeSettings(settingsRequest);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = ex.Message,
                message = "Failed to configure Testimize settings",
                timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff UTC")
            });
        }
    }

    /// <summary>
    /// Get current Testimize settings configuration
    /// </summary>
    [HttpGet("current")]
    public IActionResult GetCurrentSettings()
    {
        try
        {
            var result = _abcSettingsService.GetTestimizeSettings();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = ex.Message,
                message = "Failed to retrieve current settings",
                timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff UTC")
            });
        }
    }

    /// <summary>
    /// Update settings using plain text instructions
    /// </summary>
    [HttpPost("configure/instruction")]
    public IActionResult ConfigureWithInstruction([FromBody] string instruction)
    {
        try
        {
            var requestObject = JsonDocument.Parse($"{{\"instruction\": \"{instruction}\"}}");
            var result = _abcSettingsService.ConfigureTestimizeSettings(requestObject.RootElement);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = ex.Message,
                message = "Failed to process plain text instruction",
                instruction,
                timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff UTC")
            });
        }
    }

    /// <summary>
    /// Legacy endpoint - kept for backward compatibility
    /// </summary>
    [HttpPost("update")]
    public IActionResult UpdateABCSettingsLegacy([FromBody] JsonElement abcSettings)
    {
        var changes = _abcSettingsService.UpdateABCSettings(abcSettings);
        return Ok(new { changes });
    }

    /// <summary>
    /// Test endpoint to call ConfigureTestimizeSettings via MCP Protocol Handler
    /// </summary>
    [HttpPost("mcp/configure-settings")]
    public IActionResult TestConfigureTestimizeSettings([FromBody] JsonElement settingsRequest)
    {
        try
        {
            var result = _mcpProtocolHandler.ConfigureTestimizeSettings(settingsRequest);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = ex.Message,
                message = "Failed to configure Testimize settings via MCP Protocol Handler",
                timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff UTC")
            });
        }
    }
}