using System;
using System.Collections.Generic;
using System.Text.Json;

// MCP Protocol implementation (Dependency Inversion - depends on abstraction)
public class McpProtocolHandler : IMcpProtocolHandler
{
    private readonly IUtilityService _utilityService;

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
}