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
                tools = new { list = new { }, call = new { } },
                resources = new { list = new { } },
                prompts = new { list = new { } }
            }
        };
    }

    public object ToolsList()
    {
        var baseNoArgSchema = new
        {
            type = "object",
            additionalProperties = false,
            properties = new { } // no inputs
        };

        var preciseValueItemSchema = new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                Value = new { }, // any
                Category = new { type = "string", description = "Valid | BoundaryValid | BoundaryInvalid | Invalid" },
                ExpectedInvalidMessage = new { type = "string" }
            },
            required = new[] { "Value", "Category" }
        };

        var parameterItemSchema = new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                ParameterType = new { type = "string", description = "Text | Email | Integer | Currency | Boolean | MultiSelect | ..." },
                PreciseMode = new { type = "boolean" },

                MinBoundary = new { },     // any
                MaxBoundary = new { },     // any

                IncludeBoundaryValues = new { type = "boolean" },
                AllowValidEquivalenceClasses = new { type = "boolean" },
                AllowInvalidEquivalenceClasses = new { type = "boolean" },

                PreciseTestValues = new
                {
                    type = "array",
                    minItems = 1,
                    items = preciseValueItemSchema
                },

                Options = new
                {
                    type = "array",
                    minItems = 1,
                    items = new { type = "string" }
                },

                Multiple = new { type = "boolean" }
            },
            required = new[] { "ParameterType", "PreciseMode" }
        };

        var parametersArraySchema = new
        {
            type = "array",
            minItems = 0, // Allow empty arrays
            items = parameterItemSchema,
            // helpful default for clients that pre-fill arguments
            @default = Array.Empty<object>()
        };

        var abcSettingsSchema = new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                TotalPopulationGenerations = new { type = "integer", minimum = 10, maximum = 200, @default = 50 },
                MutationRate = new { type = "number", minimum = 0.1, maximum = 0.9, @default = 0.3 },
                FinalPopulationSelectionRatio = new { type = "number", minimum = 0.1, maximum = 1.0, @default = 0.5 },
                EliteSelectionRatio = new { type = "number", minimum = 0.1, maximum = 0.9, @default = 0.2 },
                OnlookerSelectionRatio = new { type = "number", minimum = 0.05, maximum = 0.5, @default = 0.3 },
                ScoutSelectionRatio = new { type = "number", minimum = 0.1, maximum = 0.5, @default = 0.1 },
                EnableOnlookerSelection = new { type = "boolean", @default = true },
                EnableScoutPhase = new { type = "boolean", @default = true },
                EnforceMutationUniqueness = new { type = "boolean", @default = true },
                StagnationThresholdPercentage = new { type = "number", minimum = 0.5, maximum = 1.0, @default = 0.8 },
                CoolingRate = new { type = "number", minimum = 0.8, maximum = 0.99, @default = 0.95 },
                AllowMultipleInvalidInputs = new { type = "boolean", @default = false },
                Seed = new { type = "integer" }
            }
        };

        var settingsSchema = new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                Mode = new { type = "integer", description = "0..4", minimum = 0, maximum = 4 },
                TestCaseCategory = new { type = "integer", description = "0..3", minimum = 0, maximum = 3 },
                MethodName = new { type = "string", minLength = 1 },
                ABCSettings = abcSettingsSchema
            },
            required = new[] { "Mode", "TestCaseCategory", "MethodName" }
        };

        var generateTestCasesInputSchema = new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                parameters = parametersArraySchema, // Optional (allowed empty)
                settings = settingsSchema
            },
            required = new[] { "settings" } // Only settings are required
        };

        Console.WriteLine("DEBUG: Generating schema for generate_test_cases tool...");
        Console.WriteLine($"DEBUG: parametersArraySchema: {JsonSerializer.Serialize(parametersArraySchema)}");
        Console.WriteLine($"DEBUG: generateTestCasesInputSchema: {JsonSerializer.Serialize(generateTestCasesInputSchema)}");

        var tools = new object[]
        {
            new { name = "health_check",  description = "Get API health info",       inputSchema = baseNoArgSchema },
            new { name = "get_time",      description = "Get current UTC time",       inputSchema = baseNoArgSchema },
            new { name = "generate_guid", description = "Generate a random GUID",     inputSchema = baseNoArgSchema },
            new { name = "generate_test_cases", description = "Generate test cases using Testimize engine", inputSchema = generateTestCasesInputSchema }
        };

        Console.WriteLine($"DEBUG: Tools list: {JsonSerializer.Serialize(tools)}");

        return new
        {
            tools
        };
    }

    public object ToolsCall(object @params)
    {
        string name = "";
        JsonElement? args = null;

        if (@params is JsonElement je)
        {
            if (je.TryGetProperty("name", out var n)) name = n.GetString() ?? "";
            if (je.TryGetProperty("arguments", out var a)) args = a;
        }
        else if (@params is Dictionary<string, object> d)
        {
            name = d.TryGetValue("name", out var n) ? n?.ToString() ?? "" : "";
            if (d.TryGetValue("arguments", out var a) && a is JsonElement aje) args = aje;
        }

        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Tool name not found in parameters.");

        object result = name switch
        {
            "health_check" => _utilityService.GetHealth(),
            "get_time" => _utilityService.GetTime(),
            "generate_guid" => _utilityService.GenerateGuid(),
            "generate_test_cases" => GenerateTestCases(args ?? throw new ArgumentException("Missing 'arguments' for generate_test_cases")),
            _ => throw new Exception($"Unknown tool: {name}")
        };

        return new
        {
            content = new object[] { new { type = "text", text = JsonSerializer.Serialize(result) } }
        };
    }

    public object GenerateTestCases(object @params)
    {
        if (@params is not JsonElement argumentsElement)
        {
            throw new ArgumentException("Invalid parameters for GenerateTestCases - not a JsonElement");
        }

        if (!argumentsElement.TryGetProperty("parameters", out var parametersElement))
        {
            parametersElement = JsonDocument.Parse("[]").RootElement; // Default to empty array
        }

        if (!argumentsElement.TryGetProperty("settings", out var settingsElement))
        {
            throw new ArgumentException("Missing 'settings' in arguments");
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
        Console.WriteLine("DEBUG: Validating settings and parameters before generating test cases...");
        Console.WriteLine($"DEBUG: Settings: {JsonSerializer.Serialize(settings, JsonOptions)}");
        Console.WriteLine($"DEBUG: Parameters: {JsonSerializer.Serialize(parameters, JsonOptions)}");

        if (settings == null)
        {
            throw new ArgumentException("Settings object is null");
        }

        if (parameters == null || parameters.Count == 0)
        {
            throw new ArgumentException("Parameters list is null or empty");
        }

        var testCases = _utilityService.Generate(parameters, settings);
        Console.WriteLine($"DEBUG: Generated {testCases?.Count() ?? 0} test cases");
        
        return new { testCases };
    }
}