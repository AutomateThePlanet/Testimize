using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Testimize;
using Testimize.Parameters;
using Testimize.Parameters.Core;
using Testimize.Usage;
using Testimize.Contracts;
using Testimize.OutputGenerators;
using Testimize.MCP.Server.Services;

namespace Testimize.MCP.Server.Services
{
    public class ABCSettingsService
    {
        private readonly PreciseTestEngineSettings _defaultSettings;

        // Shared JsonSerializerOptions for consistent deserialization
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        public ABCSettingsService(PreciseTestEngineSettings defaultSettings)
        {
            _defaultSettings = defaultSettings;
        }

        /// <summary>
        /// Comprehensive settings configuration with plain text support
        /// </summary>
        public object ConfigureTestimizeSettings(JsonElement argumentsElement)
        {
            Console.WriteLine("?? CONFIG: Updating Testimize default settings...");
            
            var changesApplied = new List<string>();
            var errors = new List<string>();
            var warnings = new List<string>();

            Console.WriteLine($"?? DEBUG: Raw input JSON: {argumentsElement.GetRawText()}");

            // Support for plain text instructions
            if (argumentsElement.TryGetProperty("instruction", out var instructionElement))
            {
                var instruction = instructionElement.GetString()?.ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(instruction))
                {
                    Console.WriteLine($"?? CONFIG: Processing plain text instruction: '{instruction}'");
                    var changes = ProcessPlainTextInstruction(instruction);
                    changesApplied.AddRange(changes);
                }
            }

            // Update TestCaseCategory if provided
            if (argumentsElement.TryGetProperty("testCaseCategory", out var categoryElement))
            {
                Console.WriteLine($"?? DEBUG: Processing TestCaseCategory - ValueKind: {categoryElement.ValueKind}, RawValue: '{categoryElement.GetRawText()}'");
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
                        Console.WriteLine($"? SUCCESS: {change}");
                        changesApplied.Add(change);
                    }
                    else
                    {
                        var errorMsg = $"TestCaseCategory value {category} is invalid. Valid values: 0 (All), 1 (Valid), 2 (Validation)";
                        Console.WriteLine($"? VALIDATION ERROR: {errorMsg}");
                        errors.Add(errorMsg);
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Failed to parse TestCaseCategory from '{categoryElement.GetRawText()}': {ex.Message}";
                    Console.WriteLine($"? PARSE ERROR: {errorMsg}");
                    errors.Add(errorMsg);
                }
            }

            // Update MethodName if provided
            if (argumentsElement.TryGetProperty("methodName", out var methodNameElement))
            {
                Console.WriteLine($"?? DEBUG: Processing MethodName - ValueKind: {methodNameElement.ValueKind}, RawValue: '{methodNameElement.GetRawText()}'");
                var methodName = methodNameElement.GetString();
                if (!string.IsNullOrWhiteSpace(methodName))
                {
                    var oldMethodName = _defaultSettings.MethodName;
                    _defaultSettings.MethodName = methodName;
                    var change = $"Updated MethodName from '{oldMethodName}' to '{_defaultSettings.MethodName}'";
                    Console.WriteLine($"? SUCCESS: {change}");
                    changesApplied.Add(change);
                }
                else
                {
                    var warningMsg = "MethodName is empty or whitespace - ignoring";
                    Console.WriteLine($"?? WARNING: {warningMsg}");
                    warnings.Add(warningMsg);
                }
            }

            // Update individual ABCSettings properties if provided
            if (argumentsElement.TryGetProperty("abcSettings", out var abcElement))
            {
                Console.WriteLine($"?? DEBUG: Processing complete ABCSettings object");
                var abcChanges = UpdateABCSettings(abcElement);
                changesApplied.AddRange(abcChanges);
            }

