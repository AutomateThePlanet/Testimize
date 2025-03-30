// // <copyright file="BoundaryCapableDataProviderStrategy.cs" company="Automate The Planet Ltd.">
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

using Bogus;
using Testimize.Parameters.Core;
using Testimize.Utilities;

namespace Testimize.TestValueProviders.Base;

public abstract class BoundaryCapableDataProviderStrategy<T> : EquivalenceOnlyDataProviderStrategy
        where T : struct, IComparable<T>
{
    protected readonly Faker Faker;
    protected readonly T? MinBoundary;
    protected readonly T? MaxBoundary;
    protected readonly string PrecisionStep;
    protected readonly string PrecisionStepUnit;
    protected readonly string FormatString;

    protected BoundaryCapableDataProviderStrategy(
        T? minBoundary = null,
        T? maxBoundary = null,
        List<object> customValidEquivalenceClasses = null,
        List<object> customInvalidEquivalenceClasses = null)
    {
        Faker = FakerFactory.GetFaker();
        MinBoundary = minBoundary;
        MaxBoundary = maxBoundary;

        var config = Settings.GetSection<TestimizeSettings>();
        var settings = config.InputTypeSettings[GetInputTypeName()];
        PrecisionStep = settings.PrecisionStep;
        PrecisionStepUnit = settings.PrecisionStepUnit;
        FormatString = settings.FormatString;
    }

    public override List<TestValue> GenerateTestValues(
        bool? includeBoundaryValues = null,
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
    {
        var testValues = base.GenerateTestValues(
            includeBoundaryValues: false, // Let us handle boundary addition here
            allowValidEquivalenceClasses,
            allowInvalidEquivalenceClasses,
            preciseTestValues);

        if ((includeBoundaryValues ?? true) && MinBoundary != null && MaxBoundary != null)
        {
            testValues.Add(CreateBoundaryTestValue(OffsetValue(MinBoundary.Value, BoundaryOffsetDirection.Before), TestValueCategory.BoundaryInvalid));
            testValues.Add(CreateBoundaryTestValue(MinBoundary.Value, TestValueCategory.BoundaryValid));
            testValues.Add(CreateBoundaryTestValue(MaxBoundary.Value, TestValueCategory.BoundaryValid));
            testValues.Add(CreateBoundaryTestValue(OffsetValue(MaxBoundary.Value, BoundaryOffsetDirection.After), TestValueCategory.BoundaryInvalid));
        }

        return testValues;
    }

    protected abstract T OffsetValue(T value, BoundaryOffsetDirection direction);
    protected abstract TestValue CreateBoundaryTestValue(T boundaryInput, TestValueCategory category);
}