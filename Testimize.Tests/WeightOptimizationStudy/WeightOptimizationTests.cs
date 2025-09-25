using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;
using NUnit.Framework;
using Testimize;
using Testimize.Contracts;
using Testimize.Parameters;
using Testimize.Parameters.Core;
using Testimize.TestCaseGenerators;
using Testimize.Usage;

namespace Testimize.Tests.WeightOptimizationStudy;

/// <summary>
/// Scientific study to validate the optimal weights for TestCaseEvaluator
/// using statistical analysis with t-tests and effect sizes.
/// </summary>
[TestFixture]
public class WeightOptimizationTests
{
    private const int RunsPerConfiguration = 30; // Number of runs for statistical validity
    private const double SignificanceLevel = 0.05; // p-value threshold

    /// <summary>
    /// Metrics collected for each test run
    /// </summary>
    private class TestRunMetrics
    {
        public int TestCaseCount { get; set; }
        public double CoverageRatio { get; set; }  // Unique combinations / Total possible
        public double DiversityScore { get; set; }  // Standard deviation of value distribution
        public double BoundaryRatio { get; set; }  // Boundary values / Total values
        public double OverallScore { get; set; }   // Composite metric
        public long ExecutionTimeMs { get; set; }
    }

    /// <summary>
    /// Statistical results for a weight configuration
    /// </summary>
    private class ConfigurationStats
    {
        public string Name { get; set; }
        public double MeanScore { get; set; }
        public double StandardDeviation { get; set; }
        public double TTestPValue { get; set; }
        public double EffectSize { get; set; }  // Cohen's d
        public double MeanTestCases { get; set; }
        public double MeanCoverage { get; set; }
        public double MeanDiversity { get; set; }
        public double MeanBoundary { get; set; }
    }

    [Test]
    public void CompareWeightConfigurations_StatisticalAnalysis()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("WEIGHT CONFIGURATION OPTIMIZATION STUDY");
        Console.WriteLine(new string('=', 80));

        // Step 1: Define weight configurations to test
        var configurations = new List<EvaluatorWeights>
        {
            EvaluatorWeights.Default(),        // Current baseline
            EvaluatorWeights.AllEqual(),       // All weights = 10
            EvaluatorWeights.NoDiversity(),    // No first-time bonus
            EvaluatorWeights.HighPenalty(),    // Higher invalid penalties
            EvaluatorWeights.BoundaryFocused(), // Triple boundary weights
            EvaluatorWeights.DiversityFocused(), // Double diversity bonus
            EvaluatorWeights.Conservative(),    // Lower rewards, higher penalties
            EvaluatorWeights.Aggressive()       // Higher rewards, lower penalties
        };

        // Step 2: Define test scenarios (using existing patterns from the codebase)
        var scenarios = CreateTestScenarios();

        // Step 3: Run experiments and collect metrics
        Console.WriteLine("\nRunning experiments...");
        var results = new Dictionary<EvaluatorWeights, List<TestRunMetrics>>();

        foreach (var config in configurations)
        {
            Console.WriteLine($"Testing configuration: {config.Name}");
            results[config] = RunExperiments(config, scenarios, RunsPerConfiguration);
        }

        // Step 4: Perform statistical analysis
        Console.WriteLine("\nPerforming statistical analysis...");
        var statistics = PerformStatisticalAnalysis(results);

        // Step 5: Generate and display report
        GenerateReport(statistics);

        // Step 6: Assert that current weights are optimal or near-optimal
        var currentStats = statistics.First(s => s.Name.Contains("Baseline"));
        var bestStats = statistics.OrderByDescending(s => s.MeanScore).First();

        Assert.That(currentStats.MeanScore, Is.GreaterThan(statistics.Average(s => s.MeanScore)),
            "Current weights should perform above average");

        // Current weights should be in top 2
        var rank = statistics.OrderByDescending(s => s.MeanScore)
            .Select((s, i) => new { Stats = s, Rank = i + 1 })
            .First(x => x.Stats.Name == currentStats.Name).Rank;

