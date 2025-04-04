﻿// <copyright file="ABCPairwiseScoresComparisonTests.cs" company="Automate The Planet Ltd.">
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
using System.Diagnostics;
using Testimize.Parameters;
using Testimize.TestCaseGenerators;
using Testimize.Contracts;
using Testimize.Parameters.Core;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Testimize.Tests.Experiments;

[TestFixture]
public class ABCOptimizationBenchmarkTests
{
    private const int Iterations = 10;
    private List<IInputParameter> _parameters;
    private List<ABCGenerationSettings> _parameterSets;
    private Dictionary<ABCGenerationSettings, List<double>> _abcScores = new();
    private Dictionary<ABCGenerationSettings, List<double>> _pairwiseScores = new();
    private HashSet<TestCase> _sortedPairwiseScores = new();

    [SetUp]
    public void SetUp()
    {
        InitializeParameters();
        InitializeParameterSets();
        PrecomputePairwiseScores();
    }

    [Test]
    public void FindBestSeed_ForTopABCConfig_Parallel_Optimized()
    {
        const int maxSeed = 10000;
        var baseConfig = new ABCGenerationSettings();

        Console.WriteLine("\n========== Finding Best Seed for Top Config ==========");

        var results = new ConcurrentBag<(int Seed, double Score)>();

        Parallel.For(0, maxSeed, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, seed =>
        {
            var config = (ABCGenerationSettings)baseConfig.Clone();
            config.Seed = seed;

            var abcGenerator = new HybridArtificialBeeColonyTestCaseGenerator(config);
            var abcTestCases = abcGenerator.RunABCAlgorithm(_parameters);

            var evaluator = new TestCaseEvaluator();
            double score = evaluator.EvaluatePopulationToDictionary(abcTestCases).Values.Sum();
            results.Add((seed, score));
        });

        var best = results.OrderByDescending(r => r.Score).First();

        Console.WriteLine($"\n✅ Best Seed Found: {best.Seed} with Score: {best.Score}");
        Debug.WriteLine($"\n✅ Best Seed Found: {best.Seed} with Score: {best.Score}");
    }

    [Test]
    [Category(Categories.CI)]
    public void RunOptimizationBenchmark()
    {
        Console.WriteLine("\n========== Running ABC Parameter Optimization Benchmark ==========");
        Debug.WriteLine("\n========== Running ABC Parameter Optimization Benchmark ==========");

        foreach (var paramSet in _parameterSets)
        {
            RunBenchmarkForParameterSet(paramSet);
        }

        PrintBestABCParameters();
        PrintBestPairwisePerformance();
    }

