using Testimize.Parameters.Core;
using Testimize.TestValueProviders.Strategies;

namespace Testimize.Parameters;

public class BooleanDataParameter : DataParameter<BooleanDataProviderStrategy>
{
    public BooleanDataParameter(
        bool preciseMode = false,
        int? minBoundary = null,
        int? maxBoundary = null,
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
        : base(new BooleanDataProviderStrategy(),
              preciseMode,
              false,
              allowValidEquivalenceClasses,
              allowInvalidEquivalenceClasses,
              preciseTestValues)
    {
    }
}