using Testimize.Parameters.Core;
using Testimize.TestValueProviders.Strategies;

namespace Testimize.Parameters;

public class MultiSelectDataParameter : DataParameter<MultiSelectDataProviderStrategy>
{
    public MultiSelectDataParameter(
        bool preciseMode = false,
        bool? includeBoundaryValues = null,
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
        : base(new MultiSelectDataProviderStrategy(),
              preciseMode,
              includeBoundaryValues,
              allowValidEquivalenceClasses,
              allowInvalidEquivalenceClasses,
              preciseTestValues)
    {
    }
}
