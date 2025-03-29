using Testimize.TestValueProviders.Base;

namespace Testimize.TestValueProviders.Strategies;

public class SingleSelectDataProviderStrategy : EquivalenceOnlyDataProviderStrategy
{
    protected override string GetInputTypeName() => "SingleSelect";

    protected override Type GetExpectedType() => typeof(string);
}
