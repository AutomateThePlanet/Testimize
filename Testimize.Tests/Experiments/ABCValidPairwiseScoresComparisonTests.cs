// <copyright file="ABCValidPairwiseScoresComparisonTests.cs" company="Automate The Planet Ltd.">
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
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;

namespace Testimize.Tests.Experiments;

/// <summary>
/// Comprehensive benchmark test class that compares the performance of ABC (Artificial Bee Colony) 
/// algorithm against traditional pairwise test case generation. This class implements rigorous 
/// statistical analysis using t-tests and confidence intervals to validate performance improvements.
/// 
/// The class performs the following statistical analyses:
/// 1. Descriptive statistics (mean, standard deviation)
/// 2. 95% confidence intervals using t-distribution
/// 3. Paired t-tests for significance testing
/// 4. Variance analysis across different random seeds
/// </summary>
[TestFixture]
public class ABCValidPairwiseScoresComparisonTests
{
    private const int Iterations = 10;
    private static readonly int[] Seeds = Enumerable.Range(0, 10).ToArray(); // 0..9

    private List<IInputParameter> _parameters;
    private List<ABCGenerationSettings> _parameterSets;
    private readonly Dictionary<ABCGenerationSettings, List<double>> _abcScores = new();
    private readonly Dictionary<ABCGenerationSettings, List<double>> _pairwiseScores = new();
    private HashSet<TestCase> _sortedPairwiseScores = new();

