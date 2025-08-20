using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Testimize.Parameters;
using Testimize.Parameters.Core;
using Testimize.Contracts;
using Testimize.Usage;
using Testimize.OutputGenerators;
using Testimize;
using Testimize.MCP.Server.Services;

// MCP Protocol implementation (Dependency Inversion - depends on abstraction)
public class McpProtocolHandler : IMcpProtocolHandler
{
    private readonly IUtilityService _utilityService;

    // In-memory settings that persist until restart
    private static PreciseTestEngineSettings _defaultSettings = new()
    {
        Mode = TestGenerationMode.HybridArtificialBeeColony,
        TestCaseCategory = TestCaseCategory.All,
        MethodName = "GeneratedTest",
        ABCSettings = new ABCGenerationSettings()
    };

    public static PreciseTestEngineSettings DefaultSettings => _defaultSettings;

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
                name = "testimize-mcp-server",
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
                    description = "Parameter type (case-sensitive)" 
                },
                PreciseMode = new { 
                    type = "boolean", 
                    description = "false for Text/Email/Phone/Password/Integer/Date/Url/Boolean, true for SingleSelect/MultiSelect" 
                },

                // Exploratory mode properties
                MinBoundary = new { description = "Required for exploratory mode (PreciseMode: false)" },
                MaxBoundary = new { description = "Required for exploratory mode (PreciseMode: false)" },
                IncludeBoundaryValues = new { 
                    type = "boolean", 
                    description = "Include boundary values (true for comprehensive testing)" 
                },
                AllowValidEquivalenceClasses = new { 
                    type = "boolean", 
                    description = "Generate valid equivalence classes (true for comprehensive, false for SingleSelect/MultiSelect)" 
                },
                AllowInvalidEquivalenceClasses = new { 
                    type = "boolean", 
                    description = "Generate invalid equivalence classes (true for comprehensive, false for SingleSelect/MultiSelect)" 
                },

                // Precise mode properties
                PreciseTestValues = new
                {
                    type = "array",
                    items = preciseValueItemSchema,
                    description = "Required for SingleSelect/MultiSelect with Valid category values"
                },

                Multiple = new { 
                    type = "boolean",
                    description = "Set to true for MultiSelect parameters"
                }
            },
            required = new[] { "ParameterType", "PreciseMode" }
        };

        // Simplified schema for hybrid and pairwise tools (only parameters)
        var simpleParametersSchema = new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                parameters = new
                {
                    type = "array",
                    items = parameterItemSchema,
                    description = "Array of test parameters"
                },
                methodName = new
                {
                    type = "string",
                    @default = "GeneratedTest",
                    description = "Optional test method name"
                }
            },
            required = new[] { "parameters" }
        };

        // Settings configuration schema
        var configureSettingsSchema = new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                instruction = new
                {
                    type = "string",
                    description = "Plain text instruction for what to change (e.g., 'set mutation rate to 0.5', 'change method name to UserTest')"
                },
                TestCaseCategory = new { 
                    type = "integer", 
                    @enum = new[] { 0, 1, 2 },
                    description = "0=All (comprehensive), 1=Valid only, 2=Validation only" 
                },
                MethodName = new { 
                    type = "string", 
                    description = "Default method name for generated tests" 
                },
                // Individual ABC property updates (for partial updates)
                TotalPopulationGenerations = new { type = "integer", minimum = 10, maximum = 200 },
                MutationRate = new { type = "number", minimum = 0.1, maximum = 0.9 },
                FinalPopulationSelectionRatio = new { type = "number", minimum = 0.1, maximum = 1.0 },
                Seed = new { type = "integer" },
                EnableScoutPhase = new { type = "boolean" },
                EnableOnlookerSelection = new { type = "boolean" },
                // Complete ABC settings object (for full updates)
                ABCSettings = new
                {
                    type = "object",
                    properties = new
                    {
                        TotalPopulationGenerations = new { type = "integer", minimum = 10, maximum = 200 },
                        MutationRate = new { type = "number", minimum = 0.1, maximum = 0.9 },
                        FinalPopulationSelectionRatio = new { type = "number", minimum = 0.1, maximum = 1.0 },
                        EliteSelectionRatio = new { type = "number", minimum = 0.1, maximum = 0.9 },
                        OnlookerSelectionRatio = new { type = "number", minimum = 0.05, maximum = 0.5 },
                        ScoutSelectionRatio = new { type = "number", minimum = 0.1, maximum = 0.5 },
                        EnableOnlookerSelection = new { type = "boolean" },
                        EnableScoutPhase = new { type = "boolean" },
                        EnforceMutationUniqueness = new { type = "boolean" },
                        StagnationThresholdPercentage = new { type = "number", minimum = 0.5, maximum = 1.0 },
                        CoolingRate = new { type = "number", minimum = 0.8, maximum = 0.99 },
                        AllowMultipleInvalidInputs = new { type = "boolean" },
                        Seed = new { type = "integer" }
                    }
                }
            }
        };

        var tools = new object[]
        {
            new { name = "health_check", description = "Get API health info", inputSchema = baseNoArgSchema },
            new { name = "get_time", description = "Get current UTC time", inputSchema = baseNoArgSchema },
            new { name = "generate_guid", description = "Generate a random GUID", inputSchema = baseNoArgSchema },
            
            // 🚀 NEW: Simplified Hybrid ABC tool
            new { 
                name = "generate_hybrid_test_cases", 
                description = @"🚀 SIMPLIFIED: Generate test cases using Hybrid Artificial Bee Colony algorithm with optimized defaults.

✅ AUTOMATIC SETTINGS APPLIED:
- Mode: HybridArtificialBeeColony (most effective)
- TestCaseCategory: All (comprehensive testing)
- ABCSettings: Optimized defaults (can be overridden via configure_testimize_settings)

📝 USAGE RULES:
1. Text/Email/Phone/Password/Integer/Date/Url/Boolean parameters:
   - PreciseMode: false
   - IncludeBoundaryValues: true
   - AllowValidEquivalenceClasses: true  
   - AllowInvalidEquivalenceClasses: true
   - MinBoundary and MaxBoundary required

2. SingleSelect/MultiSelect parameters:
   - PreciseMode: true
   - PreciseTestValues: array with Valid category values
   - AllowValidEquivalenceClasses: false
   - AllowInvalidEquivalenceClasses: false

🎯 PERFECT FOR: Maximum fault detection, form validation, comprehensive API testing", 
                inputSchema = simpleParametersSchema 
            },
            
            // 🚀 NEW: Simplified Pairwise tool  
            new { 
                name = "generate_pairwise_test_cases", 
                description = @"⚡ SIMPLIFIED: Generate test cases using Pairwise algorithm for fast, efficient testing.

✅ AUTOMATIC SETTINGS APPLIED:
- Mode: Pairwise (fast and efficient)
- TestCaseCategory: All (comprehensive testing)
- Minimal test suite covering all parameter interactions

📝 USAGE RULES: Same as hybrid tool but optimized for speed.

🎯 PERFECT FOR: Fast exploration, CI/CD pipelines, smoke testing, initial coverage", 
                inputSchema = simpleParametersSchema 
            },

            // 🚀 NEW: Settings configuration tool
            new { 
                name = "configure_testimize_settings", 
                description = @"CONFIGURE: Update default settings for Testimize test generation.

CONFIGURABLE OPTIONS:
- TestCaseCategory: Filter generated test types (0=All, 1=Valid, 2=Validation)
- MethodName: Default name for generated test methods
- ABCSettings: Fine-tune Hybrid ABC algorithm parameters
PERSISTENCE: Settings persist in-memory until server restart.
AFFECTS: Both generate_hybrid_test_cases and generate_pairwise_test_cases tools.", 
                inputSchema = configureSettingsSchema
            },

            // 📋 NEW: Get current settings tool
            new { 
                name = "get_testimize_settings", 
                description = @"📋 VIEW: Get current Testimize settings configuration.

🔍 RETURNS:
- Current TestCaseCategory setting
- Current default MethodName
- Complete ABCSettings configuration
- Information about what each setting does

💡 USE BEFORE: Configuring to see current values
🎯 PERFECT FOR: Understanding current configuration state", 
                inputSchema = baseNoArgSchema 
            }
        };

        return new { tools };
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
            "generate_hybrid_test_cases" => GenerateHybridTestCases(args ?? throw new ArgumentException("Missing 'arguments' for generate_hybrid_test_cases")),
            "generate_pairwise_test_cases" => GeneratePairwiseTestCases(args ?? throw new ArgumentException("Missing 'arguments' for generate_pairwise_test_cases")),
            "configure_testimize_settings" => ConfigureTestimizeSettings(args ?? throw new ArgumentException("Missing 'arguments' for configure_testimize_settings")),
            "get_testimize_settings" => GetTestimizeSettings(),
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

    // 🚀 NEW: Simplified Hybrid ABC Test Case Generation
    public object GenerateHybridTestCases(object @params)
    {
        Console.WriteLine("🚀 HYBRID ABC: Starting simplified hybrid test case generation...");
        
        var parametersAndSettings = ProcessSimplifiedParameters(@params, TestGenerationMode.HybridArtificialBeeColony);
        var testCases = _utilityService.Generate(parametersAndSettings.parameters, parametersAndSettings.settings);
        
        Console.WriteLine($"🚀 HYBRID ABC: Generated {testCases?.Count() ?? 0} test cases using advanced optimization");
        return new { testCases, mode = "HybridArtificialBeeColony", settings = parametersAndSettings.settings };
    }

    // ⚡ NEW: Simplified Pairwise Test Case Generation  
    public object GeneratePairwiseTestCases(object @params)
    {
        Console.WriteLine("⚡ PAIRWISE: Starting simplified pairwise test case generation...");
        
        var parametersAndSettings = ProcessSimplifiedParameters(@params, TestGenerationMode.Pairwise);
        var testCases = _utilityService.Generate(parametersAndSettings.parameters, parametersAndSettings.settings);
        
        Console.WriteLine($"⚡ PAIRWISE: Generated {testCases?.Count() ?? 0} test cases using efficient pairwise algorithm");
        return new { testCases, mode = "Pairwise", settings = parametersAndSettings.settings };
    }

    // ⚙️ ENHANCED: Configure Default Testimize Settings with Plain Text Support
    public object ConfigureTestimizeSettings(object @params)
    {
        Console.WriteLine("⚙️ CONFIG: Delegating to ABCSettingsService...");
        
        if (@params is not JsonElement argumentsElement)
        {
            throw new ArgumentException("Invalid parameters for ConfigureTestimizeSettings - not a JsonElement");
        }

        // Get the ABCSettingsService from the service provider or create a new instance
        // Note: In a real application, this should be injected via constructor
        var abcSettingsService = new ABCSettingsService(_defaultSettings);
        
        return abcSettingsService.ConfigureTestimizeSettings(argumentsElement);
    }

    // 📋 NEW: Get Current Testimize Settings
    public object GetTestimizeSettings()
    {
        Console.WriteLine("📋 VIEW: Delegating to ABCSettingsService...");
        
        // Get the ABCSettingsService from the service provider or create a new instance
        // Note: In a real application, this should be injected via constructor
        var abcSettingsService = new ABCSettingsService(_defaultSettings);
        
        return abcSettingsService.GetTestimizeSettings();
    }

    // 🔧 HELPER: Process simplified parameters for hybrid and pairwise tools
    private (List<IInputParameter> parameters, PreciseTestEngineSettings settings) ProcessSimplifiedParameters(object @params, TestGenerationMode mode)
    {
        if (@params is not JsonElement argumentsElement)
        {
            throw new ArgumentException("Invalid parameters - not a JsonElement");
        }

        if (!argumentsElement.TryGetProperty("parameters", out var parametersElement))
        {
            throw new ArgumentException("Missing 'parameters' in arguments");
        }

        // Process parameters with automatic error correction
        var parameters = new List<IInputParameter>();
        for (int i = 0; i < parametersElement.GetArrayLength(); i++)
        {
            var parameterElement = parametersElement[i];
            var rawJson = parameterElement.GetRawText();
            
            Console.WriteLine($"SIMPLIFIED DEBUG: Processing parameter {i}: {rawJson}");
            
            var universalParameter = JsonSerializer.Deserialize<UniversalDataParameter>(rawJson, JsonOptions);
            if (universalParameter == null)
            {
                throw new ArgumentException($"Parameter {i} could not be deserialized");
            }

            // Apply automatic error correction
            ApplyParameterCorrections(universalParameter, i);
            
            var parameter = DataParameterFactory.CreateFromUniversal(universalParameter);
            parameters.Add(parameter);
            
            Console.WriteLine($"SIMPLIFIED DEBUG: Successfully created parameter of type {parameter.GetType().Name}");
        }

        // Create settings based on current defaults and specified mode
        var settings = new PreciseTestEngineSettings
        {
            Mode = mode,
            TestCaseCategory = _defaultSettings.TestCaseCategory,
            MethodName = _defaultSettings.MethodName,
            ABCSettings = mode == TestGenerationMode.HybridArtificialBeeColony ? 
                (ABCGenerationSettings)_defaultSettings.ABCSettings.Clone() : 
                null
        };

        // Override method name if provided
        if (argumentsElement.TryGetProperty("methodName", out var methodNameElement))
        {
            var methodName = methodNameElement.GetString();
            if (!string.IsNullOrWhiteSpace(methodName))
            {
                settings.MethodName = methodName;
            }
        }

        Console.WriteLine($"SIMPLIFIED DEBUG: Using mode {mode} with {parameters.Count} parameters");
        return (parameters, settings);
    }

    // 🔧 HELPER: Apply automatic error corrections to parameters
    private void ApplyParameterCorrections(UniversalDataParameter universalParameter, int parameterIndex)
    {
        var paramType = universalParameter.ParameterType;
        var isSelectType = paramType == "SingleSelect" || paramType == "MultiSelect";
        var isRegularType = !isSelectType;

        // Auto-correct PreciseMode
        if (isRegularType && universalParameter.PreciseMode)
        {
            Console.WriteLine($"🔧 AUTO-CORRECT: Parameter {parameterIndex} ({paramType}) PreciseMode: true → false");
            universalParameter.PreciseMode = false;
        }
        if (isSelectType && !universalParameter.PreciseMode)
        {
            Console.WriteLine($"🔧 AUTO-CORRECT: Parameter {parameterIndex} ({paramType}) PreciseMode: false → true");
            universalParameter.PreciseMode = true;
        }

        // Auto-correct exploratory mode flags
        if (isRegularType && !universalParameter.PreciseMode)
        {
            if (universalParameter.IncludeBoundaryValues != true)
            {
                Console.WriteLine($"🔧 AUTO-CORRECT: Parameter {parameterIndex} IncludeBoundaryValues → true");
                universalParameter.IncludeBoundaryValues = true;
            }
            if (universalParameter.AllowValidEquivalenceClasses != true)
            {
                Console.WriteLine($"🔧 AUTO-CORRECT: Parameter {parameterIndex} AllowValidEquivalenceClasses → true");
                universalParameter.AllowValidEquivalenceClasses = true;
            }
            if (universalParameter.AllowInvalidEquivalenceClasses != true)
            {
                Console.WriteLine($"🔧 AUTO-CORRECT: Parameter {parameterIndex} AllowInvalidEquivalenceClasses → true");
                universalParameter.AllowInvalidEquivalenceClasses = true;
            }
        }

        // Auto-correct select type flags
        if (isSelectType && universalParameter.PreciseMode)
        {
            if (universalParameter.AllowValidEquivalenceClasses != false)
            {
                Console.WriteLine($"🔧 AUTO-CORRECT: Parameter {parameterIndex} ({paramType}) AllowValidEquivalenceClasses → false");
                universalParameter.AllowValidEquivalenceClasses = false;
            }
            if (universalParameter.AllowInvalidEquivalenceClasses != false)
            {
                Console.WriteLine($"🔧 AUTO-CORRECT: Parameter {parameterIndex} ({paramType}) AllowInvalidEquivalenceClasses → false");
                universalParameter.AllowInvalidEquivalenceClasses = false;
            }
        }

        // Handle Options to PreciseTestValues conversion for select types
        if (isSelectType && universalParameter.Options != null && universalParameter.Options.Length > 0)
        {
            Console.WriteLine($"🔧 AUTO-CORRECT: Parameter {parameterIndex} ({paramType}) converting Options to PreciseTestValues");
            universalParameter.PreciseTestValues = universalParameter.Options.Select(option => new TestValue
            {
                Value = option,
                Category = TestValueCategory.Valid
            }).ToArray();
            universalParameter.Options = null;
        }
    }
}