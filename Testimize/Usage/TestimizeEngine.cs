using Testimize.Contracts;

namespace Testimize.Usage;
public partial class TestimizeEngine
{
    private readonly List<IInputParameter> _parameters;
    private readonly PreciseTestEngineSettings _config;

    private TestimizeEngine(List<IInputParameter> parameters, PreciseTestEngineSettings config)
    {
        _parameters = parameters;
        _config = config;
    }

    public static TestSuiteBuilder Configure(
        Action<TestInputSetBuilder> parametersConfig,
        Action<PreciseTestEngineSettings> configOverrides = null)
    {
        var composer = new TestInputSetBuilder();
        parametersConfig(composer);

        var config = new PreciseTestEngineSettings();
        configOverrides?.Invoke(config);

        return new TestSuiteBuilder(composer.Build(), config);
    }
}