    [SetUp]
    public void SetUp()
    {
        InitializeParameters();
        InitializeParameterSets();
        PrecomputePairwiseScores();
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

    [Test]
    [Category(Categories.CI)]
    public void RunOptimizationBenchmark_VarianceBySeed()
    {
        Console.WriteLine("\n========== Running ABC Variance by Seed (additional reproducibility check) ==========");

        foreach (var paramSet in _parameterSets)
        {
            var abc = new List<double>();
            var pw = new List<double>();

            // Pairwise is deterministic; we calculate it once for paramSet (topCount depends on FinalPopulationSelectionRatio)
            var topCount = (int)(_sortedPairwiseScores.Count * paramSet.FinalPopulationSelectionRatio);
            var pairwiseTotalScore = _sortedPairwiseScores.Take(topCount).Sum(p => p.Score);

            foreach (var seed in Seeds)
            {
                var clone = (ABCGenerationSettings)paramSet.Clone();
                clone.Seed = seed;

                var abcGenerator = new HybridArtificialBeeColonyTestCaseGenerator(clone);
                var abcTestCases = abcGenerator.RunABCAlgorithm(_parameters);

                var evaluator = new TestCaseEvaluator();
                double abcTotal = evaluator.EvaluatePopulationToDictionary(abcTestCases).Values.Sum();

                abc.Add(abcTotal);
                pw.Add(pairwiseTotalScore); // same baseline value for every seed
            }

            // Statistical analysis using StatisticalAnalysisHelper
            var (avgAbc, sdAbc, seAbc) = StatisticalAnalysisHelper.CalculateDescriptiveStatistics(abc);
            var (loAbc, hiAbc) = StatisticalAnalysisHelper.ConfidenceInterval95T(avgAbc, seAbc, df: abc.Count - 1);

            var (avgPw, sdPw, sePw) = StatisticalAnalysisHelper.CalculateDescriptiveStatistics(pw);
            var (loPw, hiPw) = StatisticalAnalysisHelper.ConfidenceInterval95T(avgPw, sePw, df: pw.Count - 1);

            var improvement = StatisticalAnalysisHelper.CalculateImprovement(avgAbc, avgPw);

            // Paired t-test on differences by seed
            var diffs = abc.Zip(pw, (a, b) => a - b).ToArray();
            var tRes = StatisticalAnalysisHelper.PairedTTest(diffs);

            Console.WriteLine($"\n========== Variance by Seed for: {paramSet} ==========");
            Console.WriteLine($"ABC Mean ± SD: {StatisticalAnalysisHelper.FormatMeanWithStdDev(avgAbc, sdAbc, 2)}  (95% CI: {StatisticalAnalysisHelper.FormatConfidenceInterval(loAbc, hiAbc, 2)})");
            Console.WriteLine($"PW  Mean ± SD: {StatisticalAnalysisHelper.FormatMeanWithStdDev(avgPw, sdPw, 2)}  (95% CI: {StatisticalAnalysisHelper.FormatConfidenceInterval(loPw, hiPw, 2)})");
            Console.WriteLine($"Δ Improvement (mean): {StatisticalAnalysisHelper.FormatPercentage(improvement, 1)}");
            Console.WriteLine($"Paired t-test: t = {StatisticalAnalysisHelper.FormatStatValue(tRes.tStatistic, 3)}, df = {tRes.degreesOfFreedom}, p = {StatisticalAnalysisHelper.FormatPValue(tRes.pValueTwoTailed)}");
        }
    }

    // 🔹 Initialize input parameters (valid-focused)
    private void InitializeParameters()
    {
        _parameters = new List<IInputParameter>
        {
            new TextDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("Normal1", TestValueCategory.Valid),
                new TestValue("BoundaryMin-1", TestValueCategory.Valid),
                new TestValue("BoundaryMin", TestValueCategory.BoundaryValid),
                new TestValue("BoundaryMax", TestValueCategory.BoundaryValid),
                new TestValue("BoundaryMax+1", TestValueCategory.Valid),
                new TestValue("Invalid1", TestValueCategory.Valid)
            }),
            new EmailDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("test@mail.comMIN-1", TestValueCategory.Valid),
                new TestValue("test@mail.comMIN", TestValueCategory.BoundaryValid),
                new TestValue("test@mail.comMAX", TestValueCategory.BoundaryValid),
                new TestValue("test@mail.comMAX+1", TestValueCategory.Valid),
                new TestValue("test@mail.com", TestValueCategory.Valid),
                new TestValue("invalid@mail", TestValueCategory.Valid)
            }),
            new PhoneDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("+359888888888", TestValueCategory.Valid),
                new TestValue("000000", TestValueCategory.Valid)
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
                TotalPopulationGenerations = 150,
                MutationRate = 0.8,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                OnlookerSelectionRatio = 0.4,
                EnableScoutPhase = true,
                StagnationThresholdPercentage = 0.85,
                ScoutSelectionRatio = 0.3,
                CoolingRate = 0.99,
                EnforceMutationUniqueness = false
            },
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.6,
                EliteSelectionRatio = 0.3,
                TotalPopulationGenerations = 100,
                MutationRate = 0.5,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                OnlookerSelectionRatio = 0.4,
                EnableScoutPhase = true,
                ScoutSelectionRatio = 0.3,
                CoolingRate = 0.95,
                EnforceMutationUniqueness = false
            },
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.6,
                EliteSelectionRatio = 0.3,
                TotalPopulationGenerations = 100,
                MutationRate = 0.5,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                OnlookerSelectionRatio = 0.4,
                EnableScoutPhase = false,
                ScoutSelectionRatio = 0.3,
                CoolingRate = 0.95,
                EnforceMutationUniqueness = false
            },
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.6,
                EliteSelectionRatio = 0.3,
                TotalPopulationGenerations = 100,
                MutationRate = 0.5,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = false,
                OnlookerSelectionRatio = 0.4,
                EnableScoutPhase = true,
                ScoutSelectionRatio = 0.3,
                CoolingRate = 0.95,
                EnforceMutationUniqueness = false
            },
             new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.6,
                EliteSelectionRatio = 0.3,
                TotalPopulationGenerations = 100,
                MutationRate = 0.5,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = false,
                OnlookerSelectionRatio = 0.4,
                EnableScoutPhase = false,
                ScoutSelectionRatio = 0.3,
                CoolingRate = 0.95,
                EnforceMutationUniqueness = false
            },
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
                MutationRate = 0.45,
                AllowMultipleInvalidInputs = true,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true
            },
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.55,
                EliteSelectionRatio = 0.45,
                TotalPopulationGenerations = 50,
                MutationRate = 0.35,
                AllowMultipleInvalidInputs = true,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true
            },
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

        var testCaseEvaluator = new TestCaseEvaluator();
        var abcScores = testCaseEvaluator.EvaluatePopulationToDictionary(abcTestCases);
        double abcTotalScore = abcScores.Values.Sum();

        var topCount = (int)(_sortedPairwiseScores.Count * config.FinalPopulationSelectionRatio);
        var pairwiseTotalScore = _sortedPairwiseScores.Take(topCount).Sum(p => p.Score);
        _pairwiseScores[config].Add(pairwiseTotalScore);

        return abcTotalScore;
    }

    // 🔹 Print results for each ABC parameter set (mean±SD, 95% CI, paired t-test)
    private void PrintResultsForParameterSet(ABCGenerationSettings paramSet)
    {
        var abcList = _abcScores[paramSet];
        var pwList = _pairwiseScores[paramSet];

        var (avgAbc, sdAbc, seAbc) = StatisticalAnalysisHelper.CalculateDescriptiveStatistics(abcList);
        var (avgPw, sdPw, sePw) = StatisticalAnalysisHelper.CalculateDescriptiveStatistics(pwList);
        var improvement = StatisticalAnalysisHelper.CalculateImprovement(avgAbc, avgPw);

        // t-distribution based 95% confidence intervals
        var (loAbc, hiAbc) = StatisticalAnalysisHelper.ConfidenceInterval95T(avgAbc, seAbc, df: abcList.Count - 1);
        var (loPw, hiPw) = StatisticalAnalysisHelper.ConfidenceInterval95T(avgPw, sePw, df: pwList.Count - 1);

        // Paired t-test on per-iteration differences
        var diffs = abcList.Zip(pwList, (a, b) => a - b).ToArray();
        var tRes = StatisticalAnalysisHelper.PairedTTest(diffs);

        Console.WriteLine($"\n========== Summary for Parameters: {paramSet} ==========");
        Console.WriteLine($"✅ ABC Mean ± SD: {StatisticalAnalysisHelper.FormatMeanWithStdDev(avgAbc, sdAbc, 2)}  (95% CI: {StatisticalAnalysisHelper.FormatConfidenceInterval(loAbc, hiAbc, 2)})");
        Console.WriteLine($"✅ Pairwise Mean ± SD: {StatisticalAnalysisHelper.FormatMeanWithStdDev(avgPw, sdPw, 2)}  (95% CI: {StatisticalAnalysisHelper.FormatConfidenceInterval(loPw, hiPw, 2)})");
        Console.WriteLine($"📈 Improvement Over Pairwise (mean): {StatisticalAnalysisHelper.FormatPercentage(improvement, 1)}");
        Console.WriteLine($"🧪 Paired t-test (ABC - Pairwise): t = {StatisticalAnalysisHelper.FormatStatValue(tRes.tStatistic, 3)}, df = {tRes.degreesOfFreedom}, p = {StatisticalAnalysisHelper.FormatPValue(tRes.pValueTwoTailed)}");
    }


    // === Summary Report Methods ===

    private void PrintBestABCParameters()
    {
        var bestABC = _abcScores.OrderByDescending(p => p.Value.Average()).First();
        Console.WriteLine("\n========== Best ABC Parameters ==========");
        Console.WriteLine($"Final Population Ratio: {bestABC.Key.FinalPopulationSelectionRatio}");
        Console.WriteLine($"Elite Selection Ratio: {bestABC.Key.EliteSelectionRatio}");
        Console.WriteLine($"Total Generations: {bestABC.Key.TotalPopulationGenerations}");
        Console.WriteLine($"Mutation Rate: {bestABC.Key.MutationRate}");
        Console.WriteLine($"Achieved Avg Score: {StatisticalAnalysisHelper.FormatStatValue(bestABC.Value.Average(), 2)}");
    }

    private void PrintBestPairwisePerformance()
    {
        var bestPairwise = _pairwiseScores.OrderByDescending(p => p.Value.Average()).First();
        Console.WriteLine("\n========== Best Pairwise Performance ==========");
        Console.WriteLine($"Achieved Avg Score: {StatisticalAnalysisHelper.FormatStatValue(bestPairwise.Value.Average(), 2)} with ABC parameters: {bestPairwise.Key}");
    }
}
