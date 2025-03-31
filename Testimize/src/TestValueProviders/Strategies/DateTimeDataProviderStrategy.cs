// <copyright file="DateTimeDataProviderStrategy.cs" company="Automate The Planet Ltd.">
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

public class DateTimeDataProviderStrategy : BoundaryCapableDataProviderStrategy<DateTime>
{
    public DateTimeDataProviderStrategy(DateTime? minBoundary = null, DateTime? maxBoundary = null)
        : base(minBoundary, maxBoundary)
    {
    }

    protected override string GetInputTypeName() => "DateTime";

    protected override Type GetExpectedType() => typeof(DateTime);

    protected override TestValue CreateBoundaryTestValue(DateTime boundaryInput, TestValueCategory category)
    {
        var formatted = boundaryInput.ToString(FormatString ?? "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        return new TestValue(formatted, category);
    }

    protected override DateTime OffsetValue(DateTime value, BoundaryOffsetDirection direction)
    {
        var parsed = double.TryParse(PrecisionStep, NumberStyles.Float, CultureInfo.InvariantCulture, out var step);
        if (!parsed) step = 1.0;

        var offset = PrecisionStepUnit switch
        {
            "Seconds" => TimeSpan.FromSeconds(step),
            "Minutes" => TimeSpan.FromMinutes(step),
            "Hours" => TimeSpan.FromHours(step),
            "Days" => TimeSpan.FromDays(step),
            "Milliseconds" => TimeSpan.FromMilliseconds(step),
            _ => TimeSpan.FromMinutes(step)
        };

        return direction == BoundaryOffsetDirection.Before
            ? value.Subtract(offset)
            : value.Add(offset);
    }
}