            // Support for individual ABC property updates
            Console.WriteLine($"?? DEBUG: Processing individual ABC properties...");
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
                messageParts.Add($"? {successfulChanges.Count} setting(s) updated successfully");
            }
            
            if (errors.Count > 0)
            {
                messageParts.Add($"? {errors.Count} error(s) occurred");
            }
            
            if (warnings.Count > 0)
            {
                messageParts.Add($"?? {warnings.Count} warning(s)");
            }

            if (messageParts.Count == 0)
            {
                messageParts.Add("No changes were requested");
            }

            var message = string.Join(", ", messageParts);

            Console.WriteLine($"?? CONFIG SUMMARY: {message}");

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

        /// <summary>
        /// Get current Testimize settings
        /// </summary>
        public object GetTestimizeSettings()
        {
            Console.WriteLine("?? VIEW: Retrieving current Testimize settings...");
            
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

        public List<string> UpdateABCSettings(JsonElement abcElement)
        {
            var changes = new List<string>();
            var abcSettings = JsonSerializer.Deserialize<ABCGenerationSettings>(abcElement.GetRawText(), JsonOptions);
            if (abcSettings != null)
            {
                _defaultSettings.ABCSettings = abcSettings;
                var change = $"Updated complete ABCSettings - Generations: {abcSettings.TotalPopulationGenerations}, MutationRate: {abcSettings.MutationRate}";
                Console.WriteLine($"?? CONFIG: {change}");
                changes.Add(change);
            }
            else
            {
                changes.Add("FAILED: Unable to deserialize ABCSettings.");
            }
            return changes;
        }

        // ?? HELPER: Process plain text instructions
        private List<string> ProcessPlainTextInstruction(string instruction)
        {
            var changes = new List<string>();
            Console.WriteLine($"?? DEBUG: Processing plain text instruction: '{instruction}'");
            
            // Method name patterns
            if (instruction.Contains("method name") || instruction.Contains("methodname"))
            {
                var match = Regex.Match(instruction, @"(?:method name|methodname).*?(?:to|=)\s*([a-zA-Z_][a-zA-Z0-9_]*)");
                if (match.Success)
                {
                    var newName = match.Groups[1].Value;
                    var oldName = _defaultSettings.MethodName;
                    _defaultSettings.MethodName = newName;
                    var change = $"Updated MethodName from '{oldName}' to '{newName}' (from instruction)";
                    Console.WriteLine($"? PLAIN TEXT SUCCESS: {change}");
                    changes.Add(change);
                }
                else
                {
                    var errorMsg = "Could not parse method name from instruction. Expected format: 'method name to SomeName'";
                    Console.WriteLine($"? PLAIN TEXT PARSE ERROR: {errorMsg}");
                    changes.Add($"FAILED: {errorMsg}");
                }
            }

            // Mutation rate patterns
            if (instruction.Contains("mutation rate"))
            {
                var match = Regex.Match(instruction, @"mutation rate.*?(?:to|=)\s*([\d.]+)");
                if (match.Success && double.TryParse(match.Groups[1].Value, out var rate))
                {
                    Console.WriteLine($"?? DEBUG: Parsed mutation rate: {rate}");
                    if (rate >= 0.1 && rate <= 0.9)
                    {
                        var oldRate = _defaultSettings.ABCSettings.MutationRate;
                        _defaultSettings.ABCSettings.MutationRate = rate;
                        var change = $"Updated MutationRate from {oldRate:F3} to {rate:F3} (from instruction)";
                        Console.WriteLine($"? PLAIN TEXT SUCCESS: {change}");
                        changes.Add(change);
                    }
                    else
                    {
                        var errorMsg = $"Mutation rate {rate} is out of range [0.1-0.9]. Value not updated.";
                        Console.WriteLine($"? PLAIN TEXT VALIDATION ERROR: {errorMsg}");
                        changes.Add($"FAILED: {errorMsg}");
                    }
                }
                else
                {
                    var errorMsg = "Could not parse mutation rate from instruction. Expected format: 'mutation rate to 0.5'";
                    Console.WriteLine($"? PLAIN TEXT PARSE ERROR: {errorMsg}");
                    changes.Add($"FAILED: {errorMsg}");
                }
            }

            // Elite selection ratio patterns
            if (instruction.Contains("elite selection") || instruction.Contains("elite ratio"))
            {
                var match = Regex.Match(instruction, @"(?:elite selection|elite ratio).*?(?:to|=)\s*([\d.]+)");
                if (match.Success && double.TryParse(match.Groups[1].Value, out var ratio))
                {
                    Console.WriteLine($"?? DEBUG: Parsed elite selection ratio: {ratio}");
                    if (ratio >= 0.1 && ratio <= 0.9)
                    {
                        var oldRatio = _defaultSettings.ABCSettings.EliteSelectionRatio;
                        _defaultSettings.ABCSettings.EliteSelectionRatio = ratio;
                        var change = $"Updated EliteSelectionRatio from {oldRatio:F3} to {ratio:F3} (from instruction)";
                        Console.WriteLine($"? PLAIN TEXT SUCCESS: {change}");
                        changes.Add(change);
                    }
                    else
                    {
                        var errorMsg = $"Elite selection ratio {ratio} is out of range [0.1-0.9]. Value not updated.";
                        Console.WriteLine($"? PLAIN TEXT VALIDATION ERROR: {errorMsg}");
                        changes.Add($"FAILED: {errorMsg}");
                    }
                }
                else
                {
                    var errorMsg = "Could not parse elite selection ratio from instruction. Expected format: 'elite selection ratio to 0.3'";
                    Console.WriteLine($"? PLAIN TEXT PARSE ERROR: {errorMsg}");
                    changes.Add($"FAILED: {errorMsg}");
                }
            }

            // Total generations patterns
            if (instruction.Contains("total generations") || instruction.Contains("generations"))
            {
                var match = Regex.Match(instruction, @"(?:total )?generations.*?(?:to|=)\s*(\d+)", RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var generations))
                {
                    Console.WriteLine($"?? DEBUG: Parsed total generations: {generations}");
                    if (generations >= 10 && generations <= 200)
                    {
                        var oldGenerations = _defaultSettings.ABCSettings.TotalPopulationGenerations;
                        _defaultSettings.ABCSettings.TotalPopulationGenerations = generations;
                        var change = $"Updated TotalPopulationGenerations from {oldGenerations} to {generations} (from instruction)";
                        Console.WriteLine($"? PLAIN TEXT SUCCESS: {change}");
                        changes.Add(change);
                    }
                    else
                    {
                        var errorMsg = $"Total generations {generations} is out of range [10-200]. Value not updated.";
                        Console.WriteLine($"? PLAIN TEXT VALIDATION ERROR: {errorMsg}");
                        changes.Add($"FAILED: {errorMsg}");
                    }
                }
                else
                {
                    var errorMsg = "Could not parse total generations from instruction. Expected format: 'total generations to 100'";
                    Console.WriteLine($"? PLAIN TEXT PARSE ERROR: {errorMsg}");
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
                    Console.WriteLine($"? PLAIN TEXT SUCCESS: {change}");
                    changes.Add(change);
                }
                else if (instruction.Contains("disable") || instruction.Contains("false"))
                {
                    var oldValue = _defaultSettings.ABCSettings.EnableScoutPhase;
                    _defaultSettings.ABCSettings.EnableScoutPhase = false;
                    var change = $"Updated EnableScoutPhase from {oldValue} to false (from instruction)";
                    Console.WriteLine($"? PLAIN TEXT SUCCESS: {change}");
                    changes.Add(change);
                }
                else
                {
                    var errorMsg = "Could not determine scout phase action from instruction. Expected 'enable scout phase' or 'disable scout phase'";
                    Console.WriteLine($"? PLAIN TEXT PARSE ERROR: {errorMsg}");
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
                    Console.WriteLine($"? PLAIN TEXT SUCCESS: {change}");
                    changes.Add(change);
                }
                else if (instruction.Contains("valid only") || instruction.Contains("valid"))
                {
                    var oldCategory = _defaultSettings.TestCaseCategory;
                    _defaultSettings.TestCaseCategory = TestCaseCategory.Valid;
                    var change = $"Updated TestCaseCategory from {oldCategory} to Valid (from instruction)";
                    Console.WriteLine($"? PLAIN TEXT SUCCESS: {change}");
                    changes.Add(change);
                }
                else if (instruction.Contains("validation") || instruction.Contains("invalid"))
                {
                    var oldCategory = _defaultSettings.TestCaseCategory;
                    _defaultSettings.TestCaseCategory = TestCaseCategory.Validation;
                    var change = $"Updated TestCaseCategory from {oldCategory} to Validation (from instruction)";
                    Console.WriteLine($"? PLAIN TEXT SUCCESS: {change}");
                    changes.Add(change);
                }
                else
                {
                    var errorMsg = "Could not determine test case category from instruction. Expected: 'category to all', 'category to valid', or 'category to validation'";
                    Console.WriteLine($"? PLAIN TEXT PARSE ERROR: {errorMsg}");
                    changes.Add($"FAILED: {errorMsg}");
                }
            }

            Console.WriteLine($"?? DEBUG: Plain text instruction processing completed. {changes.Count} changes processed");
            return changes;
        }

        // ?? HELPER: Update individual ABC properties
        private void UpdateIndividualABCProperties(JsonElement argumentsElement, List<string> changesApplied)
        {
            var propertyMappings = new Dictionary<string, Action<JsonElement>>
            {
                ["totalPopulationGenerations"] = e => {
                    if (e.TryGetInt32(out var val)) {
                        if (val >= 10 && val <= 1000) {
                            var oldValue = _defaultSettings.ABCSettings.TotalPopulationGenerations;
                            _defaultSettings.ABCSettings.TotalPopulationGenerations = val;
                            changesApplied.Add($"Updated TotalPopulationGenerations from {oldValue} to {val}");
                        } else {
                            changesApplied.Add($"FAILED: TotalPopulationGenerations value {val} is out of range [10-1000].");
                        }
                    } else {
                        changesApplied.Add($"FAILED: Failed to parse TotalPopulationGenerations.");
                    }
                },
                ["mutationRate"] = e => {
                    if (e.TryGetDouble(out var val)) {
                        if (val >= 0.1 && val <= 0.9) {
                            var oldValue = _defaultSettings.ABCSettings.MutationRate;
                            _defaultSettings.ABCSettings.MutationRate = val;
                            changesApplied.Add($"Updated MutationRate from {oldValue:F3} to {val:F3}");
                        } else {
                            changesApplied.Add($"FAILED: MutationRate value {val} is out of range [0.1-0.9].");
                        }
                    } else {
                        changesApplied.Add($"FAILED: Failed to parse MutationRate.");
                    }
                },
                ["eliteSelectionRatio"] = e => {
                    if (e.TryGetDouble(out var val)) {
                        if (val >= 0.1 && val <= 0.9) {
                            var oldValue = _defaultSettings.ABCSettings.EliteSelectionRatio;
                            _defaultSettings.ABCSettings.EliteSelectionRatio = val;
                            changesApplied.Add($"Updated EliteSelectionRatio from {oldValue:F3} to {val:F3}");
                        } else {
                            changesApplied.Add($"FAILED: EliteSelectionRatio value {val} is out of range [0.1-0.9].");
                        }
                    } else {
                        changesApplied.Add($"FAILED: Failed to parse EliteSelectionRatio.");
                    }
                }
            };

            // Process each property mapping
            foreach (var mapping in propertyMappings)
            {
                if (argumentsElement.TryGetProperty(mapping.Key, out var element))
                {
                    mapping.Value(element);
                }
            }

            // Check if there's an ABCSettings object and process its nested properties
            if (argumentsElement.TryGetProperty("ABCSettings", out var abcSettingsElement))
            {
                foreach (var prop in abcSettingsElement.EnumerateObject())
                {
                    if (propertyMappings.TryGetValue(prop.Name, out var action))
                    {
                        action(prop.Value);
                    }
                    else
                    {
                        changesApplied.Add($"WARNING: Unknown property '{prop.Name}' in ABCSettings.");
                    }
                }
            }
        }

        // ?? HELPER: Get current settings object
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
    }
}