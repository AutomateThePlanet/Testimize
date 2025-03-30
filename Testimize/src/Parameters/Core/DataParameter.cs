// // <copyright file="DataParameter.cs" company="Automate The Planet Ltd.">
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

using Testimize.Contracts;

namespace Testimize.Parameters.Core;

public class DataParameter<TDataStrategy> : IInputParameter
    where TDataStrategy : class, IDataProviderStrategy
{
    public DataParameter(
        TDataStrategy dataProviderStrategy = null,
        bool preciseMode = false,
        bool? includeBoundaryValues = null,
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
    {
        DataProviderStrategy = dataProviderStrategy;

        TestValues = DataProviderStrategy.GenerateTestValues(
         includeBoundaryValues: preciseMode ? false : includeBoundaryValues, // Disable boundary calculations in manual mode
         allowValidEquivalenceClasses: preciseMode ? false : allowValidEquivalenceClasses,
         allowInvalidEquivalenceClasses: preciseMode ? false : allowInvalidEquivalenceClasses,
         preciseTestValues);
    }

    protected TDataStrategy DataProviderStrategy { get; }
    public List<TestValue> TestValues { get; }
}
