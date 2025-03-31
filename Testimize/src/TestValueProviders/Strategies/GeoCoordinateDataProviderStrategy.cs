// <copyright file="GeoCoordinateDataProviderStrategy.cs" company="Automate The Planet Ltd.">
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
public class GeoCoordinateDataProviderStrategy : BoundaryCapableDataProviderStrategy<double>
{
    public GeoCoordinateDataProviderStrategy(double? minBoundary = null, double? maxBoundary = null)
        : base(minBoundary, maxBoundary)
    {
    }

    protected override string GetInputTypeName() => "GeoCoordinate";

    protected override Type GetExpectedType() => typeof(string); // Stored as "lat,lon" string

    protected override TestValue CreateBoundaryTestValue(double boundaryInput, TestValueCategory category)
    {
        var lat = boundaryInput.ToString(FormatString ?? "F6", CultureInfo.InvariantCulture);
        var lon = (boundaryInput / 2).ToString(FormatString ?? "F6", CultureInfo.InvariantCulture);

        var coord = $"{lat},{lon}";
        return new TestValue(coord, category);
    }

    protected override double OffsetValue(double value, BoundaryOffsetDirection direction)
    {
        var parsed = double.TryParse(PrecisionStep, NumberStyles.Float, CultureInfo.InvariantCulture, out var step);
        var offset = parsed ? step : 0.01;

        return direction == BoundaryOffsetDirection.Before ? value - offset : value + offset;
    }
}
