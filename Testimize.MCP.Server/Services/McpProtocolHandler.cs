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
            var category = categoryElement.GetInt32();
            _defaultSettings.TestCaseCategory = (TestCaseCategory)category;
            var categoryName = category switch
            {
                0 => "All (comprehensive testing)",
                1 => "Valid only",
                2 => "Validation/Invalid only",
                _ => category.ToString()
            };
            var change = $"Updated TestCaseCategory to {categoryName}";
            Console.WriteLine($"⚙️ CONFIG: {change}");
            changesApplied.Add(change);
        }

        // Update MethodName if provided
        if (argumentsElement.TryGetProperty("methodName", out var methodNameElement))
        {
            var methodName = methodNameElement.GetString();
            if (!string.IsNullOrWhiteSpace(methodName))
            {
                _defaultSettings.MethodName = methodName;
                var change = $"Updated default MethodName to '{_defaultSettings.MethodName}'";
                Console.WriteLine($"⚙️ CONFIG: {change}");
                changesApplied.Add(change);
            }
        }

        // Update individual ABCSettings properties if provided
        if (argumentsElement.TryGetProperty("abcSettings", out var abcElement))
        {
            var abcChanges = UpdateABCSettings(abcElement);
            changesApplied.AddRange(abcChanges);
        }

        // Support for individual ABC property updates
        UpdateIndividualABCProperties(argumentsElement, changesApplied);

        var message = changesApplied.Count > 0 
            ? $"Settings updated successfully. Changes applied: {string.Join(", ", changesApplied)}"
            : "No changes were applied. Current settings returned.";

        return new 
        { 
            message,
            changesApplied,
            currentSettings = GetCurrentSettingsObject()
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
        
        // Method name patterns
        if (instruction.Contains("method name") || instruction.Contains("methodname"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(instruction, @"(?:method name|methodname).*?(?:to|=)\s*([a-zA-Z_][a-zA-Z0-9_]*)");
            if (match.Success)
            {
                var newName = match.Groups[1].Value;
                _defaultSettings.MethodName = newName;
                var change = $"Updated MethodName to '{newName}' (from instruction)";
                Console.WriteLine($"⚙️ PLAIN TEXT: {change}");
                changes.Add(change);
            }
        }

        // Mutation rate patterns
        if (instruction.Contains("mutation rate"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(instruction, @"mutation rate.*?(?:to|=)\s*([\d.]+)");
            if (match.Success && double.TryParse(match.Groups[1].Value, out var rate) && rate >= 0.1 && rate <= 0.9)
            {
                _defaultSettings.ABCSettings.MutationRate = rate;
                var change = $"Updated MutationRate to {rate} (from instruction)";
                Console.WriteLine($"⚙️ PLAIN TEXT: {change}");
                changes.Add(change);
            }
        }

        // Total generations patterns
        if (instruction.Contains("total generations") || instruction.Contains("generations"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(instruction, @"(?:total )?generations.*?(?:to|=)\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var generations) && generations >= 10 && generations <= 200)
            {
                _defaultSettings.ABCSettings.TotalPopulationGenerations = generations;
                var change = $"Updated TotalPopulationGenerations to {generations} (from instruction)";
                Console.WriteLine($"⚙️ PLAIN TEXT: {change}");
                changes.Add(change);
            }
        }

        // Scout phase patterns
        if (instruction.Contains("scout phase"))
        {
            if (instruction.Contains("enable") || instruction.Contains("true"))
            {
                _defaultSettings.ABCSettings.EnableScoutPhase = true;
                var change = "Enabled ScoutPhase (from instruction)";
                Console.WriteLine($"⚙️ PLAIN TEXT: {change}");
                changes.Add(change);
            }
            else if (instruction.Contains("disable") || instruction.Contains("false"))
            {
                _defaultSettings.ABCSettings.EnableScoutPhase = false;
                var change = "Disabled ScoutPhase (from instruction)";
                Console.WriteLine($"⚙️ PLAIN TEXT: {change}");
                changes.Add(change);
            }
        }

        // Seed patterns
        if (instruction.Contains("seed"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(instruction, @"seed.*?(?:to|=)\s*(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var seed))
            {
                _defaultSettings.ABCSettings.Seed = seed;
                var change = $"Updated Seed to {seed} (from instruction)";
                Console.WriteLine($"⚙️ PLAIN TEXT: {change}");
                changes.Add(change);
            }
        }

        // Test case category patterns
        if (instruction.Contains("test case category") || instruction.Contains("category"))
        {
            if (instruction.Contains("all") || instruction.Contains("comprehensive"))
            {
                _defaultSettings.TestCaseCategory = TestCaseCategory.All;
                var change = "Updated TestCaseCategory to All (from instruction)";
                Console.WriteLine($"⚙️ PLAIN TEXT: {change}");
                changes.Add(change);
            }
            else if (instruction.Contains("valid only") || instruction.Contains("valid"))
            {
                _defaultSettings.TestCaseCategory = TestCaseCategory.Valid;
                var change = "Updated TestCaseCategory to Valid (from instruction)";
                Console.WriteLine($"⚙️ PLAIN TEXT: {change}");
                changes.Add(change);
            }
            else if (instruction.Contains("validation") || instruction.Contains("invalid"))
            {
                _defaultSettings.TestCaseCategory = TestCaseCategory.Validation;
                var change = "Updated TestCaseCategory to Validation (from instruction)";
                Console.WriteLine($"⚙️ PLAIN TEXT: {change}");
                changes.Add(change);
            }
        }

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
                if (e.TryGetInt32(out var val) && val >= 10 && val <= 200) {
                    _defaultSettings.ABCSettings.TotalPopulationGenerations = val;
                    changesApplied.Add($"Updated TotalPopulationGenerations to {val}");
                }
            },
            ["mutationRate"] = e => {
                if (e.TryGetDouble(out var val) && val >= 0.1 && val <= 0.9) {
                    _defaultSettings.ABCSettings.MutationRate = val;
                    changesApplied.Add($"Updated MutationRate to {val}");
                }
            },
            ["finalPopulationSelectionRatio"] = e => {
                if (e.TryGetDouble(out var val) && val >= 0.1 && val <= 1.0) {
                    _defaultSettings.ABCSettings.FinalPopulationSelectionRatio = val;
                    changesApplied.Add($"Updated FinalPopulationSelectionRatio to {val}");
                }
            },
            ["seed"] = e => {
                if (e.TryGetInt32(out var val)) {
                    _defaultSettings.ABCSettings.Seed = val;
                    changesApplied.Add($"Updated Seed to {val}");
                }
            },
            ["enableScoutPhase"] = e => {
                if (e.ValueKind == JsonValueKind.True || e.ValueKind == JsonValueKind.False) {
                    var val = e.GetBoolean();
                    _defaultSettings.ABCSettings.EnableScoutPhase = val;
                    changesApplied.Add($"Updated EnableScoutPhase to {val}");
                }
            },
            ["enableOnlookerSelection"] = e => {
                if (e.ValueKind == JsonValueKind.True || e.ValueKind == JsonValueKind.False) {
                    var val = e.GetBoolean();
                    _defaultSettings.ABCSettings.EnableOnlookerSelection = val;
                    changesApplied.Add($"Updated EnableOnlookerSelection to {val}");
                }
            }
        };

        foreach (var mapping in propertyMappings)
        {
            if (argumentsElement.TryGetProperty(mapping.Key, out var element))
            {
                mapping.Value(element);
                Console.WriteLine($"⚙️ CONFIG: Applied individual property update for {mapping.Key}");
            }
        }
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