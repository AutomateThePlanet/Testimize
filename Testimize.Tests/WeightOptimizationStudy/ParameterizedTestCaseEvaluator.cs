// <copyright file="ParameterizedTestCaseEvaluator.cs" company="Automate The Planet Ltd.">
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
using System;
using System.Collections.Generic;
using System.Linq;
using Testimize.Contracts;
using Testimize.Parameters.Core;
using System.Collections.Concurrent;

namespace Testimize.Tests.WeightOptimizationStudy;

/// <summary>
/// A parameterized version of TestCaseEvaluator that allows configurable weights
/// for scientific optimization studies.
/// </summary>
public class ParameterizedTestCaseEvaluator : ITestCaseEvaluator
{
    private readonly EvaluatorWeightsFactory _weights;
    private readonly bool _allowMultipleInvalidInputs;
    private readonly ConcurrentDictionary<int, object> _globalSeenValuesLocks = new();
    private readonly ConcurrentDictionary<int, HashSet<object>> _globalSeenValuesPerParameter = new();

    public ParameterizedTestCaseEvaluator(EvaluatorWeightsFactory weights, bool allowMultipleInvalidInputs = false)
    {
        _weights = weights ?? throw new ArgumentNullException(nameof(weights));
        _allowMultipleInvalidInputs = allowMultipleInvalidInputs;
    }

    public void EvaluatePopulation(HashSet<TestCase> population)
    {
        _globalSeenValuesPerParameter.Clear();
        foreach (var testCase in population)
        {
            testCase.Score = Evaluate(testCase, population);
        }
    }

    public double Evaluate(TestCase testCase, HashSet<TestCase> evaluatedTestCases)
    {
        double score = 0;
        var firstTimeValueCount = 0;
        var alreadyCoveredValues = GetCoveredValuesPerParameter(evaluatedTestCases);

        var invalidCount = testCase.Values.Count(value =>
            value.Category == TestValueCategory.Invalid ||
            value.Category == TestValueCategory.BoundaryInvalid);

        if (!_allowMultipleInvalidInputs && invalidCount > 1)
        {
            return -_weights.MultipleInvalidPenaltyFactor * invalidCount;
        }

        for (var i = 0; i < testCase.Values.Count; i++)
        {
            var value = testCase.Values[i];

            // Apply configurable weights based on category
            switch (value.Category)
            {
                case TestValueCategory.BoundaryValid:
                    score += _weights.BoundaryValidWeight;
                    break;
                case TestValueCategory.Valid:
                    score += _weights.ValidWeight;
                    break;
                case TestValueCategory.BoundaryInvalid:
                    score += _weights.BoundaryInvalidWeight;
                    break;
                case TestValueCategory.Invalid:
                    score += _weights.InvalidWeight;
                    break;
            }

            // Ensure global tracking per parameter is initialized
            _globalSeenValuesLocks.GetOrAdd(i, _ => new object());
            _globalSeenValuesPerParameter.GetOrAdd(i, _ => new HashSet<object>());

            // Apply configurable first-time value bonus
            lock (_globalSeenValuesLocks[i])
            {
                if (_globalSeenValuesPerParameter[i].Add(value.Value))
                {
                    score += _weights.FirstTimeValueBonus;
                }
            }

            // Track first-time values in the evaluated set
            if (!alreadyCoveredValues.ContainsKey(i) || !alreadyCoveredValues[i].Contains(value.Value))
            {
                alreadyCoveredValues.GetOrAdd(i, _ => new HashSet<object>()).Add(value.Value);
                firstTimeValueCount++;
            }
        }

        // Apply configurable bonus scaling for multiple first-time values
        if (firstTimeValueCount > 0)
        {
            var multiplier = 1 + firstTimeValueCount * _weights.FirstTimeMultiplierIncrement;
            score += _weights.FirstTimeValueBonus * multiplier;
        }

        return score;
    }

    private ConcurrentDictionary<int, HashSet<object>> GetCoveredValuesPerParameter(HashSet<TestCase> evaluatedPopulation)
    {
        var coveredValues = new ConcurrentDictionary<int, HashSet<object>>();

        foreach (var testCase in evaluatedPopulation)
        {
            for (var i = 0; i < testCase.Values.Count; i++)
            {
                coveredValues.GetOrAdd(i, _ => new HashSet<object>()).Add(testCase.Values[i].Value);
            }
        }

        return coveredValues;
    }

    public Dictionary<TestCase, double> EvaluatePopulationToDictionary(HashSet<TestCase> population)
    {
        return population.ToDictionary(tc => tc, tc => Evaluate(tc, population));
    }
}