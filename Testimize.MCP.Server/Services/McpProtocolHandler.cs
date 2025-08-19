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
                description = @"⚙️ CONFIGURE: Update default settings for Testimize test generation.

🔧 CONFIGURABLE OPTIONS:
- TestCaseCategory: Filter generated test types (All/Valid/Validation)
- MethodName: Default name for generated test methods
- ABCSettings: Fine-tune Hybrid ABC algorithm parameters

💬 PLAIN TEXT SUPPORT: You can describe what to change in natural language!
Examples:
- ""Set mutation rate to 0.5""
- ""Change method name to UserRegistrationTest""
- ""Set total generations to 100""
- ""Enable scout phase""

💾 PERSISTENCE: Settings persist in-memory until server restart.
🎯 AFFECTS: Both generate_hybrid_test_cases and generate_pairwise_test_cases tools.", 
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
        Console.WriteLine("⚙️ CONFIG: Updating Testimize default settings...");
        
        if (@params is not JsonElement argumentsElement)
        {
            throw new ArgumentException("Invalid parameters for ConfigureTestimizeSettings - not a JsonElement");
        }

        var changesApplied = new List<string>();
        var errors = new List<string>();
        var warnings = new List<string>();

        Console.WriteLine($"🔍 DEBUG: Raw input JSON: {argumentsElement.GetRawText()}");

        // Support for plain text instructions
        if (argumentsElement.TryGetProperty("instruction", out var instructionElement))
        {
            var instruction = instructionElement.GetString()?.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(instruction))
            {
                Console.WriteLine($"⚙️ CONFIG: Processing plain text instruction: '{instruction}'");
                var changes = ProcessPlainTextInstruction(instruction);
                changesApplied.AddRange(changes);
            }
        }

        // Update TestCaseCategory if provided
        if (argumentsElement.TryGetProperty("testCaseCategory", out var categoryElement))
        {
            Console.WriteLine($"🔍 DEBUG: Processing TestCaseCategory - ValueKind: {categoryElement.ValueKind}, RawValue: '{categoryElement.GetRawText()}'");
            try
            {
                var category = categoryElement.GetInt32();
                if (category >= 0 && category <= 2)
                {
                    var oldCategory = _defaultSettings.TestCaseCategory;
                    _defaultSettings.TestCaseCategory = (TestCaseCategory)category;
                    var categoryName = category switch
                    {
                        0 => "All (comprehensive testing)",
                        1 => "Valid only",
                        2 => "Validation/Invalid only",
                        _ => category.ToString()
                    };
                    var change = $"Updated TestCaseCategory from {oldCategory} to {categoryName}";
                    Console.WriteLine($"✅ SUCCESS: {change}");
                    changesApplied.Add(change);
                }
                else
                {
                    var errorMsg = $"TestCaseCategory value {category} is invalid. Valid values: 0 (All), 1 (Valid), 2 (Validation)";
                    Console.WriteLine($"❌ VALIDATION ERROR: {errorMsg}");
                    errors.Add(errorMsg);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Failed to parse TestCaseCategory from '{categoryElement.GetRawText()}': {ex.Message}";
                Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                errors.Add(errorMsg);
            }
        }

        // Update MethodName if provided
        if (argumentsElement.TryGetProperty("methodName", out var methodNameElement))
        {
            Console.WriteLine($"🔍 DEBUG: Processing MethodName - ValueKind: {methodNameElement.ValueKind}, RawValue: '{methodNameElement.GetRawText()}'");
            var methodName = methodNameElement.GetString();
            if (!string.IsNullOrWhiteSpace(methodName))
            {
                var oldMethodName = _defaultSettings.MethodName;
                _defaultSettings.MethodName = methodName;
                var change = $"Updated MethodName from '{oldMethodName}' to '{_defaultSettings.MethodName}'";
                Console.WriteLine($"✅ SUCCESS: {change}");
                changesApplied.Add(change);
            }
            else
            {
                var warningMsg = "MethodName is empty or whitespace - ignoring";
                Console.WriteLine($"⚠️ WARNING: {warningMsg}");
                warnings.Add(warningMsg);
            }
        }

        // Update individual ABCSettings properties if provided
        if (argumentsElement.TryGetProperty("abcSettings", out var abcElement))
        {
            Console.WriteLine($"🔍 DEBUG: Processing complete ABCSettings object");
            var abcChanges = UpdateABCSettings(abcElement);
            changesApplied.AddRange(abcChanges);
        }

        // Support for individual ABC property updates
        Console.WriteLine($"🔍 DEBUG: Processing individual ABC properties...");
        UpdateIndividualABCProperties(argumentsElement, changesApplied);

        // Separate successful changes from errors and warnings
        var successfulChanges = changesApplied.Where(c => !c.StartsWith("FAILED:") && !c.StartsWith("WARNING:")).ToList();
        var failedChanges = changesApplied.Where(c => c.StartsWith("FAILED:")).Select(c => c.Substring(7)).ToList();
        var configWarnings = changesApplied.Where(c => c.StartsWith("WARNING:")).Select(c => c.Substring(8)).ToList();

        errors.AddRange(failedChanges);
        warnings.AddRange(configWarnings);

        // Create comprehensive response message
        var messageParts = new List<string>();
        
        if (successfulChanges.Count > 0)
        {
            messageParts.Add($"✅ {successfulChanges.Count} setting(s) updated successfully");
        }
        
        if (errors.Count > 0)
        {
            messageParts.Add($"❌ {errors.Count} error(s) occurred");
        }
        
        if (warnings.Count > 0)
        {
            messageParts.Add($"⚠️ {warnings.Count} warning(s)");
        }

        if (messageParts.Count == 0)
        {
            messageParts.Add("No changes were requested");
        }

        var message = string.Join(", ", messageParts);

        Console.WriteLine($"⚙️ CONFIG SUMMARY: {message}");

        return new 
        { 
            message,
            successful = successfulChanges,
            errors = errors.Count > 0 ? errors : null,
            warnings = warnings.Count > 0 ? warnings : null,
            currentSettings = GetCurrentSettingsObject(),
            debug = new
            {
                requestReceived = argumentsElement.GetRawText(),
                totalPropertiesProcessed = argumentsElement.EnumerateObject().Count(),
                processingTimestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff UTC")
            }
        };
    }

    // 📋 NEW: Get Current Testimize Settings
    public object GetTestimizeSettings()
    {
        Console.WriteLine("📋 VIEW: Retrieving current Testimize settings...");
        
        return new
        {
            message = "Current Testimize settings configuration",
            settings = GetCurrentSettingsObject(),
            explanation = new
            {
                TestCaseCategory = new
                {
                    current = _defaultSettings.TestCaseCategory.ToString(),
                    options = new
                    {
                        All = "0 - Generate comprehensive test cases (valid + invalid + boundary)",
                        Valid = "1 - Generate only valid test cases",
                        Validation = "2 - Generate only validation/invalid test cases"
                    }
                },
                MethodName = new
                {
                    current = _defaultSettings.MethodName,
                    description = "Default name used for generated test methods"
                },
                ABCSettings = new
                {
                    description = "Hybrid Artificial Bee Colony algorithm parameters",
                    TotalPopulationGenerations = "Number of optimization iterations (10-200, higher = better optimization)",
                    MutationRate = "Exploration intensity (0.1-0.9, higher = more exploration)",
                    FinalPopulationSelectionRatio = "Percentage of best test cases to keep (0.1-1.0)",
                    EliteSelectionRatio = "Percentage of best solutions preserved unchanged (0.1-0.9)",
                    OnlookerSelectionRatio = "Percentage used in onlooker phase (0.05-0.5)",
                    ScoutSelectionRatio = "Percentage for random exploration when stagnant (0.1-0.5)",
                    EnableOnlookerSelection = "Enable exploitation of better solution regions",
                    EnableScoutPhase = "Enable random exploration when population stagnates",
                    EnforceMutationUniqueness = "Ensure all mutations are unique (slower but more diverse)",
                    StagnationThresholdPercentage = "When to trigger scout phase (0.5-1.0)",
                    CoolingRate = "Rate of mutation intensity reduction (0.8-0.99)",
                    AllowMultipleInvalidInputs = "Allow test cases with multiple invalid parameters",
                    Seed = "Random seed for reproducible results"
                }
            }
        };
    }

    // 🔧 HELPER: Process plain text instructions
    private List<string> ProcessPlainTextInstruction(string instruction)
    {
        var changes = new List<string>();
        Console.WriteLine($"🔍 DEBUG: Processing plain text instruction: '{instruction}'");
        
        // Method name patterns
        if (instruction.Contains("method name") || instruction.Contains("methodname"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(instruction, @"(?:method name|methodname).*?(?:to|=)\s*([a-zA-Z_][a-zA-Z0-9_]*)");
            if (match.Success)
            {
                var newName = match.Groups[1].Value;
                var oldName = _defaultSettings.MethodName;
                _defaultSettings.MethodName = newName;
                var change = $"Updated MethodName from '{oldName}' to '{newName}' (from instruction)";
                Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                changes.Add(change);
            }
            else
            {
                var errorMsg = "Could not parse method name from instruction. Expected format: 'method name to SomeName'";
                Console.WriteLine($"❌ PLAIN TEXT PARSE ERROR: {errorMsg}");
                changes.Add($"FAILED: {errorMsg}");
            }
        }

        // Mutation rate patterns
        if (instruction.Contains("mutation rate"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(instruction, @"mutation rate.*?(?:to|=)\s*([\d.]+)");
            if (match.Success && double.TryParse(match.Groups[1].Value, out var rate))
            {
                Console.WriteLine($"🔍 DEBUG: Parsed mutation rate: {rate}");
                if (rate >= 0.1 && rate <= 0.9)
                {
                    var oldRate = _defaultSettings.ABCSettings.MutationRate;
                    _defaultSettings.ABCSettings.MutationRate = rate;
                    var change = $"Updated MutationRate from {oldRate:F3} to {rate:F3} (from instruction)";
                    Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                    changes.Add(change);
                }
                else
                {
                    var errorMsg = $"Mutation rate {rate} is out of range [0.1-0.9]. Value not updated.";
                    Console.WriteLine($"❌ PLAIN TEXT VALIDATION ERROR: {errorMsg}");
                    changes.Add($"FAILED: {errorMsg}");
                }
            }
            else
            {
                var errorMsg = "Could not parse mutation rate from instruction. Expected format: 'mutation rate to 0.5'";
                Console.WriteLine($"❌ PLAIN TEXT PARSE ERROR: {errorMsg}");
                changes.Add($"FAILED: {errorMsg}");
            }
        }

        // Elite selection ratio patterns
        if (instruction.Contains("elite selection") || instruction.Contains("elite ratio"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(instruction, @"(?:elite selection|elite ratio).*?(?:to|=)\s*([\d.]+)");
            if (match.Success && double.TryParse(match.Groups[1].Value, out var ratio))
            {
                Console.WriteLine($"🔍 DEBUG: Parsed elite selection ratio: {ratio}");
                if (ratio >= 0.1 && ratio <= 0.9)
                {
                    var oldRatio = _defaultSettings.ABCSettings.EliteSelectionRatio;
                    _defaultSettings.ABCSettings.EliteSelectionRatio = ratio;
                    var change = $"Updated EliteSelectionRatio from {oldRatio:F3} to {ratio:F3} (from instruction)";
                    Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                    changes.Add(change);
                }
                else
                {
                    var errorMsg = $"Elite selection ratio {ratio} is out of range [0.1-0.9]. Value not updated.";
                    Console.WriteLine($"❌ PLAIN TEXT VALIDATION ERROR: {errorMsg}");
                    changes.Add($"FAILED: {errorMsg}");
                }
            }
            else
            {
                var errorMsg = "Could not parse elite selection ratio from instruction. Expected format: 'elite selection ratio to 0.3'";
                Console.WriteLine($"❌ PLAIN TEXT PARSE ERROR: {errorMsg}");
                changes.Add($"FAILED: {errorMsg}");
            }
        }

        // Total generations patterns
        if (instruction.Contains("total generations") || instruction.Contains("generations"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(instruction, @"(?:total )?generations.*?(?:to|=)\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var generations))
            {
                Console.WriteLine($"🔍 DEBUG: Parsed total generations: {generations}");
                if (generations >= 10 && generations <= 200)
                {
                    var oldGenerations = _defaultSettings.ABCSettings.TotalPopulationGenerations;
                    _defaultSettings.ABCSettings.TotalPopulationGenerations = generations;
                    var change = $"Updated TotalPopulationGenerations from {oldGenerations} to {generations} (from instruction)";
                    Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                    changes.Add(change);
                }
                else
                {
                    var errorMsg = $"Total generations {generations} is out of range [10-200]. Value not updated.";
                    Console.WriteLine($"❌ PLAIN TEXT VALIDATION ERROR: {errorMsg}");
                    changes.Add($"FAILED: {errorMsg}");
                }
            }
            else
            {
                var errorMsg = "Could not parse total generations from instruction. Expected format: 'total generations to 100'";
                Console.WriteLine($"❌ PLAIN TEXT PARSE ERROR: {errorMsg}");
                changes.Add($"FAILED: {errorMsg}");
            }
        }

        // Scout phase patterns
        if (instruction.Contains("scout phase"))
        {
            if (instruction.Contains("enable") || instruction.Contains("true"))
            {
                var oldValue = _defaultSettings.ABCSettings.EnableScoutPhase;
                _defaultSettings.ABCSettings.EnableScoutPhase = true;
                var change = $"Updated EnableScoutPhase from {oldValue} to true (from instruction)";
                Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                changes.Add(change);
            }
            else if (instruction.Contains("disable") || instruction.Contains("false"))
            {
                var oldValue = _defaultSettings.ABCSettings.EnableScoutPhase;
                _defaultSettings.ABCSettings.EnableScoutPhase = false;
                var change = $"Updated EnableScoutPhase from {oldValue} to false (from instruction)";
                Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                changes.Add(change);
            }
            else
            {
                var errorMsg = "Could not determine scout phase action from instruction. Expected 'enable scout phase' or 'disable scout phase'";
                Console.WriteLine($"❌ PLAIN TEXT PARSE ERROR: {errorMsg}");
                changes.Add($"FAILED: {errorMsg}");
            }
        }

        // Onlooker selection patterns
        if (instruction.Contains("onlooker selection") || instruction.Contains("onlooker"))
        {
            if (instruction.Contains("enable") || instruction.Contains("true"))
            {
                var oldValue = _defaultSettings.ABCSettings.EnableOnlookerSelection;
                _defaultSettings.ABCSettings.EnableOnlookerSelection = true;
                var change = $"Updated EnableOnlookerSelection from {oldValue} to true (from instruction)";
                Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                changes.Add(change);
            }
            else if (instruction.Contains("disable") || instruction.Contains("false"))
            {
                var oldValue = _defaultSettings.ABCSettings.EnableOnlookerSelection;
                _defaultSettings.ABCSettings.EnableOnlookerSelection = false;
                var change = $"Updated EnableOnlookerSelection from {oldValue} to false (from instruction)";
                Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                changes.Add(change);
            }
            else if (instruction.Contains("ratio"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(instruction, @"onlooker.*?ratio.*?(?:to|=)\s*([\d.]+)");
                if (match.Success && double.TryParse(match.Groups[1].Value, out var ratio))
                {
                    Console.WriteLine($"🔍 DEBUG: Parsed onlooker selection ratio: {ratio}");
                    if (ratio >= 0.05 && ratio <= 0.5)
                    {
                        var oldRatio = _defaultSettings.ABCSettings.OnlookerSelectionRatio;
                        _defaultSettings.ABCSettings.OnlookerSelectionRatio = ratio;
                        var change = $"Updated OnlookerSelectionRatio from {oldRatio:F3} to {ratio:F3} (from instruction)";
                        Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                        changes.Add(change);
                    }
                    else
                    {
                        var errorMsg = $"Onlooker selection ratio {ratio} is out of range [0.05-0.5]. Value not updated.";
                        Console.WriteLine($"❌ PLAIN TEXT VALIDATION ERROR: {errorMsg}");
                        changes.Add($"FAILED: {errorMsg}");
                    }
                }
                else
                {
                    var errorMsg = "Could not parse onlooker ratio from instruction. Expected format: 'onlooker ratio to 0.1'";
                    Console.WriteLine($"❌ PLAIN TEXT PARSE ERROR: {errorMsg}");
                    changes.Add($"FAILED: {errorMsg}");
                }
            }
        }

        // Seed patterns
        if (instruction.Contains("seed"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(instruction, @"seed.*?(?:to|=)\s*(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var seed))
            {
                var oldSeed = _defaultSettings.ABCSettings.Seed;
                _defaultSettings.ABCSettings.Seed = seed;
                var change = $"Updated Seed from {oldSeed} to {seed} (from instruction)";
                Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                changes.Add(change);
            }
            else
            {
                var errorMsg = "Could not parse seed from instruction. Expected format: 'seed to 12345'";
                Console.WriteLine($"❌ PLAIN TEXT PARSE ERROR: {errorMsg}");
                changes.Add($"FAILED: {errorMsg}");
            }
        }

        // Test case category patterns
        if (instruction.Contains("test case category") || instruction.Contains("category"))
        {
            if (instruction.Contains("all") || instruction.Contains("comprehensive"))
            {
                var oldCategory = _defaultSettings.TestCaseCategory;
                _defaultSettings.TestCaseCategory = TestCaseCategory.All;
                var change = $"Updated TestCaseCategory from {oldCategory} to All (from instruction)";
                Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                changes.Add(change);
            }
            else if (instruction.Contains("valid only") || instruction.Contains("valid"))
            {
                var oldCategory = _defaultSettings.TestCaseCategory;
                _defaultSettings.TestCaseCategory = TestCaseCategory.Valid;
                var change = $"Updated TestCaseCategory from {oldCategory} to Valid (from instruction)";
                Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                changes.Add(change);
            }
            else if (instruction.Contains("validation") || instruction.Contains("invalid"))
            {
                var oldCategory = _defaultSettings.TestCaseCategory;
                _defaultSettings.TestCaseCategory = TestCaseCategory.Validation;
                var change = $"Updated TestCaseCategory from {oldCategory} to Validation (from instruction)";
                Console.WriteLine($"✅ PLAIN TEXT SUCCESS: {change}");
                changes.Add(change);
            }
            else
            {
                var errorMsg = "Could not determine test case category from instruction. Expected: 'category to all', 'category to valid', or 'category to validation'";
                Console.WriteLine($"❌ PLAIN TEXT PARSE ERROR: {errorMsg}");
                changes.Add($"FAILED: {errorMsg}");
            }
        }

        Console.WriteLine($"🔍 DEBUG: Plain text instruction processing completed. {changes.Count} changes processed");
        return changes;
    }

    // 🔧 HELPER: Update ABC Settings object
    private List<string> UpdateABCSettings(JsonElement abcElement)
    {
        var changes = new List<string>();
        var abcSettings = JsonSerializer.Deserialize<ABCGenerationSettings>(abcElement.GetRawText(), JsonOptions);
        if (abcSettings != null)
        {
            _defaultSettings.ABCSettings = abcSettings;
            var change = $"Updated complete ABCSettings - Generations: {abcSettings.TotalPopulationGenerations}, MutationRate: {abcSettings.MutationRate}";
            Console.WriteLine($"⚙️ CONFIG: {change}");
            changes.Add(change);
        }
        return changes;
    }

    // 🔧 HELPER: Update individual ABC properties
    private void UpdateIndividualABCProperties(JsonElement argumentsElement, List<string> changesApplied)
    {
        var propertyMappings = new Dictionary<string, Action<JsonElement>>
        {
            ["totalPopulationGenerations"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing TotalPopulationGenerations - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.TryGetInt32(out var val)) {
                    Console.WriteLine($"🔍 DEBUG: Parsed TotalPopulationGenerations value: {val}");
                    if (val >= 10 && val <= 1000) {
                        var oldValue = _defaultSettings.ABCSettings.TotalPopulationGenerations;
                        _defaultSettings.ABCSettings.TotalPopulationGenerations = val;
                        var change = $"Updated TotalPopulationGenerations from {oldValue} to {val}";
                        Console.WriteLine($"✅ SUCCESS: {change}");
                        changesApplied.Add(change);
                    } else {
                        var errorMsg = $"TotalPopulationGenerations value {val} is out of range [10-1000]. Value not updated.";
                        Console.WriteLine($"❌ VALIDATION ERROR: {errorMsg}");
                        changesApplied.Add($"FAILED: {errorMsg}");
                    }
                } else {
                    var errorMsg = $"Failed to parse TotalPopulationGenerations from '{e.GetRawText()}' - expected integer";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["mutationRate"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing MutationRate - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.TryGetDouble(out var val)) {
                    Console.WriteLine($"🔍 DEBUG: Parsed MutationRate value: {val}");
                    if (val >= 0.1 && val <= 1) {
                        var oldValue = _defaultSettings.ABCSettings.MutationRate;
                        _defaultSettings.ABCSettings.MutationRate = val;
                        var change = $"Updated MutationRate from {oldValue:F3} to {val:F3}";
                        Console.WriteLine($"✅ SUCCESS: {change}");
                        changesApplied.Add(change);
                    } else {
                        var errorMsg = $"MutationRate value {val} is out of range [0.1-1.0]. Value not updated.";
                        Console.WriteLine($"❌ VALIDATION ERROR: {errorMsg}");
                        changesApplied.Add($"FAILED: {errorMsg}");
                    }
                } else {
                    var errorMsg = $"Failed to parse MutationRate from '{e.GetRawText()}' - expected number";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["eliteSelectionRatio"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing EliteSelectionRatio - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.TryGetDouble(out var val)) {
                    Console.WriteLine($"🔍 DEBUG: Parsed EliteSelectionRatio value: {val}");
                    if (val >= 0.1 && val <= 1) {
                        var oldValue = _defaultSettings.ABCSettings.EliteSelectionRatio;
                        _defaultSettings.ABCSettings.EliteSelectionRatio = val;
                        var change = $"Updated EliteSelectionRatio from {oldValue:F3} to {val:F3}";
                        Console.WriteLine($"✅ SUCCESS: {change}");
                        changesApplied.Add(change);
                    } else {
                        var errorMsg = $"EliteSelectionRatio value {val} is out of range [0.1-1.0]. Value not updated.";
                        Console.WriteLine($"❌ VALIDATION ERROR: {errorMsg}");
                        changesApplied.Add($"FAILED: {errorMsg}");
                    }
                } else {
                    var errorMsg = $"Failed to parse EliteSelectionRatio from '{e.GetRawText()}' - expected number";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["onlookerSelectionRatio"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing OnlookerSelectionRatio - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.TryGetDouble(out var val)) {
                    Console.WriteLine($"🔍 DEBUG: Parsed OnlookerSelectionRatio value: {val}");
                    if (val >= 0.05 && val <= 1) {
                        var oldValue = _defaultSettings.ABCSettings.OnlookerSelectionRatio;
                        _defaultSettings.ABCSettings.OnlookerSelectionRatio = val;
                        var change = $"Updated OnlookerSelectionRatio from {oldValue:F3} to {val:F3}";
                        Console.WriteLine($"✅ SUCCESS: {change}");
                        changesApplied.Add(change);
                    } else {
                        var errorMsg = $"OnlookerSelectionRatio value {val} is out of range [0.05-1.0]. Value not updated.";
                        Console.WriteLine($"❌ VALIDATION ERROR: {errorMsg}");
                        changesApplied.Add($"FAILED: {errorMsg}");
                    }
                } else {
                    var errorMsg = $"Failed to parse OnlookerSelectionRatio from '{e.GetRawText()}' - expected number";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["scoutSelectionRatio"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing ScoutSelectionRatio - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.TryGetDouble(out var val)) {
                    Console.WriteLine($"🔍 DEBUG: Parsed ScoutSelectionRatio value: {val}");
                    if (val >= 0.1 && val <= 1) {
                        var oldValue = _defaultSettings.ABCSettings.ScoutSelectionRatio;
                        _defaultSettings.ABCSettings.ScoutSelectionRatio = val;
                        var change = $"Updated ScoutSelectionRatio from {oldValue:F3} to {val:F3}";
                        Console.WriteLine($"✅ SUCCESS: {change}");
                        changesApplied.Add(change);
                    } else {
                        var errorMsg = $"ScoutSelectionRatio value {val} is out of range [0.1-1.0]. Value not updated.";
                        Console.WriteLine($"❌ VALIDATION ERROR: {errorMsg}");
                        changesApplied.Add($"FAILED: {errorMsg}");
                    }
                } else {
                    var errorMsg = $"Failed to parse ScoutSelectionRatio from '{e.GetRawText()}' - expected number";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["finalPopulationSelectionRatio"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing FinalPopulationSelectionRatio - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.TryGetDouble(out var val)) {
                    Console.WriteLine($"🔍 DEBUG: Parsed FinalPopulationSelectionRatio value: {val}");
                    if (val >= 0.1 && val <= 1.0) {
                        var oldValue = _defaultSettings.ABCSettings.FinalPopulationSelectionRatio;
                        _defaultSettings.ABCSettings.FinalPopulationSelectionRatio = val;
                        var change = $"Updated FinalPopulationSelectionRatio from {oldValue:F3} to {val:F3}";
                        Console.WriteLine($"✅ SUCCESS: {change}");
                        changesApplied.Add(change);
                    } else {
                        var errorMsg = $"FinalPopulationSelectionRatio value {val} is out of range [0.1-1.0]. Value not updated.";
                        Console.WriteLine($"❌ VALIDATION ERROR: {errorMsg}");
                        changesApplied.Add($"FAILED: {errorMsg}");
                    }
                } else {
                    var errorMsg = $"Failed to parse FinalPopulationSelectionRatio from '{e.GetRawText()}' - expected number";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["stagnationThresholdPercentage"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing StagnationThresholdPercentage - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.TryGetDouble(out var val)) {
                    Console.WriteLine($"🔍 DEBUG: Parsed StagnationThresholdPercentage value: {val}");
                    if (val >= 0.5 && val <= 1.0) {
                        var oldValue = _defaultSettings.ABCSettings.StagnationThresholdPercentage;
                        _defaultSettings.ABCSettings.StagnationThresholdPercentage = val;
                        var change = $"Updated StagnationThresholdPercentage from {oldValue:F3} to {val:F3}";
                        Console.WriteLine($"✅ SUCCESS: {change}");
                        changesApplied.Add(change);
                    } else {
                        var errorMsg = $"StagnationThresholdPercentage value {val} is out of range [0.5-1.0]. Value not updated.";
                        Console.WriteLine($"❌ VALIDATION ERROR: {errorMsg}");
                        changesApplied.Add($"FAILED: {errorMsg}");
                    }
                } else {
                    var errorMsg = $"Failed to parse StagnationThresholdPercentage from '{e.GetRawText()}' - expected number";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["coolingRate"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing CoolingRate - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.TryGetDouble(out var val)) {
                    Console.WriteLine($"🔍 DEBUG: Parsed CoolingRate value: {val}");
                    if (val >= 0.8 && val <= 1) {
                        var oldValue = _defaultSettings.ABCSettings.CoolingRate;
                        _defaultSettings.ABCSettings.CoolingRate = val;
                        var change = $"Updated CoolingRate from {oldValue:F3} to {val:F3}";
                        Console.WriteLine($"✅ SUCCESS: {change}");
                        changesApplied.Add(change);
                    } else {
                        var errorMsg = $"CoolingRate value {val} is out of range [0.8-1.0]. Value not updated.";
                        Console.WriteLine($"❌ VALIDATION ERROR: {errorMsg}");
                        changesApplied.Add($"FAILED: {errorMsg}");
                    }
                } else {
                    var errorMsg = $"Failed to parse CoolingRate from '{e.GetRawText()}' - expected number";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["allowMultipleInvalidInputs"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing AllowMultipleInvalidInputs - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.ValueKind == JsonValueKind.True || e.ValueKind == JsonValueKind.False) {
                    var val = e.GetBoolean();
                    var oldValue = _defaultSettings.ABCSettings.AllowMultipleInvalidInputs;
                    _defaultSettings.ABCSettings.AllowMultipleInvalidInputs = val;
                    var change = $"Updated AllowMultipleInvalidInputs from {oldValue} to {val}";
                    Console.WriteLine($"✅ SUCCESS: {change}");
                    changesApplied.Add(change);
                } else {
                    var errorMsg = $"Failed to parse AllowMultipleInvalidInputs from '{e.GetRawText()}' - expected boolean (true/false)";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["enforceMutationUniqueness"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing EnforceMutationUniqueness - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.ValueKind == JsonValueKind.True || e.ValueKind == JsonValueKind.False) {
                    var val = e.GetBoolean();
                    var oldValue = _defaultSettings.ABCSettings.EnforceMutationUniqueness;
                    _defaultSettings.ABCSettings.EnforceMutationUniqueness = val;
                    var change = $"Updated EnforceMutationUniqueness from {oldValue} to {val}";
                    Console.WriteLine($"✅ SUCCESS: {change}");
                    changesApplied.Add(change);
                } else {
                    var errorMsg = $"Failed to parse EnforceMutationUniqueness from '{e.GetRawText()}' - expected boolean (true/false)";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["seed"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing Seed - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.TryGetInt32(out var val)) {
                    Console.WriteLine($"🔍 DEBUG: Parsed Seed value: {val}");
                    var oldValue = _defaultSettings.ABCSettings.Seed;
                    _defaultSettings.ABCSettings.Seed = val;
                    var change = $"Updated Seed from {oldValue} to {val}";
                    Console.WriteLine($"✅ SUCCESS: {change}");
                    changesApplied.Add(change);
                } else {
                    var errorMsg = $"Failed to parse Seed from '{e.GetRawText()}' - expected integer";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["enableScoutPhase"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing EnableScoutPhase - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.ValueKind == JsonValueKind.True || e.ValueKind == JsonValueKind.False) {
                    var val = e.GetBoolean();
                    var oldValue = _defaultSettings.ABCSettings.EnableScoutPhase;
                    _defaultSettings.ABCSettings.EnableScoutPhase = val;
                    var change = $"Updated EnableScoutPhase from {oldValue} to {val}";
                    Console.WriteLine($"✅ SUCCESS: {change}");
                    changesApplied.Add(change);
                } else {
                    var errorMsg = $"Failed to parse EnableScoutPhase from '{e.GetRawText()}' - expected boolean (true/false)";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            },
            ["enableOnlookerSelection"] = e => {
                Console.WriteLine($"🔍 DEBUG: Processing EnableOnlookerSelection - ValueKind: {e.ValueKind}, RawValue: '{e.GetRawText()}'");
                if (e.ValueKind == JsonValueKind.True || e.ValueKind == JsonValueKind.False) {
                    var val = e.GetBoolean();
                    var oldValue = _defaultSettings.ABCSettings.EnableOnlookerSelection;
                    _defaultSettings.ABCSettings.EnableOnlookerSelection = val;
                    var change = $"Updated EnableOnlookerSelection from {oldValue} to {val}";
                    Console.WriteLine($"✅ SUCCESS: {change}");
                    changesApplied.Add(change);
                } else {
                    var errorMsg = $"Failed to parse EnableOnlookerSelection from '{e.GetRawText()}' - expected boolean (true/false)";
                    Console.WriteLine($"❌ PARSE ERROR: {errorMsg}");
                    changesApplied.Add($"FAILED: {errorMsg}");
                }
            }
        };

        // Debug: List all properties received in the request
        Console.WriteLine($"🔍 DEBUG: Processing individual ABC property updates. Properties in request:");
        foreach (var prop in argumentsElement.EnumerateObject())
        {
            Console.WriteLine($"  - {prop.Name}: {prop.Value.GetRawText()} (ValueKind: {prop.Value.ValueKind})");
        }

        // Process each property mapping
        foreach (var mapping in propertyMappings)
        {
            if (argumentsElement.TryGetProperty(mapping.Key, out var element))
            {
                Console.WriteLine($"🔧 CONFIG: Found property '{mapping.Key}' - processing...");
                mapping.Value(element);
            }
            else
            {
                Console.WriteLine($"🔍 DEBUG: Property '{mapping.Key}' not found in request");
            }
        }

        // Check for unknown properties
        foreach (var prop in argumentsElement.EnumerateObject())
        {
            if (!propertyMappings.ContainsKey(prop.Name.ToLowerInvariant()) && 
                !new[] { "instruction", "testcasecategory", "methodname", "abcsettings" }.Contains(prop.Name.ToLowerInvariant()))
            {
                var warningMsg = $"Unknown property '{prop.Name}' ignored. Valid ABC properties: {string.Join(", ", propertyMappings.Keys)}";
                Console.WriteLine($"⚠️ WARNING: {warningMsg}");
                changesApplied.Add($"WARNING: {warningMsg}");
            }
        }

        Console.WriteLine($"🔍 DEBUG: Completed processing individual ABC properties. Total changes applied: {changesApplied.Count(c => !c.StartsWith("FAILED") && !c.StartsWith("WARNING"))}");
    }

    // 🔧 HELPER: Get current settings object
    private object GetCurrentSettingsObject()
    {
        return new
        {
            TestCaseCategory = _defaultSettings.TestCaseCategory,
            MethodName = _defaultSettings.MethodName,
            ABCSettings = new
            {
                _defaultSettings.ABCSettings.TotalPopulationGenerations,
                _defaultSettings.ABCSettings.MutationRate,
                _defaultSettings.ABCSettings.FinalPopulationSelectionRatio,
                _defaultSettings.ABCSettings.EliteSelectionRatio,
                _defaultSettings.ABCSettings.OnlookerSelectionRatio,
                _defaultSettings.ABCSettings.ScoutSelectionRatio,
                _defaultSettings.ABCSettings.EnableOnlookerSelection,
                _defaultSettings.ABCSettings.EnableScoutPhase,
                _defaultSettings.ABCSettings.EnforceMutationUniqueness,
                _defaultSettings.ABCSettings.StagnationThresholdPercentage,
                _defaultSettings.ABCSettings.CoolingRate,
                _defaultSettings.ABCSettings.AllowMultipleInvalidInputs,
                _defaultSettings.ABCSettings.Seed
            }
        };
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