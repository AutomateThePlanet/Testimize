// <copyright file="EquivalenceOnlyDataProviderStrategy.cs" company="Automate The Planet Ltd.">
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

using Testimize.Contracts;
using Testimize.Parameters.Core;

namespace Testimize.TestValueProviders.Base;
public abstract class EquivalenceOnlyDataProviderStrategy : IDataProviderStrategy
{
    protected readonly TestimizeSettings Config;

    protected EquivalenceOnlyDataProviderStrategy()
    {
        Config = Settings.GetSection<TestimizeSettings>();
    }

    public virtual List<TestValue> GenerateTestValues(
        bool? includeBoundaryValues = null, // Ignored
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
    {
        var testValues = new List<TestValue>();
        var allowValidEquiv = allowValidEquivalenceClasses ?? Config.AllowValidEquivalenceClasses;
        var allowInvalidEquiv = allowInvalidEquivalenceClasses ?? Config.AllowInvalidEquivalenceClasses;

        if (allowValidEquiv)
        {
            var source = Config.InputTypeSettings[GetInputTypeName()].ValidEquivalenceClasses.Select(x => (object)x);

            foreach (var value in source)
            {
                testValues.Add(new TestValue(value, TestValueCategory.Valid));
            }
        }

        if (allowInvalidEquiv)
        {
            var source = Config.InputTypeSettings[GetInputTypeName()].InvalidEquivalenceClasses.Select(x => (object)x);

            foreach (var value in source)
            {
                testValues.Add(new TestValue(value, TestValueCategory.Invalid));
            }
        }

        foreach (var customValue in preciseTestValues)
        {
            testValues.Add(new TestValue(customValue.Value, customValue.Category, customValue.ExpectedInvalidMessage));
        }

        return testValues;
    }

    protected abstract Type GetExpectedType();
    protected abstract string GetInputTypeName();
}