    // 🔹 Initialize input parameters for testing different fields
    private void InitializeParameters()
    {

        _parameters = new List<IInputParameter>
        {
            // new TextDataParameter(minBoundary: 6, maxBoundary: 12),
            //new EmailDataParameter(minBoundary: 5, maxBoundary: 10),
            //new PhoneDataParameter(minBoundary: 6, maxBoundary: 8),
            //new TextDataParameter(minBoundary: 4, maxBoundary: 10),
            new TextDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("Normal1", TestValueCategory.Valid),
                new TestValue("BoundaryMin-1", TestValueCategory.BoundaryInvalid),
                new TestValue("BoundaryMin", TestValueCategory.BoundaryValid),
                new TestValue("BoundaryMax", TestValueCategory.BoundaryValid),
                new TestValue("BoundaryMax+1", TestValueCategory.BoundaryInvalid),
                new TestValue("Invalid1", TestValueCategory.Invalid)
            }),
            new EmailDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("test@mail.comMIN-1", TestValueCategory.BoundaryInvalid),
                new TestValue("test@mail.comMIN", TestValueCategory.BoundaryValid),
                new TestValue("test@mail.comMAX", TestValueCategory.BoundaryValid),
                new TestValue("test@mail.comMAX+1", TestValueCategory.BoundaryInvalid),
                new TestValue("test@mail.com", TestValueCategory.Valid),
                new TestValue("invalid@mail", TestValueCategory.Invalid)
            }),
            new PhoneDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("+359888888888", TestValueCategory.Valid),
                new TestValue("000000", TestValueCategory.Invalid)
            }),
            new TextDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("NormalX", TestValueCategory.Valid)
            }),
        };
    }

    // 🔹 Define different ABC parameter sets for benchmarking
    public void InitializeParameterSets()
    {
        _parameterSets = new List<ABCGenerationSettings>
        {
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.6,
                EliteSelectionRatio = 0.6,
                TotalPopulationGenerations = 100,
                MutationRate = 0.5,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                OnlookerSelectionRatio = 0.5,
                EnableScoutPhase = true,
                ScoutSelectionRatio = 0.5,
                CoolingRate = 0.85,
                EnforceMutationUniqueness = false
            },
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.5,
                EliteSelectionRatio = 0.5,
                TotalPopulationGenerations = 50,
                MutationRate = 0.45, // Slightly higher mutation rate to check for improvements
                AllowMultipleInvalidInputs = true,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true
            },
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.55,  // Slightly higher to retain more cases
                EliteSelectionRatio = 0.45,  // Slightly lower to allow more diversity
                TotalPopulationGenerations = 50,
                MutationRate = 0.35,  // Fine-tuned mutation rate
                AllowMultipleInvalidInputs = true,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true
            },
            // 🔹 Best general configuration: Balanced selection & mutation
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.5,
                EliteSelectionRatio = 0.5,
                TotalPopulationGenerations = 50,
                MutationRate = 0.4,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true
            },

            // 🔹 Stronger selection & refinement: Ideal when test cases must be stable
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.5,
                EliteSelectionRatio = 0.7,
                TotalPopulationGenerations = 60,
                MutationRate = 0.5,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true
            },

            // 🔹 Higher mutation rate: Ensures wider test coverage
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.5,
                EliteSelectionRatio = 0.6,
                TotalPopulationGenerations = 70,
                MutationRate = 0.6,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true
            },

            //// 🔹 Balanced exploitation & diversity: Great for complex test scenarios
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.5,
                EliteSelectionRatio = 0.6,
                TotalPopulationGenerations = 100,
                MutationRate = 0.7,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = false,
                EnableScoutPhase = false
            },

            // 🔹 More diverse test cases: Prevents overfitting to high-scoring cases
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.4,
                EliteSelectionRatio = 0.6,
                TotalPopulationGenerations = 100,
                MutationRate = 0.8,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = false,
                EnableScoutPhase = false
            },

            // 🔹 Balanced mutation & selection: Useful when both exploration and exploitation are needed
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.5,
                EliteSelectionRatio = 0.5,
                TotalPopulationGenerations = 100,
                MutationRate = 0.4,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = false,
                EnableScoutPhase = false
            },

            // 🔹 Maximum exploration: Ensures high diversity, best for finding edge cases
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.4,
                EliteSelectionRatio = 0.5,
                TotalPopulationGenerations = 100,
                MutationRate = 0.4,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = false,
                EnableScoutPhase = false
            }
        };
    }

    // 🔹 Precompute pairwise scores for baseline comparison
    private void PrecomputePairwiseScores()
    {
        var pairwiseTestCases = new PairwiseTestCaseGenerator().GenerateTestCases(_parameters);
        var testCaseEvaluator = new TestCaseEvaluator();
        testCaseEvaluator.EvaluatePopulation(pairwiseTestCases);

        // ✅ Ensure correct sorting before storing in `_sortedPairwiseScores`
        _sortedPairwiseScores = new HashSet<TestCase>(pairwiseTestCases.OrderByDescending(tc => tc.Score));
    }

    // 🔹 Run benchmarking for a given ABC parameter set
    private void RunBenchmarkForParameterSet(ABCGenerationSettings paramSet)
    {
        Console.WriteLine($"\n========== Testing ABC with Parameters: {paramSet} ==========");
        _abcScores[paramSet] = new List<double>();
        _pairwiseScores[paramSet] = new List<double>();

        for (var i = 0; i < Iterations; i++)
        {
            var abcTotalScore = RunSingleIteration(paramSet);
            _abcScores[paramSet].Add(abcTotalScore);
        }

        PrintResultsForParameterSet(paramSet);
    }

    // 🔹 Run a single iteration of ABC optimization
    private double RunSingleIteration(ABCGenerationSettings config)
    {
        var abcGenerator = new HybridArtificialBeeColonyTestCaseGenerator(config);

        var abcTestCases = abcGenerator.RunABCAlgorithm(_parameters);

        //abcGenerator.GenerateTestCases("ValidationParams", _parameters, TestCaseCategoty.Valid);
        var testCaseEvaluator = new TestCaseEvaluator();
        var abcScores = testCaseEvaluator.EvaluatePopulationToDictionary(abcTestCases);
        double abcTotalScore = abcScores.Values.Sum();

        var topCount = (int)(_sortedPairwiseScores.Count * config.FinalPopulationSelectionRatio);
        var pairwiseTotalScore = _sortedPairwiseScores.Take(topCount).Sum(p => p.Score);
        _pairwiseScores[config].Add(pairwiseTotalScore);

        return abcTotalScore;
    }

    // 🔹 Print results for each ABC parameter set
    private void PrintResultsForParameterSet(ABCGenerationSettings paramSet)
    {
        var avgAbcScore = _abcScores[paramSet].Average();
        var avgPairwiseScore = _pairwiseScores[paramSet].Average();
        var percentageImprovement = (avgAbcScore - avgPairwiseScore) / avgPairwiseScore * 100;

        Console.WriteLine($"\n========== Summary for Parameters: {paramSet} ==========");
        Console.WriteLine($"✅ ABC Avg Score: {Math.Abs(avgAbcScore)}");
        Console.WriteLine($"✅ Pairwise Avg Score: {Math.Abs(avgPairwiseScore)}");
        Console.WriteLine($"📈 Improvement Over Pairwise: {Math.Abs(percentageImprovement):F2}%");

        Debug.WriteLine($"\n========== Summary for Parameters: {paramSet} ==========");
        Debug.WriteLine($"✅ ABC Avg Score: {avgAbcScore}");
        Debug.WriteLine($"✅ Pairwise Avg Score: {avgPairwiseScore}");
        Debug.WriteLine($"📈 Improvement Over Pairwise: {percentageImprovement:F2}%");
    }

    // 🔹 Print the best ABC parameters
    private void PrintBestABCParameters()
    {
        var bestABC = _abcScores.OrderByDescending(p => p.Value.Average()).First();
        Console.WriteLine("\n========== Best ABC Parameters ==========");
        Console.WriteLine($"Final Population Ratio: {bestABC.Key.FinalPopulationSelectionRatio}");
        Console.WriteLine($"Elite Selection Ratio: {bestABC.Key.EliteSelectionRatio}");
        Console.WriteLine($"Total Generations: {bestABC.Key.TotalPopulationGenerations}");
        Console.WriteLine($"Mutation Rate: {bestABC.Key.MutationRate}");
        Console.WriteLine($"Achieved Avg Score: {bestABC.Value.Average()}");

        Debug.WriteLine("\n========== Best ABC Parameters ==========");
        Debug.WriteLine($"Final Population Ratio: {bestABC.Key.FinalPopulationSelectionRatio}");
        Debug.WriteLine($"Elite Selection Ratio: {bestABC.Key.EliteSelectionRatio}");
        Debug.WriteLine($"Total Generations: {bestABC.Key.TotalPopulationGenerations}");
        Debug.WriteLine($"Mutation Rate: {bestABC.Key.MutationRate}");
        Debug.WriteLine($"Achieved Avg Score: {bestABC.Value.Average()}");
    }

    // 🔹 Print the best pairwise score
    private void PrintBestPairwisePerformance()
    {
        var bestPairwise = _pairwiseScores.OrderByDescending(p => p.Value.Average()).First();
        Console.WriteLine("\n========== Best Pairwise Performance ==========");
        Console.WriteLine($"Achieved Avg Score: {bestPairwise.Value.Average()} with ABC parameters: {bestPairwise.Key}");

        Debug.WriteLine("\n========== Best Pairwise Performance ==========");
        Debug.WriteLine($"Achieved Avg Score: {bestPairwise.Value.Average()} with ABC parameters: {bestPairwise.Key}");
    }
}
