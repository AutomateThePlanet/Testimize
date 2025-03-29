using Testimize.Parameters.Core;
using Testimize.TestValueProviders.Strategies;

namespace Testimize.Parameters;

public class WeekDataParameter : DataParameter<WeekDataProviderStrategy>
{
    public WeekDataParameter(
        bool preciseMode = false,
        DateTime? minBoundary = null,
        DateTime? maxBoundary = null,
        bool? includeBoundaryValues = null,
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
        : base(new WeekDataProviderStrategy(minBoundary, maxBoundary),
              preciseMode,
              includeBoundaryValues,
              allowValidEquivalenceClasses,
              allowInvalidEquivalenceClasses,
              preciseTestValues)
    {
    }
}