using System;
using System.Collections.Generic;
using System.Text.Json;
using Testimize.Parameters;
using Testimize.Parameters.Core;
using Testimize.Contracts;
using Testimize.Usage;

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

    public object GenerateTestCases(object @params)
    {
        if (@params is not JsonElement jsonElement || !jsonElement.TryGetProperty("parameters", out var parametersElement) || !jsonElement.TryGetProperty("settings", out var settingsElement))
        {
            throw new ArgumentException("Invalid parameters for GenerateTestCases");
        }

        var parameters = new List<IInputParameter>();
        foreach (var parameterElement in parametersElement.EnumerateArray())
        {
            var type = parameterElement.GetProperty("Type").GetString();
            var values = parameterElement.GetProperty("Values").GetRawText();

            var parameter = type switch
            {
                "Testimize.Parameters.TextDataParameter" => JsonSerializer.Deserialize<TextDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.IntegerDataParameter" => JsonSerializer.Deserialize<IntegerDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.CurrencyDataParameter" => JsonSerializer.Deserialize<CurrencyDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.UsernameDataParameter" => JsonSerializer.Deserialize<UsernameDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.EmailDataParameter" => JsonSerializer.Deserialize<EmailDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.BooleanDataParameter" => JsonSerializer.Deserialize<BooleanDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.DateTimeDataParameter" => JsonSerializer.Deserialize<DateTimeDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.AddressDataParameter" => JsonSerializer.Deserialize<AddressDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.ColorDataParameter" => JsonSerializer.Deserialize<ColorDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.DateDataParameter" => JsonSerializer.Deserialize<DateDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.MonthDataParameter" => JsonSerializer.Deserialize<MonthDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.MultiSelectDataParameter" => JsonSerializer.Deserialize<MultiSelectDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.PasswordDataParameter" => JsonSerializer.Deserialize<PasswordDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.PercentageDataParameter" => JsonSerializer.Deserialize<PercentageDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.PhoneDataParameter" => JsonSerializer.Deserialize<PhoneDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.SingleSelectDataParameter" => JsonSerializer.Deserialize<SingleSelectDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.TimeDataParameter" => JsonSerializer.Deserialize<TimeDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.UrlDataParameter" => JsonSerializer.Deserialize<UrlDataParameter>(values) as IInputParameter,
                "Testimize.Parameters.WeekDataParameter" => JsonSerializer.Deserialize<WeekDataParameter>(values) as IInputParameter,
                _ => throw new ArgumentException($"Unknown parameter type: {type}")
            };

            parameters.Add(parameter);
        }

        var settings = JsonSerializer.Deserialize<PreciseTestEngineSettings>(settingsElement.GetRawText());

        if (settings == null)
        {
            throw new ArgumentException("Failed to deserialize settings");
        }

        var testCases = _utilityService.Generate(parameters, settings);
        return new { testCases };
    }
}