using Testimize.TestValueProviders.Base;

namespace Testimize.TestValueProviders.Strategies;

public class ColorDataProviderStrategy : EquivalenceOnlyDataProviderStrategy
{
    protected override string GetInputTypeName() => "Color";

    protected override Type GetExpectedType() => typeof(string);
}