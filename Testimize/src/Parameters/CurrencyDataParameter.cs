using Testimize.Parameters.Core;
using Testimize.TestValueProviders.Strategies;

namespace Testimize.Parameters;

public class CurrencyDataParameter : DataParameter<CurrencyDataProviderStrategy>
{
    public CurrencyDataParameter(
        bool preciseMode = false,
        decimal? minBoundary = null,
        decimal? maxBoundary = null,
        bool? includeBoundaryValues = null,
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
        : base(new CurrencyDataProviderStrategy(minBoundary, maxBoundary),
              preciseMode,
              includeBoundaryValues,
              allowValidEquivalenceClasses,
              allowInvalidEquivalenceClasses,
              preciseTestValues)
    {
    }

    // Parameterless constructor for factory creation
    public CurrencyDataParameter() : base()
    {
    }

    public override string ParameteryType => this.GetType().FullName;
}