// // <copyright file="TextDataProviderStrategy.cs" company="Automate The Planet Ltd.">
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

using Testimize.Parameters.Core;
using Testimize.TestValueProviders.Base;
using Testimize.Utilities;

namespace Testimize.TestValueProviders.Strategies;
public class TextDataProviderStrategy : BoundaryCapableDataProviderStrategy<int>
{
    public TextDataProviderStrategy(int? minBoundary = null, int? maxBoundary = null)
        : base(minBoundary, maxBoundary)
    {
    }

    protected override string GetInputTypeName() => "Text";

    protected override Type GetExpectedType() => typeof(string);

    protected override TestValue CreateBoundaryTestValue(int boundaryInput, TestValueCategory category)
    {
        var rawText = Faker.Lorem.Sentence(); // e.g., "Lorem ipsum dolor sit amet."
        var finalText = rawText
            .EnsureMaxLength(boundaryInput)
            .EnsureMinLength(boundaryInput, paddingChar: 'A'); // preserves textual feel

        return new TestValue(finalText, category);
    }

    protected override int OffsetValue(int value, BoundaryOffsetDirection direction)
    {
        var parsedSuccessfully = int.TryParse(PrecisionStep, out var step);
        var offset = parsedSuccessfully ? step : 1;

        return direction == BoundaryOffsetDirection.Before ? value - offset : value + offset;
    }
}