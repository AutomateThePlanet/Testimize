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
                ParameterType = new { 
                    type = "string", 
                    @enum = new[] { "Text", "Email", "Phone", "Password", "Integer", "Date", "Url", "Boolean", "SingleSelect", "MultiSelect", "Currency", "Username", "Address" },
                    description = "🚨 CRITICAL: Use 'Phone' for phone, 'Url' for URLs (case-sensitive)" 
                },
                PreciseMode = new { 
                    type = "boolean", 
                    description = "🚨 CRITICAL: false for Text/Email/Phone/Password/Integer/Date/Url/Boolean, true ONLY for SingleSelect/MultiSelect" 
                },

                // Exploratory mode properties (required when PreciseMode: false)
                MinBoundary = new { description = "Required for exploratory mode (PreciseMode: false)" },
                MaxBoundary = new { description = "Required for exploratory mode (PreciseMode: false)" },
                IncludeBoundaryValues = new { 
                    type = "boolean", 
                    description = "🚨 MUST be true for exploratory mode" 
                },
                AllowValidEquivalenceClasses = new { 
                    type = "boolean", 
                    description = "🚨 MUST be true for comprehensive testing in exploratory mode, false for SingleSelect/MultiSelect" 
                },
                AllowInvalidEquivalenceClasses = new { 
                    type = "boolean", 
                    description = "🚨 MUST be true for comprehensive testing in exploratory mode, false for SingleSelect/MultiSelect" 
                },

                // Precise mode properties (required when PreciseMode: true)
                PreciseTestValues = new
                {
                    type = "array",
                    items = preciseValueItemSchema,
                    description = "🚨 REQUIRED for SingleSelect/MultiSelect with Valid category values. NO Options property!"
                },

                // DEPRECATED - will be converted internally
                Options = new
                {
                    type = "array",
                    items = new { type = "string" },
                    description = "⚠️ DEPRECATED: Use PreciseTestValues instead. This will be auto-converted."
                },

                Multiple = new { 
                    type = "boolean",
                    description = "Set to true for MultiSelect parameters"
                }
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
                Mode = new { 
                    type = "integer", 
                    @enum = new[] { 0, 1, 2, 3, 4 },
                    @default = 4,
                    description = "🚨 CRITICAL: MUST be 4 for HybridArtificialBeeColony. Other values: 0=Pairwise, 1=OptimizedPairwise, 2=Combinatorial, 3=OptimizedCombinatorial" 
                },
                TestCaseCategory = new { 
                    type = "integer", 
                    @enum = new[] { 0, 1, 2 },
                    @default = 0,
                    description = "🚨 CRITICAL: MUST be 0 for comprehensive testing (All). Other values: 1=Valid only, 2=Validation only" 
                },
                MethodName = new { 
                    type = "string", 
                    minLength = 1,
                    @default = "FormValidation",
                    description = "Name for the generated test method" 
                },
                ABCSettings = abcSettingsSchema
            },
            required = new[] { "Mode", "TestCaseCategory", "MethodName", "ABCSettings" },
            @default = new
            {
                Mode = 4,
                TestCaseCategory = 0,
                MethodName = "FormValidation",
                ABCSettings = new
                {
                    TotalPopulationGenerations = 50,
                    MutationRate = 0.3,
                    FinalPopulationSelectionRatio = 0.5,
                    EliteSelectionRatio = 0.5,
                    OnlookerSelectionRatio = 0.1,
                    ScoutSelectionRatio = 0.3,
                    EnableOnlookerSelection = true,
                    EnableScoutPhase = true,
                    EnforceMutationUniqueness = true,
                    StagnationThresholdPercentage = 0.75,
                    CoolingRate = 0.95,
                    AllowMultipleInvalidInputs = false,
                    Seed = 42
                }
            }
        };

        var generateTestCasesInputSchema = new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                parameters = parametersArraySchema,
                settings = settingsSchema
            },
            required = new[] { "parameters", "settings" },
            @default = new
            {
                parameters = new object[] { },
                settings = new
                {
                    Mode = 4,
                    TestCaseCategory = 0,
                    MethodName = "FormValidation"
                }
            }
        };

        Console.WriteLine("DEBUG: Generating schema for generate_test_cases tool...");
        Console.WriteLine($"DEBUG: parametersArraySchema: {JsonSerializer.Serialize(parametersArraySchema)}");
        Console.WriteLine($"DEBUG: generateTestCasesInputSchema: {JsonSerializer.Serialize(generateTestCasesInputSchema)}");

        var tools = new object[]
        {
            new { name = "health_check", description = "Get API health info", inputSchema = baseNoArgSchema },
            new { name = "get_time", description = "Get current UTC time", inputSchema = baseNoArgSchema },
            new { name = "generate_guid", description = "Generate a random GUID", inputSchema = baseNoArgSchema },
            new { 
                name = "generate_test_cases", 
                description = @"🚨 CRITICAL: ASSISTANT MUST FOLLOW EXACT FORMAT RULES 🚨

❌ DETECTED ERRORS IN RECENT REQUESTS:
1. Using PreciseMode: true with MinBoundary/MaxBoundary - WRONG!
2. Missing IncludeBoundaryValues, AllowValidEquivalenceClasses, AllowInvalidEquivalenceClasses - WRONG!
3. Using Options instead of PreciseTestValues for SingleSelect/MultiSelect - WRONG!
4. Using Mode: 2 instead of Mode: 4 - WRONG!
5. Using TestCaseCategory: 2 instead of TestCaseCategory: 0 - WRONG!
6. Missing ABCSettings - WRONG!

✅ MANDATORY RULES - NO EXCEPTIONS:
1. Text/Email/Phone/Password/Integer/Date/Url/Boolean parameters MUST use:
   - PreciseMode: false
   - IncludeBoundaryValues: true
   - AllowValidEquivalenceClasses: true  
   - AllowInvalidEquivalenceClasses: true
   - MinBoundary and MaxBoundary (appropriate for type)

2. SingleSelect/MultiSelect parameters MUST use:
   - PreciseMode: true
   - PreciseTestValues array with Valid category values
   - NO Options property
   - AllowValidEquivalenceClasses: false
   - AllowInvalidEquivalenceClasses: false

3. Settings MUST include:
   - Mode: 4 (HybridArtificialBeeColony)
   - TestCaseCategory: 0 (All)
   - MethodName: string
   - ABCSettings: complete object with all properties

✅ CORRECT PHONE PARAMETER TYPE: Use 'Phone' NOT 'Text'
✅ CORRECT URL PARAMETER TYPE: Use 'Url' NOT 'URL' or 'UrlDataParameter'

🔥 ASSISTANT: If you generate ANY request that violates these rules, it WILL FAIL!", 
                inputSchema = generateTestCasesInputSchema 
            }
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

                    // 🚨 CRITICAL ERROR DETECTION AND CORRECTION
                    var paramType = universalParameter.ParameterType;
                    var isSelectType = paramType == "SingleSelect" || paramType == "MultiSelect";
                    var isRegularType = !isSelectType;

                    // ERROR DETECTION #1: Wrong PreciseMode usage
                    if (isRegularType && universalParameter.PreciseMode)
                    {
                        Console.WriteLine($"🚨 ERROR DETECTED: Parameter {i} ({paramType}) has PreciseMode: true but should be false. CORRECTING...");
                        universalParameter.PreciseMode = false;
                    }
                    if (isSelectType && !universalParameter.PreciseMode)
                    {
                        Console.WriteLine($"🚨 ERROR DETECTED: Parameter {i} ({paramType}) has PreciseMode: false but should be true. CORRECTING...");
                        universalParameter.PreciseMode = true;
                    }

                    // ERROR DETECTION #2: Missing required exploratory flags
                    if (isRegularType && universalParameter.PreciseMode == false)
                    {
                        if (universalParameter.IncludeBoundaryValues != true)
                        {
                            Console.WriteLine($"🚨 ERROR DETECTED: Parameter {i} missing IncludeBoundaryValues: true. CORRECTING...");
                            universalParameter.IncludeBoundaryValues = true;
                        }
                        if (universalParameter.AllowValidEquivalenceClasses != true)
                        {
                            Console.WriteLine($"🚨 ERROR DETECTED: Parameter {i} missing AllowValidEquivalenceClasses: true. CORRECTING...");
                            universalParameter.AllowValidEquivalenceClasses = true;
                        }
                        if (universalParameter.AllowInvalidEquivalenceClasses != true)
                        {
                            Console.WriteLine($"🚨 ERROR DETECTED: Parameter {i} missing AllowInvalidEquivalenceClasses: true. CORRECTING...");
                            universalParameter.AllowInvalidEquivalenceClasses = true;
                        }
                    }

                    // ERROR DETECTION #3: Select types using precise mode but wrong flags
                    if (isSelectType && universalParameter.PreciseMode == true)
                    {
                        if (universalParameter.AllowValidEquivalenceClasses != false)
                        {
                            Console.WriteLine($"🚨 ERROR DETECTED: Parameter {i} ({paramType}) should have AllowValidEquivalenceClasses: false. CORRECTING...");
                            universalParameter.AllowValidEquivalenceClasses = false;
                        }
                        if (universalParameter.AllowInvalidEquivalenceClasses != false)
                        {
                            Console.WriteLine($"🚨 ERROR DETECTED: Parameter {i} ({paramType}) should have AllowInvalidEquivalenceClasses: false. CORRECTING...");
                            universalParameter.AllowInvalidEquivalenceClasses = false;
                        }
                    }

                    // Handle SingleSelect and MultiSelect options -> PreciseTestValues conversion
                    if (isSelectType)
                    {
                        if (universalParameter.Options != null && universalParameter.Options.Length > 0)
                        {
                            Console.WriteLine($"🚨 ERROR DETECTED: Parameter {i} ({paramType}) using deprecated Options. Converting to PreciseTestValues...");
                            universalParameter.PreciseTestValues = universalParameter.Options.Select(option => new TestValue
                            {
                                Value = option,
                                Category = TestValueCategory.Valid
                            }).ToArray();
                            
                            // Clear Options since we've converted them
                            universalParameter.Options = null;
                        }
                        else if (universalParameter.PreciseTestValues == null || universalParameter.PreciseTestValues.Length == 0)
                        {
                            throw new ArgumentException($"Parameter {i} of type {universalParameter.ParameterType} must have Options or PreciseTestValues");
                        }
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

        // 🚨 CRITICAL ERROR DETECTION AND CORRECTION FOR SETTINGS
        bool settingsChanged = false;

        // ERROR DETECTION #6: Missing or wrong MethodName
        if (string.IsNullOrWhiteSpace(settings.MethodName) || settings.MethodName == "TestMethodName")
        {
            Console.WriteLine($"🚨 ERROR DETECTED: MethodName is '{settings.MethodName}' but should be meaningful. CORRECTING...");
            settings.MethodName = "FormValidation";
            settingsChanged = true;
        }

        // ERROR DETECTION #7: Missing ABCSettings
        if (settings.ABCSettings == null)
        {
            settings.ABCSettings = new Testimize.ABCGenerationSettings();
            settingsChanged = true;
        }

        if (settingsChanged)
        {
            Console.WriteLine($"🚨 ASSISTANT ERROR SUMMARY: Multiple errors were detected and corrected automatically. Please update your request format!");
        }
        
        if (string.IsNullOrWhiteSpace(settings.MethodName) || settings.MethodName == "TestMethodName")
        {
            settings.MethodName = "FormValidation";
        }

        Console.WriteLine($"MCP DEBUG: Successfully processed {parameters.Count} parameters");
        Console.WriteLine($"MCP DEBUG: Final Settings - Mode: {settings.Mode}, TestCaseCategory: {settings.TestCaseCategory}, MethodName: {settings.MethodName}");
        Console.WriteLine($"MCP DEBUG: ABCSettings - Generations: {settings.ABCSettings.TotalPopulationGenerations}, MutationRate: {settings.ABCSettings.MutationRate}");

        if (parameters.Count == 0)
        {
            throw new ArgumentException("No valid parameters were processed");
        }

        var testCases = _utilityService.Generate(parameters, settings);
        Console.WriteLine($"MCP DEBUG: Generated {testCases?.Count() ?? 0} test cases");

        return new { testCases };
    }
}