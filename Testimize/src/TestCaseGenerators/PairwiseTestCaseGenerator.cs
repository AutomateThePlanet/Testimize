// <copyright file="PairwiseTestCaseGenerator.cs" company="Automate The Planet Ltd.">
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
using Testimize.Contracts;
using Testimize.Parameters.Core;

namespace Testimize.TestCaseGenerators;

public class PairwiseTestCaseGenerator : ITestCaseGenerator
{
    public HashSet<TestCase> GenerateTestCases(List<IInputParameter> parameters)
    {
        if (parameters == null || parameters.Count < 2)
        {
            throw new ArgumentException("Pairwise testing requires at least two parameters.");
        }

        // Convert parameters into a format suitable for NUnit's PairwiseStrategy
        var parameterValues = parameters.Select(p => p.TestValues.Cast<object>().ToArray()).ToArray();
        var pairwiseCombinations = GeneratePairwiseCombinations(parameterValues);

        // Convert NUnit's test case format to Testimize TestCase objects
        var uniqueTestCases = new HashSet<TestCase>(pairwiseCombinations);
        return uniqueTestCases;
    }

    private List<TestCase> GeneratePairwiseCombinations(object[][] parameterValues)
    {
        var testCases = new List<TestCase>();

        // Use NUnit's PairwiseStrategy
        var strategy = new PairwiseStrategy();
        var generatedPairs = strategy.GetTestCases(parameterValues);

        // Convert generated pairs into TestCase objects
        foreach (var generatedPair in generatedPairs)
        {
            var testCase = new TestCase();
            testCase.Values.AddRange(generatedPair.Arguments.Cast<TestValue>());
            testCases.Add(testCase);
        }

        return testCases;
    }
}
