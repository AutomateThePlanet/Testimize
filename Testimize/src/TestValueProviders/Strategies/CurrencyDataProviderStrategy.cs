// <copyright file="CurrencyDataProviderStrategy.cs" company="Automate The Planet Ltd.">
// Copyright 2025 Automate The Planet Ltd.
// Licensed under the Apache License, Version 2.0 (the "License");
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Anton Angelov</author>
// <site>https://automatetheplanet.com/</site>

using System.Globalization;
using Testimize.Parameters.Core;
using Testimize.TestValueProviders.Base;

namespace Testimize.TestValueProviders.Strategies;

public class CurrencyDataProviderStrategy : BoundaryCapableDataProviderStrategy<decimal>
{
    public CurrencyDataProviderStrategy(decimal? minBoundary = null, decimal? maxBoundary = null)
        : base(minBoundary, maxBoundary)
    {
    }

    protected override string GetInputTypeName() => "Currency";

    protected override Type GetExpectedType() => typeof(decimal);

    protected override TestValue CreateBoundaryTestValue(decimal boundaryInput, TestValueCategory category)
    {
        // GenerateTestCases a realistic currency value from Bogus (not required but possible)
        var raw = Faker.Finance.Amount(min: boundaryInput, max: boundaryInput);

        // Apply formatting
        var formatted = raw.ToString(FormatString ?? "F2", CultureInfo.InvariantCulture);

        return new TestValue(formatted, category);
    }

    protected override decimal OffsetValue(decimal value, BoundaryOffsetDirection direction)
    {
        var parsed = decimal.TryParse(PrecisionStep, NumberStyles.Float, CultureInfo.InvariantCulture, out var step);
        var offset = parsed ? step : 0.01m;

        return direction == BoundaryOffsetDirection.Before
            ? value - offset
            : value + offset;
    }
}