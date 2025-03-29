using Testimize.TestValueProviders;

namespace Testimize.Parameters;

public class ColorDataParameter : DataParameter<ColorDataProviderStrategy>
{
    public ColorDataParameter(
         List<object> validOptions = null,
        List<object> invalidOptions = null,
        bool preciseMode = false,
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
        : base(new ColorDataProviderStrategy(),
              preciseMode,
              false,
              allowValidEquivalenceClasses,
              allowInvalidEquivalenceClasses,
              preciseTestValues)
    {
    }
}
