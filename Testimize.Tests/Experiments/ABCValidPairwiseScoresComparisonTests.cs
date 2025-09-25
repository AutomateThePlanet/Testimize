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
using MathNet.Numerics.Statistics;   // StdDev, Variance, Mean
using MathNet.Numerics.Distributions; // StudentT

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

            // Statistical analysis (t-distribution based CI, paired t-test)
            var avgAbc = abc.Average();
            var sdAbc = abc.StandardDeviation(); // sample SD (n-1)
            var seAbc = sdAbc / Math.Sqrt(abc.Count);
            var (loAbc, hiAbc) = ConfidenceInterval95T(avgAbc, seAbc, df: abc.Count - 1);

            var avgPw = pw.Average();
            var sdPw = pw.StandardDeviation();
            var sePw = sdPw / Math.Sqrt(pw.Count);
            var (loPw, hiPw) = ConfidenceInterval95T(avgPw, sePw, df: pw.Count - 1);

            var improvement = (avgAbc - avgPw) / avgPw * 100.0;

            // Paired t-test on differences by seed
            var diffs = abc.Zip(pw, (a, b) => a - b).ToArray();
            var tRes = PairedTTest(diffs);

            Console.WriteLine($"\n========== Variance by Seed for: {paramSet} ==========");
            Console.WriteLine($"ABC Mean ± SD: {avgAbc:F4} ± {sdAbc:F4}  (95% CI: [{loAbc:F4}, {hiAbc:F4}])");
            Console.WriteLine($"PW  Mean ± SD: {avgPw:F4} ± {sdPw:F4}  (95% CI: [{loPw:F4}, {hiPw:F4}])");
            Console.WriteLine($"Δ Improvement (mean): {improvement:F2}%");
            Console.WriteLine($"Paired t-test: t = {tRes.tStatistic:F4}, df = {tRes.degreesOfFreedom}, p = {tRes.pValueTwoTailed:E6}");
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
    private void InitializeParameterSets()
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
                OnlookerSelectionRatio = 0.5,
                ScoutSelectionRatio = 0.5,
                CoolingRate = 0.85,
                EnforceMutationUniqueness = false,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true,
            },
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.6,
                EliteSelectionRatio = 0.6,
                TotalPopulationGenerations = 100,
                MutationRate = 0.5,
                AllowMultipleInvalidInputs = false,
                OnlookerSelectionRatio = 0.5,
                ScoutSelectionRatio = 0.5,
                CoolingRate = 0.85,
                EnforceMutationUniqueness = false,
                EnableOnlookerSelection = false,
                EnableScoutPhase = false,
            },
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.5,
                EliteSelectionRatio = 0.5,
                TotalPopulationGenerations = 50,
                MutationRate = 0.45,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true,
            },
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.55,
                EliteSelectionRatio = 0.45,
                TotalPopulationGenerations = 50,
                MutationRate = 0.35,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true,
            },
            // Best general configuration
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
            // Stronger selection & refinement
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
            // Higher mutation rate
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
            // Balanced exploitation & diversity
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.5,
                EliteSelectionRatio = 0.6,
                TotalPopulationGenerations = 100,
                MutationRate = 0.7,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true
            },
            // More diverse test cases
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.4,
                EliteSelectionRatio = 0.6,
                TotalPopulationGenerations = 100,
                MutationRate = 0.8,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true
            },
            // Balanced mutation & selection
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.5,
                EliteSelectionRatio = 0.5,
                TotalPopulationGenerations = 100,
                MutationRate = 0.4,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true
            },
            // Maximum exploration
            new ABCGenerationSettings
            {
                FinalPopulationSelectionRatio = 0.4,
                EliteSelectionRatio = 0.5,
                TotalPopulationGenerations = 100,
                MutationRate = 0.4,
                AllowMultipleInvalidInputs = false,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true
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

        var avgAbc = abcList.Average();
        var avgPw = pwList.Average();
        var improvement = (avgAbc - avgPw) / avgPw * 100.0;

        var sdAbc = abcList.StandardDeviation();
        var sdPw = pwList.StandardDeviation();

        var seAbc = sdAbc / Math.Sqrt(abcList.Count);
        var sePw = sdPw / Math.Sqrt(pwList.Count);

        // t-distribution based 95% confidence intervals
        var (loAbc, hiAbc) = ConfidenceInterval95T(avgAbc, seAbc, df: abcList.Count - 1);
        var (loPw, hiPw) = ConfidenceInterval95T(avgPw, sePw, df: pwList.Count - 1);

        // Paired t-test on per-iteration differences
        var diffs = abcList.Zip(pwList, (a, b) => a - b).ToArray();
        var tRes = PairedTTest(diffs);

        Console.WriteLine($"\n========== Summary for Parameters: {paramSet} ==========");
        Console.WriteLine($"✅ ABC Mean ± SD: {avgAbc:F4} ± {sdAbc:F4}  (95% CI: [{loAbc:F4}, {hiAbc:F4}])");
        Console.WriteLine($"✅ Pairwise Mean ± SD: {avgPw:F4} ± {sdPw:F4}  (95% CI: [{loPw:F4}, {hiPw:F4}])");
        Console.WriteLine($"📈 Improvement Over Pairwise (mean): {improvement:F2}%");
        Console.WriteLine($"🧪 Paired t-test (ABC - Pairwise): t = {tRes.tStatistic:F4}, df = {tRes.degreesOfFreedom}, p = {tRes.pValueTwoTailed:E6}");
    }

    // === Statistical Analysis Helper Methods ===

    /// <summary>
    /// Calculates a 95% confidence interval using the t-distribution.
    /// 
    /// Formula: CI = mean ± t_critical * standard_error
    /// Where:
    /// - t_critical is the critical value from t-distribution at α/2 = 0.025 (for 95% CI)
    /// - standard_error = standard_deviation / √n
    /// - degrees_of_freedom = n - 1
    /// 
    /// The t-distribution is used instead of normal distribution when sample size is small (n < 30)
    /// or when population standard deviation is unknown, which provides more conservative estimates.
    /// </summary>
    /// <param name="mean">Sample mean</param>
    /// <param name="standardError">Standard error of the mean (SD/√n)</param>
    /// <param name="df">Degrees of freedom (n-1)</param>
    /// <returns>Tuple containing lower and upper bounds of 95% confidence interval</returns>
    private static (double lo, double hi) ConfidenceInterval95T(double mean, double standardError, int df)
    {
        if (df <= 0 || double.IsNaN(standardError) || double.IsInfinity(standardError))
            return (double.NaN, double.NaN);

        // Get critical t-value for two-tailed 95% confidence interval (α = 0.05, α/2 = 0.025)
        // Using 0.975 because we want the 97.5th percentile (leaving 2.5% in each tail)
        double tCrit = StudentT.InvCDF(0.0, 1.0, df, 0.975);
        double marginOfError = tCrit * standardError;
        return (mean - marginOfError, mean + marginOfError);
    }

    /// <summary>
    /// Performs a paired t-test to determine if there's a statistically significant difference 
    /// between two paired samples (e.g., ABC vs Pairwise scores for the same test configurations).
    /// 
    /// Formula: t = d̄ / (sd / √n)
    /// Where:
    /// - d̄ = mean of differences between paired observations
    /// - sd = sample standard deviation of differences
    /// - n = number of paired observations
    /// - df = n - 1 (degrees of freedom)
    /// 
    /// Null hypothesis (H₀): Mean difference = 0 (no significant difference)
    /// Alternative hypothesis (H₁): Mean difference ≠ 0 (significant difference exists)
    /// 
    /// P-value interpretation:
    /// - p < 0.05: Reject H₀, significant difference exists
    /// - p ≥ 0.05: Fail to reject H₀, no significant difference detected
    /// </summary>
    /// <param name="diffs">Array of paired differences (sample1[i] - sample2[i])</param>
    /// <returns>Tuple containing t-statistic, degrees of freedom, and two-tailed p-value</returns>
    private static (double tStatistic, int degreesOfFreedom, double pValueTwoTailed) PairedTTest(double[] diffs)
    {
        int n = diffs.Length;
        if (n <= 1) return (double.NaN, 0, double.NaN);

        double mean = diffs.Average();
        double sd = diffs.StandardDeviation(); // sample standard deviation (uses n-1 denominator)
        double se = sd / Math.Sqrt(n); // standard error of the mean difference
        
        // Handle case where all differences are identical (zero variance)
        if (se == 0) return (double.PositiveInfinity, n - 1, 0.0);

        // Calculate t-statistic: how many standard errors the mean difference is from zero
        double t = mean / se;
        int df = n - 1;

        // Calculate two-tailed p-value using t-distribution
        // P(|T| > |t|) = 2 * P(T > |t|) = 2 * (1 - CDF(|t|))
        var tDist = new StudentT(0.0, 1.0, df);
        double p = 2.0 * (1.0 - tDist.CumulativeDistribution(Math.Abs(t)));
        
        // Clamp p-value to valid range [0, 1] to handle numerical precision issues
        if (p < 0) p = 0;
        if (p > 1) p = 1;

        return (t, df, p);
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
        Console.WriteLine($"Achieved Avg Score: {bestABC.Value.Average()}");
    }

    private void PrintBestPairwisePerformance()
    {
        var bestPairwise = _pairwiseScores.OrderByDescending(p => p.Value.Average()).First();
        Console.WriteLine("\n========== Best Pairwise Performance ==========");
        Console.WriteLine($"Achieved Avg Score: {bestPairwise.Value.Average()} with ABC parameters: {bestPairwise.Key}");
    }
}
