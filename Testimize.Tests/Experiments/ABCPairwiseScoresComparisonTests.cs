// <copyright file="ABCPairwiseScoresComparisonTests.cs" company="Automate The Planet Ltd.">
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

using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;

namespace Testimize.Tests.Experiments;

[TestFixture]
public class ABCOptimizationBenchmarkTests
{
    private const int Iterations = 10;
    private List<IInputParameter> _parameters;

    // 🔹 Fixed list of seeds for variance analysis
    private static readonly int[] Seeds = Enumerable.Range(0, 10).ToArray(); // 0..9

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
    public void RunOptimizationBenchmark_VarianceBySeed()
    {
        Console.WriteLine("\n========== Running ABC Variance by Seed (additional reproducibility check) ==========");

        foreach (var paramSet in _parameterSets)
        {
            var abc = new List<double>();
            var pw = new List<double>();

            // Pairwise is deterministic; compute once per paramSet (topCount depends on paramSet)
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
                pw.Add(pairwiseTotalScore); // same baseline per seed
            }

            // Stats (MathNet-based)
            var avgAbc = abc.Average();
            var sdAbc = StatsEx.StdDevSample(abc);
            var seAbc = sdAbc / Math.Sqrt(abc.Count);
            var (loAbc, hiAbc) = StatsEx.ConfidenceInterval95(avgAbc, seAbc, abc.Count - 1);

            var avgPw = pw.Average();
            var sdPw = StatsEx.StdDevSample(pw);
            var sePw = sdPw / Math.Sqrt(pw.Count);
            var (loPw, hiPw) = StatsEx.ConfidenceInterval95(avgPw, sePw, pw.Count - 1);

            var improvement = (avgAbc - avgPw) / avgPw * 100.0;
            var diffs = abc.Zip(pw, (a, b) => a - b).ToArray();
            var tRes = StatsEx.PairedTTest(diffs);

            Console.WriteLine($"\n========== Variance by Seed for: {paramSet} ==========");
            Console.WriteLine($"ABC Mean ± SD: {avgAbc:F4} ± {sdAbc:F4}  (95% CI: [{loAbc:F4}, {hiAbc:F4}])");
            Console.WriteLine($"PW  Mean ± SD: {avgPw:F4} ± {sdPw:F4}  (95% CI: [{loPw:F4}, {hiPw:F4}])");
            Console.WriteLine($"Δ Improvement (mean): {improvement:F2}%");
            Console.WriteLine($"Paired t-test: t = {tRes.tStatistic:F4}, df = {tRes.degreesOfFreedom}, p = {tRes.pValueTwoTailed:E6}");
        }
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

        var testCaseEvaluator = new TestCaseEvaluator();
        var abcScores = testCaseEvaluator.EvaluatePopulationToDictionary(abcTestCases);
        double abcTotalScore = abcScores.Values.Sum();

        var topCount = (int)(_sortedPairwiseScores.Count * config.FinalPopulationSelectionRatio);
        var pairwiseTotalScore = _sortedPairwiseScores.Take(topCount).Sum(p => p.Score);
        _pairwiseScores[config].Add(pairwiseTotalScore);

