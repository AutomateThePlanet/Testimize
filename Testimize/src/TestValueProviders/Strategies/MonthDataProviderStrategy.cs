using System.Globalization;
using Testimize.Parameters.Core;
using Testimize.TestValueProviders.Base;

namespace Testimize.TestValueProviders.Strategies;

public class MonthDataProviderStrategy : BoundaryCapableDataProviderStrategy<DateTime>
{
    public MonthDataProviderStrategy(DateTime? minBoundary = null, DateTime? maxBoundary = null)
        : base(minBoundary, maxBoundary)
    {
    }

    protected override string GetInputTypeName() => "Month";

    protected override Type GetExpectedType() => typeof(string); // Output as "yyyy-MM"

    protected override TestValue CreateBoundaryTestValue(DateTime boundaryInput, TestValueCategory category)
    {
        var formatted = boundaryInput.ToString(FormatString ?? "yyyy-MM", CultureInfo.InvariantCulture);
        return new TestValue(formatted, category);
    }

    protected override DateTime OffsetValue(DateTime value, BoundaryOffsetDirection direction)
    {
        var parsed = int.TryParse(PrecisionStep, out var months);
        var offset = parsed ? months : 1;

        return direction == BoundaryOffsetDirection.Before
            ? value.AddMonths(-offset)
            : value.AddMonths(offset);
    }
}