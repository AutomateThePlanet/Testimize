using Testimize.TestValueProviders.Base;

namespace Testimize.TestValueProviders;
public class MultiSelectDataProviderStrategy : EquivalenceOnlyDataProviderStrategy
{
    protected override string GetInputTypeName() => "MultiSelect";

    protected override Type GetExpectedType() => typeof(string[]);
}
