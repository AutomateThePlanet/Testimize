// // <copyright file="MonthDataProviderStrategy.cs" company="Automate The Planet Ltd.">
// // Copyright 2025 Automate The Planet Ltd.
// // Licensed under the Apache License, Version 2.0 (the "License");
// // You may not use this file except in compliance with the License.
// // You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// // Unless required by applicable law or agreed to in writing,
// // software distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// // </copyright>
// // <author>Anton Angelov</author>
// // <site>https://automatetheplanet.com/</site>

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