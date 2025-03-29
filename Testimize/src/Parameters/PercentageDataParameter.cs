using Testimize.Parameters.Core;
using Testimize.TestValueProviders.Strategies;

namespace Testimize.Parameters;

public class PercentageDataParameter : DataParameter<PercentageDataProviderStrategy>
{
    public PercentageDataParameter(
        bool preciseMode = false,
        decimal? minBoundary = null,
        decimal? maxBoundary = null,
        bool? includeBoundaryValues = null,
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
        : base(new PercentageDataProviderStrategy(minBoundary, maxBoundary),
              preciseMode,
              includeBoundaryValues,
              allowValidEquivalenceClasses,
              allowInvalidEquivalenceClasses,
              preciseTestValues)
    {
    }
}