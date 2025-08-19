// Business logic implementation
using Testimize;
using Testimize.Contracts;
using Testimize.OutputGenerators;
using Testimize.Parameters.Core;
using Testimize.Usage;

public class UtilityService : IUtilityService
{
    public object GetHealth() => new { status = "ok", service = "testimize-mcp-server", version = "1.0.0", timeUtc = DateTimeOffset.UtcNow };
    public object GetTime() => new { utc = DateTimeOffset.UtcNow, unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), iso = DateTimeOffset.UtcNow.ToString("o") };
    public object GenerateGuid() => new { value = Guid.NewGuid(), kind = "random-v4" };

    public List<TestCase> Generate(List<IInputParameter> parameters, PreciseTestEngineSettings settings)
    {
        // Apply defaults for MCP usage if not explicitly set
        if (settings.Mode == TestGenerationMode.Pairwise && 
            settings.TestCaseCategory == TestCaseCategory.All && 
            settings.MethodName == "TestMethodName")
        {
            // User likely didn't specify settings, apply MCP defaults
            settings.Mode = TestGenerationMode.HybridArtificialBeeColony;
            settings.TestCaseCategory = TestCaseCategory.All;
            settings.MethodName = settings.MethodName == "TestMethodName" ? "GeneratedTestMethod" : settings.MethodName;
        }
        
        // Always use JSON output generator for MCP responses to ensure JSON-compatible results
        settings.OutputGenerator = new JsonTestCaseOutputGenerator();
        
        return new TestSuiteBuilder(parameters, settings).Generate();
    }
}