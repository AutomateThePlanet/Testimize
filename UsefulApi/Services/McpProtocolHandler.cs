using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Testimize.Parameters;
using Testimize.Parameters.Core;
using Testimize.Contracts;
using Testimize.Usage;

// MCP Protocol implementation (Dependency Inversion - depends on abstraction)
public class McpProtocolHandler : IMcpProtocolHandler
{
    private readonly IUtilityService _utilityService;

    // Shared JsonSerializerOptions for consistent deserialization
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public McpProtocolHandler(IUtilityService utilityService)
    {
        _utilityService = utilityService ?? throw new ArgumentNullException(nameof(utilityService));
    }

    public object Initialize(object @params)
    {
        return new
        {
            protocolVersion = "2024-11-05",
            serverInfo = new
            {
                name = "useful-api",
                version = "1.0.0"
            },
            capabilities = new
            {
                tools = new { },
                resources = new { },
                prompts = new { }
            }
        };
    }

    public object ToolsList()
    {
        return new
        {
            tools = new object[]
            {
                new
                {
                    name = "health_check",
                    description = "Get API health info",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new { },
                        required = new string[] { }
                    }
                },
                new
                {
                    name = "get_time",
                    description = "Get current UTC time",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new { },
                        required = new string[] { }
                    }
                },
                new
                {
                    name = "generate_guid",
                    description = "Generate a random GUID",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new { },
                        required = new string[] { }
                    }
                },
                new
                {
                    name = "generate_test_cases",
                    description = "Generate test cases using Testimize engine",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            parameters = new
                            {
                                type = "array",
                                description = "Array of parameter definitions"
                            },
                            settings = new
                            {
                                type = "object",
                                description = "Test generation settings"
                            }
                        },
                        required = new[] { "parameters", "settings" }
                    }
                }
            }
        };
    }

    public object ToolsCall(object @params)
    {
        string name = "";
        
        // Handle different parameter formats
        if (@params is JsonElement jsonElement)
        {
            // Extract name from JSON structure: {"name":"tool_name","arguments":{},"_meta":{...}}
            if (jsonElement.TryGetProperty("name", out var nameProperty))
            {
                name = nameProperty.GetString() ?? "";
            }
        }
        else if (@params is Dictionary<string, object> paramsDict)
        {
            // Fallback for dictionary format
            name = paramsDict?["name"]?.ToString() ?? "";
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new Exception("Tool name not found in parameters");
        }

        object result = name switch
        {
            "health_check" => _utilityService.GetHealth(),
            "get_time" => _utilityService.GetTime(),
            "generate_guid" => _utilityService.GenerateGuid(),
            "generate_test_cases" => GenerateTestCases(@params),
            _ => throw new Exception($"Unknown tool: {name}")
        };

        return new
        {
            content = new object[]
            {
                new
                {
                    type = "text",
                    text = System.Text.Json.JsonSerializer.Serialize(result)
                }
            }
        };
    }

    public object GenerateTestCases(object @params)
    {
        if (@params is not JsonElement jsonElement)
        {
            throw new ArgumentException("Invalid parameters for GenerateTestCases - not a JsonElement");
        }

        // Extract arguments from the MCP call structure
        if (!jsonElement.TryGetProperty("arguments", out var argumentsElement))
        {
            throw new ArgumentException("Missing 'arguments' property in GenerateTestCases call");
        }

        if (!argumentsElement.TryGetProperty("parameters", out var parametersElement) || 
            !argumentsElement.TryGetProperty("settings", out var settingsElement))
        {
            throw new ArgumentException("Missing 'parameters' or 'settings' in arguments");
        }

        var parameters = new List<IInputParameter>();
        for (int i = 0; i < parametersElement.GetArrayLength(); i++)
        {
            try
            {
                var parameterElement = parametersElement[i];
                var rawJson = parameterElement.GetRawText();
                
                Console.WriteLine($"MCP DEBUG: Processing parameter {i}: {rawJson}");
                
                var universalParameter = JsonSerializer.Deserialize<UniversalDataParameter>(rawJson, JsonOptions);
                if (universalParameter != null)
                {
                    Console.WriteLine($"MCP DEBUG: ParameterType = '{universalParameter.ParameterType ?? "NULL"}'");
                    
                    if (string.IsNullOrWhiteSpace(universalParameter.ParameterType))
                    {
                        throw new ArgumentException($"Parameter {i} has empty or null ParameterType");
                    }
                    
                    var parameter = DataParameterFactory.CreateFromUniversal(universalParameter);
                    parameters.Add(parameter);
                    
                    Console.WriteLine($"MCP DEBUG: Successfully created parameter of type {parameter.GetType().Name}");
                }
                else
                {
                    throw new ArgumentException($"Parameter {i} could not be deserialized");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MCP DEBUG: Error processing parameter {i}: {ex.Message}");
                throw new ArgumentException($"Failed to process parameter {i}: {ex.Message}", ex);
            }
        }

        var settings = JsonSerializer.Deserialize<PreciseTestEngineSettings>(settingsElement.GetRawText(), JsonOptions);

        if (settings == null)
        {
            throw new ArgumentException("Failed to deserialize settings");
        }

        Console.WriteLine($"MCP DEBUG: Successfully processed {parameters.Count} parameters");
        var testCases = _utilityService.Generate(parameters, settings);
        Console.WriteLine($"MCP DEBUG: Generated {testCases?.Count() ?? 0} test cases");
        
        return new { testCases };
    }
}