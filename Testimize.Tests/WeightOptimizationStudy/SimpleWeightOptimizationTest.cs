using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.Statistics;
using NUnit.Framework;
using Testimize;
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
        var configurations = new List<(string Name, EvaluatorWeights Weights)>
        {
            ("Current (Baseline)", EvaluatorWeights.Default()),
            ("All Equal (10)", EvaluatorWeights.AllEqual()),
            ("No Diversity", EvaluatorWeights.NoDiversity()),
            ("High Penalty", EvaluatorWeights.HighPenalty()),
            ("Boundary Focused", EvaluatorWeights.BoundaryFocused())
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