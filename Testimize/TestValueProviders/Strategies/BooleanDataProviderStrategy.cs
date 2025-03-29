using Testimize.TestValueProviders.Base;

namespace Testimize.TestValueProviders;

public class BooleanDataProviderStrategy : EquivalenceOnlyDataProviderStrategy
{
    protected override string GetInputTypeName() => "Boolean";

    protected override Type GetExpectedType() => typeof(bool);
}