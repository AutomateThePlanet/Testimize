using Testimize.TestValueProviders.Base;

namespace Testimize.TestValueProviders.Strategies;

public class BooleanDataProviderStrategy : EquivalenceOnlyDataProviderStrategy
{
    protected override string GetInputTypeName() => "Boolean";

    protected override Type GetExpectedType() => typeof(bool);
}