        return abcTotalScore;
    }

    // 🔹 Print results for each ABC parameter set (with SD, 95% CI, paired t-test via MathNet)
    private void PrintResultsForParameterSet(ABCGenerationSettings paramSet)
    {
        var abcList = _abcScores[paramSet].ToArray();
        var pwList = _pairwiseScores[paramSet].ToArray();

        var avgAbcScore = abcList.Average();
        var avgPairwiseScore = pwList.Average();
        var percentageImprovement = (avgAbcScore - avgPairwiseScore) / avgPairwiseScore * 100;

        var abcSd = StatsEx.StdDevSample(abcList);
        var pwSd = StatsEx.StdDevSample(pwList);
        var abcSe = abcSd / Math.Sqrt(abcList.Length);
        var pwSe = pwSd / Math.Sqrt(pwList.Length);

        var (abcLo, abcHi) = StatsEx.ConfidenceInterval95(avgAbcScore, abcSe, abcList.Length - 1);
        var (pwLo, pwHi) = StatsEx.ConfidenceInterval95(avgPairwiseScore, pwSe, pwList.Length - 1);

        // Paired t-test on per-iteration differences
        var diffs = abcList.Zip(pwList, (a, b) => a - b).ToArray();
        var tRes = StatsEx.PairedTTest(diffs);

        Console.WriteLine($"\n========== Summary for Parameters: {paramSet} ==========");
        Console.WriteLine($"✅ ABC Mean ± SD: {avgAbcScore:F4} ± {abcSd:F4}  (95% CI: [{abcLo:F4}, {abcHi:F4}])");
        Console.WriteLine($"✅ Pairwise Mean ± SD: {avgPairwiseScore:F4} ± {pwSd:F4}  (95% CI: [{pwLo:F4}, {pwHi:F4}])");
        Console.WriteLine($"📈 Improvement Over Pairwise (mean): {percentageImprovement:F2}%");

        Console.WriteLine($"🧪 Paired t-test (ABC - Pairwise): t = {tRes.tStatistic:F4}, df = {tRes.degreesOfFreedom}, p = {tRes.pValueTwoTailed:E6}");
        Console.WriteLine($"   (Interpretation tip: p < 0.05 → statistical significant difference)");

        Debug.WriteLine($"\n========== Summary for Parameters: {paramSet} ==========");
        Debug.WriteLine($"ABC Mean ± SD: {avgAbcScore:F4} ± {abcSd:F4}  (95% CI: [{abcLo:F4}, {abcHi:F4}])");
        Debug.WriteLine($"Pairwise Mean ± SD: {avgPairwiseScore:F4} ± {pwSd:F4}  (95% CI: [{pwLo:F4}, {pwHi:F4}])");
        Debug.WriteLine($"Improvement Over Pairwise (mean): {percentageImprovement:F2}%");
        Debug.WriteLine($"Paired t-test: t = {tRes.tStatistic:F4}, df = {tRes.degreesOfFreedom}, p = {tRes.pValueTwoTailed:E6}");
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

/// <summary>
/// MathNet-backed helpers (sample SD/variance, 95% CI, paired t-test with Student's t CDF)
/// </summary>
internal static class StatsEx
{
    // Sample variance (n-1)
    public static double VarianceSample(IEnumerable<double> values)
        => ArrayStatistics.Variance(values.ToArray());

    // Sample SD (n-1)
    public static double StdDevSample(IEnumerable<double> values)
        => ArrayStatistics.StandardDeviation(values.ToArray());

    public static (double lo, double hi) ConfidenceInterval95(double mean, double standardError, int df)
    {
        // two-tailed t critical for 95% CI
        double tCrit = StudentT.InvCDF(0.0, 1.0, df, 0.975);
        double half = tCrit * standardError;
        return (mean - half, mean + half);
    }

    /// <summary>
    /// Paired t-test on differences d_i = A_i - B_i with exact p-value via Student's t CDF.
    /// </summary>
    public static (double tStatistic, int degreesOfFreedom, double pValueTwoTailed) PairedTTest(double[] diffs)
    {
        int n = diffs.Length;
        if (n <= 1) return (double.NaN, 0, double.NaN);

        double mean = diffs.Mean();
        double sd = ArrayStatistics.StandardDeviation(diffs); // sample SD
        double se = sd / Math.Sqrt(n);
        if (se == 0) return (double.PositiveInfinity, n - 1, 0.0);

        double t = mean / se;
        int df = n - 1;

        var tDist = new StudentT(0.0, 1.0, df);       // mean=0, scale=1, df
        double p = 2.0 * (1.0 - tDist.CumulativeDistribution(Math.Abs(t))); // two-tailed

        // clamp
        if (p < 0) p = 0;
        if (p > 1) p = 1;

        return (t, df, p);
    }
}
