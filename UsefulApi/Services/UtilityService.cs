
// Business logic implementation
using Testimize;
using Testimize.Contracts;
using Testimize.OutputGenerators;
using Testimize.Parameters.Core;
using Testimize.Usage;

public class UtilityService : IUtilityService
{
    public object GetHealth() => new { status = "ok", service = "useful-api", version = "1.0.0", timeUtc = DateTimeOffset.UtcNow };
    public object GetTime() => new { utc = DateTimeOffset.UtcNow, unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), iso = DateTimeOffset.UtcNow.ToString("o") };
    public object GenerateGuid() => new { value = Guid.NewGuid(), kind = "random-v4" };

    public List<TestCase> Generate(List<IInputParameter> parameters, PreciseTestEngineSettings settings)
        => new TestSuiteBuilder(parameters, settings).Generate();
}