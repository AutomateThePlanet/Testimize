// <copyright file="CombinatorialTestCaseGenerator.cs" company="Automate The Planet Ltd.">
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

using NUnit.Framework.Internal.Builders;
using NUnit.Framework.Internal;
using Testimize.Contracts;
using Testimize.Parameters.Core;

namespace Testimize.TestCaseGenerators;

public class CombinatorialTestCaseGenerator : ITestCaseGenerator
{
    public HashSet<TestCase> GenerateTestCases(List<IInputParameter> parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            throw new ArgumentException("At least one parameter is required for combinatorial generation.");
        }

        // Convert each parameter's test values to object arrays for NUnit
        var parameterValueSets = parameters.Select(p => p.TestValues.Cast<object>().ToArray()).ToArray();

        // Use NUnit's CombinatorialStrategy to get all combinations
        var strategy = new CombinatorialStrategy();
        var testCasesFromNUnit = strategy.GetTestCases(parameterValueSets);

        var finalTestCases = new HashSet<TestCase>();

        foreach (var testMethod in testCasesFromNUnit)
        {
            var testCase = new TestCase();

            foreach (var value in testMethod.Arguments)
            {
                if (value is TestValue testValue)
                {
                    testCase.Values.Add(testValue);
                }
                else
                {
                    testCase.Values.Add(new TestValue(value?.ToString(), TestValueCategory.Valid));
                }
            }

            finalTestCases.Add(testCase);
        }

        return finalTestCases;
    }

}