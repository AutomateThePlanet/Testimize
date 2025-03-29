using Testimize.Parameters.Core;
using Testimize.TestValueProviders.Base;
using Testimize.Utilities;

namespace Testimize.TestValueProviders.Strategies;

public class AddressDataProviderStrategy : BoundaryCapableDataProviderStrategy<int>
{
    public AddressDataProviderStrategy(int? minBoundary = null, int? maxBoundary = null)
        : base(minBoundary, maxBoundary)
    {
    }

    protected override string GetInputTypeName() => "Address";

    protected override Type GetExpectedType() => typeof(string);

    protected override TestValue CreateBoundaryTestValue(int boundaryInput, TestValueCategory category)
    {
        var generated = Faker.Address.FullAddress()
            .EnsureMaxLength(boundaryInput)
            .EnsureMinLength(boundaryInput);

        return new TestValue(generated, category);
    }

    protected override int OffsetValue(int value, BoundaryOffsetDirection direction)
    {
        var parsed = int.TryParse(PrecisionStep, out var step);
        var offset = parsed ? step : 1;
        return direction == BoundaryOffsetDirection.Before ? value - offset : value + offset;
    }
}