using Testimize.TestValueProviders.Base;

namespace Testimize.TestValueProviders;

public class SingleSelectDataProviderStrategy : EquivalenceOnlyDataProviderStrategy
{
    protected override string GetInputTypeName() => "SingleSelect";

    protected override Type GetExpectedType() => typeof(string);
}
