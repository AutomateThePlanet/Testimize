// <copyright file="TestCaseEvaluator.cs" company="Automate The Planet Ltd.">
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

namespace Testimize.TestCaseGenerators;

public class TestCaseEvaluator : ITestCaseEvaluator
{
    private readonly bool _allowMultipleInvalidInputs;
    private readonly Dictionary<int, HashSet<object>> _globalSeenValuesPerParameter = new(); // Tracks first-time values per parameter

    public TestCaseEvaluator(bool allowMultipleInvalidInputs = false)
    {
        _allowMultipleInvalidInputs = allowMultipleInvalidInputs;
    }

    // 🔹 Evaluates a population of test cases and assigns scores
    public void EvaluatePopulation(HashSet<TestCase> population)
    {
        _globalSeenValuesPerParameter.Clear(); // Reset global tracking at the start
        foreach (var testCase in population)
        {
            testCase.Score = Evaluate(testCase, population);
        }
    }

    public double Evaluate(TestCase testCase, HashSet<TestCase> evaluatedTestCases)
    {
        double score = 0;
        var firstTimeValueCount = 0; // Track new values in this test case
        var alreadyCoveredValues = GetCoveredValuesPerParameter(evaluatedTestCases);

        var invalidCount = testCase.Values.Count(value =>
            value.Category == TestValueCategory.Invalid ||
            value.Category == TestValueCategory.BoundaryInvalid);

        if (!_allowMultipleInvalidInputs && invalidCount > 1)
        {
            return -50 * invalidCount; // Penalty for multiple invalid inputs
        }

        for (var i = 0; i < testCase.Values.Count; i++)
        {
            var value = testCase.Values[i];

            // 🔹 Assign base score based on category
            switch (value.Category)
            {
                case TestValueCategory.BoundaryValid: score += 20; break;
                case TestValueCategory.Valid: score += 2; break;
                case TestValueCategory.BoundaryInvalid: score += -1; break;
                case TestValueCategory.Invalid: score += -2; break;
            }

            // 🔹 Ensure global tracking per parameter is initialized
            if (!_globalSeenValuesPerParameter.ContainsKey(i))
            {
                _globalSeenValuesPerParameter[i] = new HashSet<object>();
            }

            // 🔹 If this value has never been seen globally, apply a one-time bonus
            if (_globalSeenValuesPerParameter[i].Add(value.Value))
            {
                score += 25;
            }

            // 🔹 If this is the first time this value is seen in the whole evaluated set for this parameter position
            if (!alreadyCoveredValues.ContainsKey(i) || !alreadyCoveredValues[i].Contains(value.Value))
            {
                alreadyCoveredValues[i].Add(value.Value);
                firstTimeValueCount++;
            }
        }

        // 🔹 Apply bonus scaling if multiple first-time values exist
        if (firstTimeValueCount > 0)
        {
            var multiplier = 1 + firstTimeValueCount * 0.25;
            score += 25 * multiplier;
        }

        return score;
    }

    private Dictionary<int, HashSet<object>> GetCoveredValuesPerParameter(HashSet<TestCase> evaluatedPopulation)
    {
        var coveredValues = new Dictionary<int, HashSet<object>>();

        foreach (var testCase in evaluatedPopulation)
        {
            for (var i = 0; i < testCase.Values.Count; i++)
            {
                if (!coveredValues.ContainsKey(i))
                {
                    coveredValues[i] = new HashSet<object>();
                }
                coveredValues[i].Add(testCase.Values[i].Value);
            }
        }

        return coveredValues;
    }

    public Dictionary<TestCase, double> EvaluatePopulationToDictionary(HashSet<TestCase> population)
    {
        return population.ToDictionary(tc => tc, tc => Evaluate(tc, population));
    }
}