        Assert.That(rank, Is.LessThanOrEqualTo(2),
            $"Current weights should rank in top 2, but ranked {rank}");
    }

    private List<(string Name, List<IInputParameter> Parameters)> CreateTestScenarios()
    {
        var scenarios = new List<(string, List<IInputParameter>)>();

        // Scenario 1: Simple (3 text parameters) - similar to existing tests
        var simpleParams = new List<IInputParameter>
        {
            new TextDataParameter(preciseMode: true, minBoundary: 3, maxBoundary: 20, preciseTestValues: new[]
            {
                new TestValue("ABC", TestValueCategory.BoundaryValid),
                new TestValue("12345678901234567890", TestValueCategory.BoundaryValid),
                new TestValue("Normal Text", TestValueCategory.Valid),
                new TestValue("AB", TestValueCategory.BoundaryInvalid),
                new TestValue("", TestValueCategory.Invalid)
            }),
            new TextDataParameter(preciseMode: true, minBoundary: 5, maxBoundary: 15, preciseTestValues: new[]
            {
                new TestValue("ABCDE", TestValueCategory.BoundaryValid),
                new TestValue("123456789012345", TestValueCategory.BoundaryValid),
                new TestValue("Medium", TestValueCategory.Valid),
                new TestValue("ABCD", TestValueCategory.BoundaryInvalid)
            }),
            new TextDataParameter(preciseMode: true, minBoundary: 1, maxBoundary: 10, preciseTestValues: new[]
            {
                new TestValue("A", TestValueCategory.BoundaryValid),
                new TestValue("1234567890", TestValueCategory.BoundaryValid),
                new TestValue("Short", TestValueCategory.Valid)
            })
        };
        scenarios.Add(("Simple (3 text)", simpleParams));

        // Scenario 2: Medium (5 mixed parameters) - based on form validation patterns
        var mediumParams = new List<IInputParameter>
        {
            new TextDataParameter(preciseMode: true, minBoundary: 3, maxBoundary: 20, preciseTestValues: new[]
            {
                new TestValue("Ann", TestValueCategory.BoundaryValid),
                new TestValue("Anton Angelov", TestValueCategory.Valid),
                new TestValue("", TestValueCategory.Invalid)
            }),
            new EmailDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("a@e.io", TestValueCategory.BoundaryValid),
                new TestValue("anton@example.com", TestValueCategory.Valid),
                new TestValue("invalid", TestValueCategory.Invalid)
            }),
            new PhoneDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("+3598888", TestValueCategory.BoundaryValid),
                new TestValue("+359888888888", TestValueCategory.Valid),
                new TestValue("123", TestValueCategory.Invalid)
            }),
            new IntegerDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue(18, TestValueCategory.BoundaryValid),
                new TestValue(25, TestValueCategory.Valid),
                new TestValue(100, TestValueCategory.BoundaryValid),
                new TestValue(17, TestValueCategory.BoundaryInvalid)
            }),
            new BooleanDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue(true, TestValueCategory.Valid),
                new TestValue(false, TestValueCategory.Invalid)
            })
        };
        scenarios.Add(("Medium (5 mixed)", mediumParams));

        // Scenario 3: Complex (8 mixed parameters) - comprehensive form without URL
        var complexParams = new List<IInputParameter>
        {
            new TextDataParameter(preciseMode: true, minBoundary: 3, maxBoundary: 20, preciseTestValues: new[]
            {
                new TestValue("John Doe", TestValueCategory.Valid),
                new TestValue("", TestValueCategory.Invalid)
            }),
            new EmailDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("user@example.com", TestValueCategory.Valid),
                new TestValue("notanemail", TestValueCategory.Invalid)
            }),
            new PhoneDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("+1234567890", TestValueCategory.Valid),
                new TestValue("", TestValueCategory.Invalid)
            }),
            new PasswordDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("Secure1!", TestValueCategory.Valid),
                new TestValue("weak", TestValueCategory.Invalid)
            }),
            new IntegerDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue(50, TestValueCategory.Valid),
                new TestValue(101, TestValueCategory.BoundaryInvalid)
            }),
            new DateDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue(DateTime.Parse("1990-01-01"), TestValueCategory.Valid),
                new TestValue(DateTime.Parse("2021-01-01"), TestValueCategory.BoundaryInvalid)
            }),
            new BooleanDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue(true, TestValueCategory.Valid),
                new TestValue(false, TestValueCategory.Invalid)
            }),
            new SingleSelectDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("Option1", TestValueCategory.Valid),
                new TestValue("Option2", TestValueCategory.Valid),
                new TestValue(null, TestValueCategory.Invalid)
            })
        };
        scenarios.Add(("Complex (8 mixed)", complexParams));

        return scenarios;
    }

    private List<TestRunMetrics> RunExperiments(
        EvaluatorWeights weights,
        List<(string Name, List<IInputParameter> Parameters)> scenarios,
        int runsPerScenario)
    {
        var allMetrics = new List<TestRunMetrics>();

        foreach (var scenario in scenarios)
        {
            for (int run = 0; run < runsPerScenario / scenarios.Count; run++) // Divide runs across scenarios
            {
                var sw = Stopwatch.StartNew();

                // Use ABC generator with the custom evaluator
                var settings = new ABCGenerationSettings
                {
                    Seed = 42 + run, // Different seed for each run
                    TotalPopulationGenerations = 20,
                    FinalPopulationSelectionRatio = 0.5,
                    TestCaseEvaluator = new ParameterizedTestCaseEvaluator(weights),
                    TestCaseGenerator = new PairwiseTestCaseGenerator()
                };

                var generator = new HybridArtificialBeeColonyTestCaseGenerator(settings);
                var testCases = generator.RunABCAlgorithm(scenario.Parameters);

                sw.Stop();

                // Calculate metrics
                var metrics = CalculateMetrics(testCases, scenario.Parameters);
                metrics.ExecutionTimeMs = sw.ElapsedMilliseconds;

                allMetrics.Add(metrics);
            }
        }

        return allMetrics;
    }

    private TestRunMetrics CalculateMetrics(HashSet<TestCase> testCases, List<IInputParameter> parameters)
    {
        var metrics = new TestRunMetrics
        {
            TestCaseCount = testCases.Count
        };

        if (testCases.Count == 0)
        {
            return metrics;
        }

        // Calculate coverage ratio
        var uniqueCombinations = new HashSet<string>();
        foreach (var tc in testCases)
        {
            var combination = string.Join(",", tc.Values.Select(v => v.Value?.ToString() ?? "null"));
            uniqueCombinations.Add(combination);
        }
        metrics.CoverageRatio = (double)uniqueCombinations.Count / Math.Max(testCases.Count, 1);

        // Calculate diversity score (standard deviation of value usage)
        var valueUsage = new Dictionary<string, int>(); // Use string key to avoid null issues
        foreach (var tc in testCases)
        {
            foreach (var value in tc.Values)
            {
                var key = value.Value?.ToString() ?? "null";
                if (!valueUsage.ContainsKey(key))
                    valueUsage[key] = 0;
                valueUsage[key]++;
            }
        }
        if (valueUsage.Count > 0)
        {
            metrics.DiversityScore = valueUsage.Values.Select(v => (double)v).StandardDeviation();
        }

        // Calculate boundary ratio
        int totalValues = 0;
        int boundaryValues = 0;
        foreach (var tc in testCases)
        {
            foreach (var value in tc.Values)
            {
                totalValues++;
                if (value.Category == TestValueCategory.BoundaryValid ||
                    value.Category == TestValueCategory.BoundaryInvalid)
                {
                    boundaryValues++;
                }
            }
        }
        metrics.BoundaryRatio = totalValues > 0 ? (double)boundaryValues / totalValues : 0;

        // Calculate overall score (weighted combination)
        metrics.OverallScore =
            metrics.CoverageRatio * 0.4 +
            (1.0 / (1.0 + metrics.DiversityScore)) * 0.3 +  // Lower diversity score is better
            metrics.BoundaryRatio * 0.3;

        return metrics;
    }

    private List<ConfigurationStats> PerformStatisticalAnalysis(
        Dictionary<EvaluatorWeights, List<TestRunMetrics>> results)
    {
        var statistics = new List<ConfigurationStats>();

        // Get baseline (current weights) for comparison
        var baseline = results.First(r => r.Key.Name.Contains("Baseline"));
        var baselineScores = baseline.Value.Select(m => m.OverallScore).ToArray();

        foreach (var result in results)
        {
            var scores = result.Value.Select(m => m.OverallScore).ToArray();

            var stats = new ConfigurationStats
            {
                Name = result.Key.Name,
                MeanScore = scores.Mean(),
                StandardDeviation = scores.StandardDeviation(),
                MeanTestCases = result.Value.Average(m => m.TestCaseCount),
                MeanCoverage = result.Value.Average(m => m.CoverageRatio),
                MeanDiversity = result.Value.Average(m => m.DiversityScore),
                MeanBoundary = result.Value.Average(m => m.BoundaryRatio)
            };

            // Perform t-test against baseline
            if (result.Key != baseline.Key)
            {
                // Two-sample t-test
                var (statistic, pValue) = PerformTTest(baselineScores, scores);
                stats.TTestPValue = pValue;

                // Calculate Cohen's d effect size
                stats.EffectSize = CalculateCohenD(baselineScores, scores);
            }
            else
            {
                stats.TTestPValue = 1.0; // Baseline compared to itself
                stats.EffectSize = 0.0;
            }

            statistics.Add(stats);
        }

        return statistics;
    }

    private (double statistic, double pValue) PerformTTest(double[] sample1, double[] sample2)
    {
        // Using Welch's t-test (unequal variances)
        var n1 = sample1.Length;
        var n2 = sample2.Length;
        var mean1 = sample1.Mean();
        var mean2 = sample2.Mean();
        var var1 = sample1.Variance();
        var var2 = sample2.Variance();

        var statistic = (mean1 - mean2) / Math.Sqrt(var1 / n1 + var2 / n2);

        // Calculate degrees of freedom for Welch's t-test
        var df = Math.Pow(var1 / n1 + var2 / n2, 2) /
                (Math.Pow(var1 / n1, 2) / (n1 - 1) + Math.Pow(var2 / n2, 2) / (n2 - 1));

        // Calculate p-value using Student's t-distribution
        var tDist = new StudentT(0, 1, df);
        var pValue = 2 * (1 - tDist.CumulativeDistribution(Math.Abs(statistic)));

        return (statistic, pValue);
    }

    private double CalculateCohenD(double[] sample1, double[] sample2)
    {
        var mean1 = sample1.Mean();
        var mean2 = sample2.Mean();
        var var1 = sample1.Variance();
        var var2 = sample2.Variance();
        var n1 = sample1.Length;
        var n2 = sample2.Length;

        // Pooled standard deviation
        var pooledStd = Math.Sqrt(((n1 - 1) * var1 + (n2 - 1) * var2) / (n1 + n2 - 2));

        return Math.Abs(mean1 - mean2) / pooledStd;
    }

    private void GenerateReport(List<ConfigurationStats> statistics)
    {
        var sb = new StringBuilder();

        sb.AppendLine("\n" + new string('=', 110));
        sb.AppendLine("WEIGHT CONFIGURATION COMPARISON RESULTS");
        sb.AppendLine(new string('=', 110));
        sb.AppendLine();
        sb.AppendLine("Statistical Analysis (30 runs per configuration, 3 scenarios)");
        sb.AppendLine(new string('-', 110));

        // Header
        sb.AppendLine($"{"Configuration",-25} {"Mean Score",12} {"Std Dev",10} {"p-value",10} {"Effect Size",12} {"Interpretation",20}");
        sb.AppendLine(new string('-', 110));

        // Sort by mean score descending
        var sorted = statistics.OrderByDescending(s => s.MeanScore).ToList();

        foreach (var stat in sorted)
        {
            var pValueStr = stat.TTestPValue < 1.0 ?
                (stat.TTestPValue < 0.001 ? "<0.001" : stat.TTestPValue.ToString("F3")) :
                "-";

            var effectStr = GetEffectSizeInterpretation(stat.EffectSize);
            var significant = stat.TTestPValue < SignificanceLevel ? "*" : " ";

            sb.AppendLine($"{stat.Name,-25} {stat.MeanScore,12:F4} {stat.StandardDeviation,10:F4} " +
                         $"{pValueStr,10} {stat.EffectSize,12:F3} {effectStr,-19}{significant}");
        }

        sb.AppendLine(new string('-', 110));
        sb.AppendLine("* Statistically significant difference from baseline (p < 0.05)");
        sb.AppendLine();

        // Detailed metrics table
        sb.AppendLine("\nDetailed Metrics:");
        sb.AppendLine(new string('-', 110));
        sb.AppendLine($"{"Configuration",-25} {"Test Cases",12} {"Coverage",10} {"Diversity",10} {"Boundary",10}");
        sb.AppendLine(new string('-', 110));

        foreach (var stat in sorted)
        {
            sb.AppendLine($"{stat.Name,-25} {stat.MeanTestCases,12:F1} {stat.MeanCoverage,10:F3} " +
                         $"{stat.MeanDiversity,10:F3} {stat.MeanBoundary,10:F3}");
        }

        sb.AppendLine(new string('-', 110));

        // Summary
        sb.AppendLine("\nSUMMARY:");
        sb.AppendLine(new string('=', 110));

        var best = sorted.First();
        var baseline = statistics.First(s => s.Name.Contains("Baseline"));
        var baselineRank = sorted.FindIndex(s => s.Name.Contains("Baseline")) + 1;

        sb.AppendLine($"Best performing configuration: {best.Name} (Score: {best.MeanScore:F4})");
        sb.AppendLine($"Current (Baseline) ranking: #{baselineRank} of {sorted.Count} (Score: {baseline.MeanScore:F4})");

        if (baselineRank <= 2)
        {
            sb.AppendLine("✅ Current weights are OPTIMAL or near-optimal");
        }
        else if (baselineRank <= sorted.Count / 2)
        {
            sb.AppendLine("⚠️ Current weights are ACCEPTABLE but could be improved");
        }
        else
        {
            sb.AppendLine("❌ Current weights are SUBOPTIMAL and should be reconsidered");
        }

        sb.AppendLine();
        sb.AppendLine($"95% Confidence Interval for baseline: [{baseline.MeanScore - 1.96 * baseline.StandardDeviation / Math.Sqrt(30):F4}, " +
                     $"{baseline.MeanScore + 1.96 * baseline.StandardDeviation / Math.Sqrt(30):F4}]");

        var report = sb.ToString();
        Console.WriteLine(report);

        // Also write to debug output for CI/CD visibility
        Debug.WriteLine(report);
    }

    private string GetEffectSizeInterpretation(double cohenD)
    {
        var absD = Math.Abs(cohenD);
        if (absD < 0.2) return "Negligible";
        if (absD < 0.5) return "Small";
        if (absD < 0.8) return "Medium";
        return "Large";
    }
}