using Testimize.TestValueProviders;

namespace Testimize.Parameters;

public class UrlDataParameter : DataParameter<UrlDataProviderStrategy>
{
    public UrlDataParameter(
        bool preciseMode = false,
        int? minBoundary = null,
        int? maxBoundary = null,
        bool? includeBoundaryValues = null,
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
        : base(new UrlDataProviderStrategy(minBoundary, maxBoundary),
              preciseMode,
              includeBoundaryValues,
              allowValidEquivalenceClasses,
              allowInvalidEquivalenceClasses,
              preciseTestValues)
    {
    }
}