// <copyright file="SimpleWeightOptimizationTest.cs" company="Automate The Planet Ltd.">
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
using MathNet.Numerics.Statistics;
using Testimize.Contracts;
using Testimize.Parameters;
using Testimize.Parameters.Core;
using Testimize.TestCaseGenerators;

namespace Testimize.Tests.WeightOptimizationStudy;

/// <summary>
/// Simplified weight optimization test to validate TestCaseEvaluator weights
/// </summary>
[TestFixture]
public class SimpleWeightOptimizationTest
{
    [Test]
    public void ValidateCurrentWeights_AgainstAlternatives()
    {
        Console.WriteLine("\n========== WEIGHT CONFIGURATION COMPARISON ==========\n");

        // Define weight configurations
        var configurations = new List<(string Name, EvaluatorWeightsFactory Weights)>
        {
            ("Current (Baseline)", EvaluatorWeightsFactory.Default()),
            ("All Equal (10)", EvaluatorWeightsFactory.AllEqual()),
            ("No Diversity", EvaluatorWeightsFactory.NoDiversity()),
            ("High Penalty", EvaluatorWeightsFactory.HighPenalty()),
            ("Boundary Focused", EvaluatorWeightsFactory.BoundaryFocused())
        };

        // Create a simple test scenario with precise values
        var parameters = CreateSimpleScenario();

        // Run each configuration and collect scores
        var results = new Dictionary<string, List<double>>();
        const int runs = 10;

        foreach (var config in configurations)
        {
            results[config.Name] = new List<double>();

            for (int run = 0; run < runs; run++)
            {
                var settings = new ABCGenerationSettings
                {
                    Seed = 42 + run,
                    TotalPopulationGenerations = 10,
                    FinalPopulationSelectionRatio = 0.5,
                    TestCaseEvaluator = new ParameterizedTestCaseEvaluator(config.Weights),
                    TestCaseGenerator = new PairwiseTestCaseGenerator()
                };

                var generator = new HybridArtificialBeeColonyTestCaseGenerator(settings);
                var testCases = generator.RunABCAlgorithm(parameters);

                // Calculate total score
                double totalScore = testCases.Sum(tc => tc.Score);
                results[config.Name].Add(totalScore);
            }
        }

        // Display results
        Console.WriteLine("Configuration           Mean Score    Std Dev     Test Cases");
        Console.WriteLine("---------------------------------------------------------------");

        var baseline = results["Current (Baseline)"];
        var baselineMean = baseline.Mean();

        foreach (var result in results.OrderByDescending(r => r.Value.Mean()))
        {
            var mean = result.Value.Mean();
            var stdDev = result.Value.StandardDeviation();
            var improvement = ((mean - baselineMean) / baselineMean) * 100;

            Console.WriteLine($"{result.Key,-25} {mean,10:F2} {stdDev,10:F2}  {improvement,+8:F1}%");
        }

        // Assert that current weights are in top 2
        var rankedResults = results.OrderByDescending(r => r.Value.Mean()).ToList();
        var baselineRank = rankedResults.FindIndex(r => r.Key == "Current (Baseline)") + 1;

        Console.WriteLine($"\nBaseline ranking: #{baselineRank} of {rankedResults.Count}");

        Assert.That(baselineRank, Is.LessThanOrEqualTo(2),
            "Current weights should rank in top 2 configurations");
    }

    private List<IInputParameter> CreateSimpleScenario()
    {
        return new List<IInputParameter>
        {
            new TextDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("Short", TestValueCategory.BoundaryValid),
                new TestValue("Normal Text Value", TestValueCategory.Valid),
                new TestValue("VeryLongTextValueHere", TestValueCategory.BoundaryValid),
                new TestValue("", TestValueCategory.BoundaryInvalid),
                new TestValue("TooLongTextValueThatExceedsLimit", TestValueCategory.BoundaryInvalid),
                new TestValue("Invalid@#$", TestValueCategory.Invalid)
            }),

            new EmailDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("a@b.co", TestValueCategory.BoundaryValid),
                new TestValue("user@example.com", TestValueCategory.Valid),
                new TestValue("long.email@domain.com", TestValueCategory.BoundaryValid),
                new TestValue("invalid", TestValueCategory.BoundaryInvalid),
                new TestValue("", TestValueCategory.Invalid)
            }),

            new IntegerDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue(1, TestValueCategory.BoundaryValid),
                new TestValue(50, TestValueCategory.Valid),
                new TestValue(100, TestValueCategory.BoundaryValid),
                new TestValue(0, TestValueCategory.BoundaryInvalid),
                new TestValue(101, TestValueCategory.BoundaryInvalid),
                new TestValue(-1, TestValueCategory.Invalid)
            })
        };
    }
}