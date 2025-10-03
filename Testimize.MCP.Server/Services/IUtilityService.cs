using System;
using Testimize.Contracts;
using Testimize.Parameters.Core;
using Testimize.Usage;

// Business logic interface (Single Responsibility)
public interface IUtilityService
{
    object GetHealth();
    object GetTime();
    object GenerateGuid();
    public List<TestCase> Generate(List<IInputParameter> parameters, PreciseTestEngineSettings settings)
    => new TestSuiteBuilder(parameters, settings).Generate();
}
