using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Testimize.MCP.Server.Services;
using Testimize.Parameters.Core;
using Testimize.Parameters;
using Testimize.Contracts;
using Testimize.Usage;

namespace Testimize.MCP.Server.Controllers;

[ApiController]
[Route("api/test-cases")]
public class TestCaseGenerationController : ControllerBase
{
    private readonly IUtilityService _utilityService;
    private readonly IMcpProtocolHandler _mcpProtocolHandler;

    // Shared JsonSerializerOptions for consistent deserialization
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public TestCaseGenerationController(IUtilityService utilityService, IMcpProtocolHandler mcpProtocolHandler)
    {
        _utilityService = utilityService;
        _mcpProtocolHandler = mcpProtocolHandler;
    }

    /// <summary>
    /// Generate test cases using Hybrid Artificial Bee Colony algorithm with optimized defaults
    /// </summary>
    [HttpPost("hybrid")]
    public IActionResult GenerateHybridTestCases([FromBody] JsonElement requestBody)
    {
        try
        {
            Console.WriteLine("?? HYBRID ABC: Starting simplified hybrid test case generation via HTTP API...");
            
            // Use the same logic as McpProtocolHandler
            var result = _mcpProtocolHandler.GenerateHybridTestCases(requestBody);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"?? HYBRID ABC ERROR: {ex.Message}");
            return BadRequest(new 
            { 
                error = ex.Message,
                message = "Failed to generate hybrid test cases",
                timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff UTC")
            });
        }
    }

    /// <summary>
    /// Generate test cases using Pairwise algorithm for fast, efficient testing
    /// </summary>
    [HttpPost("pairwise")]
    public IActionResult GeneratePairwiseTestCases([FromBody] JsonElement requestBody)
    {
        try
        {
            Console.WriteLine("? PAIRWISE: Starting simplified pairwise test case generation via HTTP API...");
            
            // Use the same logic as McpProtocolHandler
            var result = _mcpProtocolHandler.GeneratePairwiseTestCases(requestBody);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? PAIRWISE ERROR: {ex.Message}");
            return BadRequest(new 
            { 
                error = ex.Message,
                message = "Failed to generate pairwise test cases",
                timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff UTC")
            });
        }
    }

    /// <summary>
    /// Generate test cases with full control over parameters and settings (legacy endpoint)
    /// </summary>
    [HttpPost("generate")]
    public IActionResult GenerateTestCases([FromBody] JsonDocument body)
    {
        try
        {
            var root = body.RootElement;

            if (!root.TryGetProperty("parameters", out var parametersElement) || 
                !root.TryGetProperty("settings", out var settingsElement))
            {
                return BadRequest(new { error = "Request must contain 'parameters' and 'settings' properties" });
            }

            var parameters = new List<IInputParameter>();
            for (int i = 0; i < parametersElement.GetArrayLength(); i++)
            {
                try
                {
                    var parameterElement = parametersElement[i];
                    var rawJson = parameterElement.GetRawText();

                    Console.WriteLine($"DEBUG: Processing parameter {i}: {rawJson}");

                    var universalParameter = JsonSerializer.Deserialize<UniversalDataParameter>(rawJson, JsonOptions);
                    if (universalParameter != null)
                    {
                        Console.WriteLine($"DEBUG: ParameterType = '{universalParameter.ParameterType ?? "NULL"}'");

                        if (string.IsNullOrWhiteSpace(universalParameter.ParameterType))
                        {
                            return BadRequest(new { error = $"Parameter {i} has empty or null ParameterType" });
                        }

                        // Handle SingleSelect and MultiSelect options
                        if (universalParameter.ParameterType == "SingleSelect" || universalParameter.ParameterType == "MultiSelect")
                        {
                            if (universalParameter.Options == null || universalParameter.Options.Length == 0)
                            {
                                return BadRequest(new { error = $"Parameter {i} of type {universalParameter.ParameterType} must have non-empty Options" });
                            }

                            universalParameter.PreciseTestValues = universalParameter.Options.Select(option => new TestValue
                            {
                                Value = option,
                                Category = TestValueCategory.Valid
                            }).ToArray();
                        }

                        // Ensure defaults for exploratory mode
                        if (!universalParameter.PreciseMode)
                        {
                            universalParameter.IncludeBoundaryValues ??= true;
                            universalParameter.AllowValidEquivalenceClasses ??= true;
                            universalParameter.AllowInvalidEquivalenceClasses ??= true;
                        }

                        var parameter = DataParameterFactory.CreateFromUniversal(universalParameter);
                        parameters.Add(parameter);

                        Console.WriteLine($"DEBUG: Successfully created parameter of type {parameter.GetType().Name}");
                    }
                    else
                    {
                        return BadRequest(new { error = $"Parameter {i} could not be deserialized" });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DEBUG: Error processing parameter {i}: {ex.Message}");
                    return BadRequest(new { error = $"Failed to process parameter {i}: {ex.Message}" });
                }
            }

            var settings = JsonSerializer.Deserialize<PreciseTestEngineSettings>(settingsElement.GetRawText(), JsonOptions);

            if (settings == null)
            {
                return BadRequest(new { error = "Failed to deserialize settings" });
            }

            // Only override specific critical settings - let library defaults handle the rest
            if (settings.Mode == (TestGenerationMode)0) // Only override if default Pairwise was used
            {
                settings.Mode = (TestGenerationMode)4; // HybridArtificialBeeColony
            }
            
            if (string.IsNullOrWhiteSpace(settings.MethodName) || settings.MethodName == "TestMethodName")
            {
                settings.MethodName = "FormValidation";
            }

            // Let ABCSettings use library defaults - no hardcoding
            if (settings.ABCSettings == null)
            {
                settings.ABCSettings = new Testimize.ABCGenerationSettings();
            }

            var testCases = _utilityService.Generate(parameters, settings);
            return Ok(new { testCases });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: Outer exception: {ex.Message}");
            Console.WriteLine($"DEBUG: Stack trace: {ex.StackTrace}");
            return BadRequest(new { error = ex.Message });
        }
    }
}