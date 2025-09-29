// <copyright file="ComprehensiveWeightOptimizationTests.cs" company="Automate The Planet Ltd.">
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Testimize.Contracts;
using Testimize.Parameters;
using Testimize.Parameters.Core;
using Testimize.TestCaseGenerators;

namespace Testimize.Tests.WeightOptimizationStudy;

[TestFixture]
public partial class ComprehensiveWeightOptimizationTests
{
    private const int RunsPerConfiguration = 10;
    private const double SignificanceLevel = 0.05;
    private const bool EnableDiagnostics = true;
    private const bool EnableDetailedValidation = true;

    private Dictionary<string, List<IInputParameter>> _testScenarios = default!;
    private List<ABCGenerationSettings> _abcConfigurations = default!;

    private readonly ConcurrentDictionary<(string Config, string Scenario, string ABC), ConcurrentBag<double>> _scoreDistributions = new();
    private readonly ConcurrentDictionary<(string Config, string Scenario, string ABC), ConcurrentBag<int>> _testCountDistributions = new();

    private readonly ConcurrentDictionary<string, ConcurrentBag<double>> _abcScoresByConfig = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<double>> _coverageByConfig = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<double>> _diversityByConfig = new();

    // Composite weight schemes for multi-criteria evaluation
    private readonly Dictionary<string, CompositeWeightScheme> _compositeWeightSchemes = new()
    {
        ["Balanced"] = new CompositeWeightScheme
        {
            Name = "Balanced",
            CoverageWeight = 0.25,
            DiversityWeight = 0.20,
            BoundaryWeight = 0.20,
            ValidWeight = 0.15,
            InvalidPenalty = 0.10,
            TestCountWeight = 0.10,
            Description = "Equal emphasis across all quality dimensions"
        },
        ["Coverage-Focused"] = new CompositeWeightScheme
        {
            Name = "Coverage-Focused",
            CoverageWeight = 0.40,
            DiversityWeight = 0.20,
            BoundaryWeight = 0.15,
            ValidWeight = 0.10,
            InvalidPenalty = 0.05,
            TestCountWeight = 0.10,
            Description = "Prioritizes comprehensive parameter space exploration"
        },
        ["Boundary-Focused"] = new CompositeWeightScheme
        {
            Name = "Boundary-Focused",
            CoverageWeight = 0.20,
            DiversityWeight = 0.15,
            BoundaryWeight = 0.35,
            ValidWeight = 0.15,
            InvalidPenalty = 0.10,
            TestCountWeight = 0.05,
            Description = "Emphasizes edge cases where faults typically cluster"
        },
        ["Efficiency-Focused"] = new CompositeWeightScheme
        {
            Name = "Efficiency-Focused",
            CoverageWeight = 0.15,
            DiversityWeight = 0.15,
            BoundaryWeight = 0.15,
            ValidWeight = 0.20,
            InvalidPenalty = 0.15,
            TestCountWeight = 0.20,
            Description = "Optimizes for minimal test suite size"
        },
        ["Quality-Focused"] = new CompositeWeightScheme
        {
            Name = "Quality-Focused",
            CoverageWeight = 0.30,
            DiversityWeight = 0.30,
            BoundaryWeight = 0.20,
            ValidWeight = 0.10,
            InvalidPenalty = 0.05,
            TestCountWeight = 0.05,
            Description = "Prioritizes executable test cases with proper validation"
        },
    };

    [SetUp]
    public void SetUp()
    {
        InitializeTestScenarios();
        InitializeABCConfigurations();
    }

    [Test]
    [Category("WeightOptimization")]
    [Category("V4-Final")]
    public void Test_WeightConfigurations_ComprehensiveAnalysis_V4()
    {
        Console.WriteLine("\n" + new string('=', 120));
        Console.WriteLine("COMPREHENSIVE WEIGHT CONFIGURATION ANALYSIS V4 (FINAL)");
        Console.WriteLine(new string('=', 120));

        var stopwatch = Stopwatch.StartNew();

        // Get all weight configurations
        var configurations = new List<EvaluatorWeightsFactory>
        {
            EvaluatorWeightsFactory.Default(),
            EvaluatorWeightsFactory.HighPenalty(),
            EvaluatorWeightsFactory.Aggressive(),
            EvaluatorWeightsFactory.Conservative(),
            EvaluatorWeightsFactory.BoundaryFocused(),
            EvaluatorWeightsFactory.DiversityFocused(),
            EvaluatorWeightsFactory.NoDiversity(),
            EvaluatorWeightsFactory.AllEqual(),
        };

        // Validate weight differences
        if (EnableDetailedValidation)
        {
            ValidateWeightDifferences(configurations);
        }

        DisplayExperimentConfiguration(configurations);

        // Run experiments with single-layer parallelism
        var results = RunExperimentsSingleParallelLayer(configurations);

        Console.WriteLine($"\n✓ Experiments completed in {stopwatch.Elapsed.TotalMinutes:F2} minutes");
        Console.WriteLine($"  Total test generations: {results.Count}");
        Console.WriteLine($"  Average time per generation: {results.Average(r => r.GenerationTimeMs):F1}ms");

        // Display diagnostics
        if (EnableDiagnostics)
        {
            DisplayDiagnosticInfo();
            DisplayABCScoreAnalysis();
        }

        // Calculate and analyze statistics
        var statistics = CalculateComprehensiveStatistics(results);

        // Perform various analyses
        PerformSensitivityAnalysis(statistics);
        DisplayDetailedResults(statistics);
        PerformStatisticalTests(statistics);
        DisplayRankingAnalysis(statistics);
        GenerateRecommendations(statistics);

        stopwatch.Stop();
        Console.WriteLine($"\nTotal analysis time: {stopwatch.Elapsed.TotalMinutes:F2} minutes");

        // Final validation
        if (EnableDetailedValidation)
        {
            ValidateFinalResults(statistics);
        }
    }

    private void DisplayExperimentConfiguration(List<EvaluatorWeightsFactory> configurations)
    {
        Console.WriteLine($"\nExperiment Configuration:");
        Console.WriteLine($"  • Weight Configurations: {configurations.Count}");
        Console.WriteLine($"  • Test Scenarios: {_testScenarios.Count} ({string.Join(", ", _testScenarios.Keys)})");
        Console.WriteLine($"  • ABC Convergence Levels: {_abcConfigurations.Count}");
        Console.WriteLine($"  • Runs per Combination: {RunsPerConfiguration}");
        Console.WriteLine($"  • Total Experiments: {configurations.Count * _testScenarios.Count * _abcConfigurations.Count * RunsPerConfiguration}");
        Console.WriteLine($"  • Composite Evaluation Schemes: {_compositeWeightSchemes.Count}");
        Console.WriteLine($"  • Parallel Execution: {Environment.ProcessorCount} cores");
        Console.WriteLine($"  • Significance Level: α = {SignificanceLevel}");
    }

    private void ValidateWeightDifferences(List<EvaluatorWeightsFactory> configurations)
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("WEIGHT CONFIGURATION VALIDATION");
        Console.WriteLine(new string('=', 100));

        // Create sample test case with all value categories
        var sampleTestCase = new TestCase
        {
            Values = new List<TestValue>
            {
                new("boundary_valid", TestValueCategory.BoundaryValid),
                new("normal_valid", TestValueCategory.Valid),
                new("boundary_invalid", TestValueCategory.BoundaryInvalid),
                new("invalid", TestValueCategory.Invalid),
                new("duplicate_boundary_valid", TestValueCategory.BoundaryValid),
            }
        };

        var emptyPopulation = new HashSet<TestCase>();

        Console.WriteLine("\nSample Test Case Scores with Different Weights:");
        Console.WriteLine(new string('-', 100));
        Console.WriteLine($"{"Configuration",-25} {"Score",-15} {"Relative to Default",-20} {"% Difference",-15}");
        Console.WriteLine(new string('-', 100));

        double? defaultScore = null;
        var scores = new Dictionary<string, double>();

        foreach (var config in configurations)
        {
            var evaluator = new ParameterizedTestCaseEvaluator(config, allowMultipleInvalidInputs: false);
            var score = evaluator.Evaluate(sampleTestCase, emptyPopulation);
            scores[config.Name] = score;

            if (config.Name.Contains("Baseline", StringComparison.OrdinalIgnoreCase) ||
                config.Name.Contains("Default", StringComparison.OrdinalIgnoreCase) ||
                config.Name == "Current (Baseline)")
            {
                defaultScore = score;
            }

            var relative = defaultScore.HasValue ? $"{(score - defaultScore.Value):+0.00;-0.00;0}" : "N/A";
            var percentDiff = defaultScore.HasValue && defaultScore.Value != 0
                ? $"{((score - defaultScore.Value) / Math.Abs(defaultScore.Value)) * 100:+0.0;-0.0;0}%"
                : "N/A";

            Console.WriteLine($"{config.Name,-25} {score,-15:F2} {relative,-20} {percentDiff,-15}");
        }

        // Validate uniqueness
        var uniqueScores = scores.Values.Distinct().Count();
        var minScore = scores.Values.Min();
        var maxScore = scores.Values.Max();
        var scoreRange = maxScore - minScore;

        Console.WriteLine($"\nValidation Summary:");
        Console.WriteLine($"  • Unique scores: {uniqueScores}/{configurations.Count}");
        Console.WriteLine($"  • Score range: {scoreRange:F2} (min: {minScore:F2}, max: {maxScore:F2})");

        if (uniqueScores < configurations.Count * 0.5)
        {
            Console.WriteLine($"  ⚠ WARNING: Only {uniqueScores} unique scores for {configurations.Count} configurations!");
            Console.WriteLine($"    This may indicate insufficient weight differentiation.");
        }
        else
        {
            Console.WriteLine($"  ✓ Validation passed: Good weight differentiation detected");
        }

        // Display weight configuration details
        Console.WriteLine("\nWeight Configuration Details:");
        Console.WriteLine(new string('-', 120));
        Console.WriteLine($"{"Config",-25} {"BoundValid",-12} {"Valid",-10} {"BoundInvalid",-14} {"Invalid",-10} {"FirstTime",-12} {"Multiplier",-12}");
        Console.WriteLine(new string('-', 120));

        foreach (var config in configurations)
        {
            Console.WriteLine($"{config.Name,-25} {config.BoundaryValidWeight,-12:+0.0;-0.0;0} " +
                            $"{config.ValidWeight,-10:+0.0;-0.0;0} {config.BoundaryInvalidWeight,-14:+0.0;-0.0;0} " +
                            $"{config.InvalidWeight,-10:+0.0;-0.0;0} {config.FirstTimeValueBonus,-12:+0.0;-0.0;0} " +
                            $"{config.FirstTimeMultiplierIncrement,-12:F2}");
        }
    }

    private ConcurrentBag<TestRunMetrics> RunExperimentsSingleParallelLayer(List<EvaluatorWeightsFactory> configurations)
    {
        var allResults = new ConcurrentBag<TestRunMetrics>();
        var totalCombinations = configurations.Count * _testScenarios.Count * _abcConfigurations.Count;
        var completed = 0;
        var progressLock = new object();

        Console.WriteLine($"\nRunning {totalCombinations * RunsPerConfiguration} experiments...");

        // Pre-compose all test combinations
        var testCombinations = (
            from config in configurations
            from scenario in _testScenarios
            from abcIndex in Enumerable.Range(0, _abcConfigurations.Count)
            select new
            {
                Config = config,
                ScenarioName = scenario.Key,
                Parameters = scenario.Value,
                ABC = _abcConfigurations[abcIndex],
                ABCName = GetABCConfigName(abcIndex)
            }
        ).ToList();

        // Use single-layer parallelism with controlled degree
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        Parallel.ForEach(testCombinations, options, combo =>
        {
            // Run experiments sequentially within each parallel task
            for (int runIndex = 0; runIndex < RunsPerConfiguration; runIndex++)
            {
                var metrics = RunSingleExperiment(
                    combo.Config,
                    combo.Parameters,
                    combo.ScenarioName,
                    combo.ABC,
                    combo.ABCName,
                    runIndex
                );

                allResults.Add(metrics);

                // Collect diagnostics (thread-safe)
                var key = (combo.Config.Name, combo.ScenarioName, combo.ABCName);
                _scoreDistributions.GetOrAdd(key, _ => new ConcurrentBag<double>()).Add(metrics.ABCScoreMean);
                _testCountDistributions.GetOrAdd(key, _ => new ConcurrentBag<int>()).Add(metrics.TestCount);

                // Additional diagnostics
                _abcScoresByConfig.GetOrAdd(combo.Config.Name, _ => new ConcurrentBag<double>()).Add(metrics.ABCScoreMean);
                _coverageByConfig.GetOrAdd(combo.Config.Name, _ => new ConcurrentBag<double>()).Add(metrics.Coverage);
                _diversityByConfig.GetOrAdd(combo.Config.Name, _ => new ConcurrentBag<double>()).Add(metrics.DiversityScore);
            }

            // Update progress
            lock (progressLock)
            {
                completed++;
                if (completed % 5 == 0 || completed == totalCombinations)
                {
                    var percentage = 100.0 * completed / totalCombinations;
                    Console.Write($"\rProgress: {completed}/{totalCombinations} ({percentage:F1}%)");
                }
            }
        });

        Console.WriteLine("\r" + new string(' ', 50) + "\r"); // Clear progress line
        return allResults;
    }

    private TestRunMetrics RunSingleExperiment(
        EvaluatorWeightsFactory config,
        List<IInputParameter> parameters,
        string scenarioName,
        ABCGenerationSettings abcSettings,
        string abcConfigName,
        int runIndex)
    {
        var sw = Stopwatch.StartNew();

        // Clone settings and set stable seed
        var settings = CloneABCSettings(abcSettings);
        var seedString = $"{config.Name}|{scenarioName}|{abcConfigName}|{runIndex}";
        settings.Seed = StableSeed(seedString);

        // Use parameterized evaluator with custom weights
        var evaluator = new ParameterizedTestCaseEvaluator(config, abcSettings.AllowMultipleInvalidInputs);
        settings.TestCaseEvaluator = evaluator;
        settings.TestCaseGenerator = new PairwiseTestCaseGenerator();

        // Run ABC algorithm
        var generator = new HybridArtificialBeeColonyTestCaseGenerator(settings);
        var testCases = generator.RunABCAlgorithm(parameters);

        sw.Stop();

        // Calculate ABC scores from generator output (not re-evaluation)
        double sumScore = 0;
        double minScore = double.MaxValue;
        double maxScore = double.MinValue;

        foreach (var tc in testCases)
        {
            var score = tc.Score; // Use the score assigned by generator
            sumScore += score;
            minScore = Math.Min(minScore, score);
            maxScore = Math.Max(maxScore, score);
        }

        var meanScore = testCases.Count > 0 ? sumScore / testCases.Count : 0.0;

        // Calculate detailed metrics
        var metrics = CalculateMetrics(testCases, parameters);
        metrics.ConfigName = config.Name;
        metrics.ScenarioName = scenarioName;
        metrics.ABCConfigName = abcConfigName;
        metrics.GenerationTimeMs = sw.ElapsedMilliseconds;
        metrics.UsedSeed = settings.Seed;

        // ABC score metrics from generator
        metrics.ABCScoreSum = sumScore;
        metrics.ABCScoreMean = meanScore;
        metrics.ABCScoreMin = minScore == double.MaxValue ? 0 : minScore;
        metrics.ABCScoreMax = maxScore == double.MinValue ? 0 : maxScore;

        // Calculate composite scores
        foreach (var scheme in _compositeWeightSchemes)
        {
            metrics.CompositeScores[scheme.Key] = CalculateCompositeScore(metrics, scheme.Value);
        }

        return metrics;
    }

    private void DisplayDiagnosticInfo()
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("DIAGNOSTIC INFORMATION");
        Console.WriteLine(new string('=', 100));

        var configNames = _scoreDistributions.Keys.Select(k => k.Config).Distinct().OrderBy(x => x).ToList();

        Console.WriteLine("\nAverage ABC Scores by Configuration:");
        Console.WriteLine(new string('-', 100));
        Console.WriteLine($"{"Configuration",-25} {"Mean Score",-15} {"Std Dev",-12} {"Min",-10} {"Max",-10} {"Range",-10}");
        Console.WriteLine(new string('-', 100));

        foreach (var cfg in configNames)
        {
            var scores = _scoreDistributions
                .Where(k => k.Key.Config == cfg)
                .SelectMany(k => k.Value)
                .ToList();

            if (scores.Count == 0) continue;

            var mean = scores.Average();
            var std = scores.SafeStandardDeviation();
            var min = scores.Min();
            var max = scores.Max();
            var range = max - min;

            Console.WriteLine($"{cfg,-25} {mean,-15:F2} {std,-12:F2} {min,-10:F2} {max,-10:F2} {range,-10:F2}");
        }

        Console.WriteLine("\nTest Count Distribution:");
        Console.WriteLine(new string('-', 100));
        Console.WriteLine($"{"Configuration",-25} {"Mean Count",-15} {"Std Dev",-12} {"Min",-10} {"Max",-10} {"CV %",-10}");
        Console.WriteLine(new string('-', 100));

        foreach (var cfg in configNames)
        {
            var counts = _testCountDistributions
                .Where(k => k.Key.Config == cfg)
                .SelectMany(k => k.Value)
                .ToList();

            if (counts.Count == 0) continue;

            var mean = counts.Average();
            var std = counts.Select(c => (double)c).SafeStandardDeviation();
            var min = counts.Min();
            var max = counts.Max();
            var cv = mean > 0 ? (std / mean) * 100 : 0;

            Console.WriteLine($"{cfg,-25} {mean,-15:F1} {std,-12:F1} {min,-10} {max,-10} {cv,-10:F1}");
        }

        // Check for proper differentiation
        var allScores = _scoreDistributions.SelectMany(k => k.Value).ToList();
        if (allScores.Count > 0)
        {
            var overallRange = allScores.Max() - allScores.Min();
            Console.WriteLine($"\nOverall ABC Score Range: {overallRange:F2}");

            if (overallRange < 10)
            {
                Console.WriteLine("⚠ WARNING: Low ABC score range may indicate insufficient weight differentiation!");
            }
            else
            {
                Console.WriteLine("✓ Good ABC score differentiation detected");
            }
        }
    }

    private void DisplayABCScoreAnalysis()
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("ABC SCORE ANALYSIS (Selection Pressure)");
        Console.WriteLine(new string('=', 100));

        Console.WriteLine("\nUnderstanding ABC Scores:");
        Console.WriteLine("  • Lower scores = Higher selection pressure (pickier algorithm)");
        Console.WriteLine("  • Higher scores = Lower selection pressure (more permissive)");
        Console.WriteLine("  • Optimal range: 40-60 (balances exploration vs exploitation)");
        Console.WriteLine("  • The 'Penalty Paradox': Harsh penalties often produce better test suites");

        Console.WriteLine("\nABC Score Classification:");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"{"Configuration",-25} {"Mean ABC Score",-15} {"Classification",-20} {"Expected Quality",-20}");
        Console.WriteLine(new string('-', 80));

        foreach (var configName in _abcScoresByConfig.Keys.OrderBy(k => k))
        {
            var scores = _abcScoresByConfig[configName].ToList();
            var mean = scores.Average();

            var classification = mean switch
            {
                < 20 => "Very High Pressure",
                < 40 => "High Pressure",
                < 60 => "Optimal Pressure",
                < 80 => "Low Pressure",
                < 100 => "Very Low Pressure",
                _ => "Minimal Pressure"
            };

            var quality = mean switch
            {
                < 20 => "May over-constrain",
                < 40 => "High quality",
                < 60 => "Optimal balance",
                < 80 => "Good coverage",
                < 100 => "May lack focus",
                _ => "Risk of poor quality"
            };

            Console.WriteLine($"{configName,-25} {mean,-15:F2} {classification,-20} {quality,-20}");
        }
    }

    private void ValidateFinalResults(Dictionary<string, ConfigurationStatistics> statistics)
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("FINAL VALIDATION");
        Console.WriteLine(new string('=', 100));

        var coverages = statistics.Values.Select(s => s.MeanCoverage).ToList();
        var diversities = statistics.Values.Select(s => s.MeanDiversity).ToList();
        var boundaries = statistics.Values.Select(s => s.MeanBoundaryRatio).ToList();
        var abcScores = statistics.Values.Select(s => s.MeanABCScore).ToList();

        var coverageRange = coverages.Max() - coverages.Min();
        var diversityRange = diversities.Max() - diversities.Min();
        var boundaryRange = boundaries.Max() - boundaries.Min();
        var abcScoreRange = abcScores.Max() - abcScores.Min();

        Console.WriteLine("\nMetric Differentiation Analysis:");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"{"Metric",-20} {"Range",-15} {"Min",-15} {"Max",-15} {"Status",-15}");
        Console.WriteLine(new string('-', 80));

        // Coverage validation
        var coverageStatus = coverageRange > 0.05 ? "✓ Good" : "⚠ Low variation";
        Console.WriteLine($"{"Coverage",-20} {coverageRange * 100,-15:F2}% {coverages.Min() * 100,-15:F2}% {coverages.Max() * 100,-15:F2}% {coverageStatus,-15}");

        // Diversity validation
        var diversityStatus = diversityRange > 1.0 ? "✓ Good" : "⚠ Low variation";
        Console.WriteLine($"{"Diversity",-20} {diversityRange,-15:F2} {diversities.Min(),-15:F2} {diversities.Max(),-15:F2} {diversityStatus,-15}");

        // Boundary validation
        var boundaryStatus = boundaryRange > 0.05 ? "✓ Good" : "⚠ Low variation";
        Console.WriteLine($"{"Boundary Ratio",-20} {boundaryRange * 100,-15:F2}% {boundaries.Min() * 100,-15:F2}% {boundaries.Max() * 100,-15:F2}% {boundaryStatus,-15}");

        // ABC Score validation
        var abcStatus = abcScoreRange > 10 ? "✓ Good" : "⚠ Low variation";
        Console.WriteLine($"{"ABC Score",-20} {abcScoreRange,-15:F2} {abcScores.Min(),-15:F2} {abcScores.Max(),-15:F2} {abcStatus,-15}");

        Console.WriteLine("\nValidation Summary:");
        var warnings = 0;

        if (coverageRange <= 0.05)
        {
            Console.WriteLine("  ⚠ Low coverage variation - weights may not be differentiating effectively");
            warnings++;
        }
        if (diversityRange <= 1.0)
        {
            Console.WriteLine("  ⚠ Low diversity variation - consider adjusting diversity bonus");
            warnings++;
        }
        if (boundaryRange <= 0.05)
        {
            Console.WriteLine("  ⚠ Low boundary variation - boundary weights may need adjustment");
            warnings++;
        }
        if (abcScoreRange <= 10)
        {
            Console.WriteLine("  ⚠ Low ABC score range - weight impact may be insufficient");
            warnings++;
        }

        if (warnings == 0)
        {
            Console.WriteLine("  ✓ All validation checks passed - weight configurations are properly differentiated");
        }
        else
        {
            Console.WriteLine($"  ⚠ {warnings} warning(s) detected - review weight configurations");
        }

        // Check for statistical power
        var totalRuns = statistics.Values.Sum(s => s.TotalRuns);
        Console.WriteLine($"\nStatistical Power Check:");
        Console.WriteLine($"  Total runs: {totalRuns}");
        Console.WriteLine($"  Runs per config: {statistics.Values.First().TotalRuns}");

        if (statistics.Values.First().TotalRuns >= 30)
        {
            Console.WriteLine("  ✓ Sufficient statistical power for robust conclusions");
        }
        else if (statistics.Values.First().TotalRuns >= 10)
        {
            Console.WriteLine("  ✓ Adequate statistical power for preliminary analysis");
        }
        else
        {
            Console.WriteLine("  ⚠ Low statistical power - consider increasing runs");
        }
    }

    private string GetABCConfigName(int index)
    {
        return index switch
        {
            0 => "Low_Conv_20gen",
            1 => "Med_Conv_50gen",
            2 => "High_Conv_100gen",
            _ => $"ABC_{index}"
        };
    }

    private ABCGenerationSettings CloneABCSettings(ABCGenerationSettings original)
    {
        return new ABCGenerationSettings
        {
            TotalPopulationGenerations = original.TotalPopulationGenerations,
            FinalPopulationSelectionRatio = original.FinalPopulationSelectionRatio,
            MutationRate = original.MutationRate,
            EliteSelectionRatio = original.EliteSelectionRatio,
            EnableOnlookerSelection = original.EnableOnlookerSelection,
            OnlookerSelectionRatio = original.OnlookerSelectionRatio,
            EnableScoutPhase = original.EnableScoutPhase,
            ScoutSelectionRatio = original.ScoutSelectionRatio,
            StagnationThresholdPercentage = original.StagnationThresholdPercentage,
            CoolingRate = original.CoolingRate,
            EnforceMutationUniqueness = original.EnforceMutationUniqueness,
            AllowMultipleInvalidInputs = original.AllowMultipleInvalidInputs,
        };
    }

    private TestRunMetrics CalculateMetrics(HashSet<TestCase> testCases, List<IInputParameter> parameters)
    {
        var metrics = new TestRunMetrics
        {
            TestCount = testCases.Count,
        };

        // Coverage: proportion of unique values covered
        var allCoveredValues = new HashSet<(int Index, object? Value)>();
        var totalPossibleValues = 0;

        for (int i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];
            var possibleValues = param.TestValues.Select(v => v.Value).Distinct().Count();
            totalPossibleValues += possibleValues;

            foreach (var testCase in testCases)
            {
                if (i < testCase.Values.Count)
                {
                    allCoveredValues.Add((i, testCase.Values[i].Value));
                }
            }
        }

        metrics.Coverage = totalPossibleValues > 0 ? (double)allCoveredValues.Count / totalPossibleValues : 0;

        // Diversity: average unique values per parameter
        var uniqueValuesPerParam = new Dictionary<int, HashSet<object?>>();
        for (int i = 0; i < parameters.Count; i++)
        {
            uniqueValuesPerParam[i] = new HashSet<object?>();
        }

        foreach (var testCase in testCases)
        {
            for (int i = 0; i < testCase.Values.Count && i < parameters.Count; i++)
            {
                uniqueValuesPerParam[i].Add(testCase.Values[i].Value);
            }
        }

        metrics.DiversityScore = uniqueValuesPerParam.Count > 0
            ? uniqueValuesPerParam.Average(kvp => kvp.Value.Count)
            : 0;

        // Category counts and ratios
        int totalValues = 0;
        foreach (var tc in testCases)
        {
            foreach (var value in tc.Values)
            {
                totalValues++;
                switch (value.Category)
                {
                    case TestValueCategory.BoundaryValid:
                        metrics.BoundaryValidCount++;
                        break;
                    case TestValueCategory.Valid:
                        metrics.ValidCount++;
                        break;
                    case TestValueCategory.BoundaryInvalid:
                        metrics.BoundaryInvalidCount++;
                        break;
                    case TestValueCategory.Invalid:
                        metrics.InvalidCount++;
                        break;
                }
            }
        }

        if (totalValues > 0)
        {
            metrics.BoundaryRatio = (double)(metrics.BoundaryValidCount + metrics.BoundaryInvalidCount) / totalValues;
            metrics.ValidRatio = (double)(metrics.BoundaryValidCount + metrics.ValidCount) / totalValues;
            metrics.InvalidRatio = (double)(metrics.BoundaryInvalidCount + metrics.InvalidCount) / totalValues;
        }

        return metrics;
    }

    private double CalculateCompositeScore(TestRunMetrics metrics, CompositeWeightScheme scheme)
    {
        // Normalize metrics to [0, 1] range
        var normalizedCoverage = metrics.Coverage; // Already in [0, 1]

        // Dynamic normalization based on scenario
        var maxDiversity = Math.Max(10.0, metrics.DiversityScore * 1.5); // Adaptive normalization
        var normalizedDiversity = Math.Min(metrics.DiversityScore / maxDiversity, 1.0);

        var normalizedBoundary = metrics.BoundaryRatio; // Already in [0, 1]
        var normalizedValid = metrics.ValidRatio; // Already in [0, 1]
        var normalizedInvalidPenalty = 1.0 - metrics.InvalidRatio; // Reward fewer invalid cases

        // Adaptive test count normalization
        var targetTestCount = 30.0; // Ideal test suite size
        var normalizedTestCount = 1.0 - Math.Min(Math.Abs(metrics.TestCount - targetTestCount) / targetTestCount, 1.0);

        return scheme.CoverageWeight * normalizedCoverage +
               scheme.DiversityWeight * normalizedDiversity +
               scheme.BoundaryWeight * normalizedBoundary +
               scheme.ValidWeight * normalizedValid +
               scheme.InvalidPenalty * normalizedInvalidPenalty +
               scheme.TestCountWeight * normalizedTestCount;
    }

    private Dictionary<string, ConfigurationStatistics> CalculateComprehensiveStatistics(ConcurrentBag<TestRunMetrics> results)
    {
        var statistics = new Dictionary<string, ConfigurationStatistics>();
        var groupedByConfig = results.GroupBy(r => r.ConfigName);

        foreach (var group in groupedByConfig)
        {
            var stats = new ConfigurationStatistics
            {
                ConfigName = group.Key,
                TotalRuns = group.Count(),
            };

            // Coverage statistics
            var coverages = group.Select(r => r.Coverage).ToList();
            stats.MeanCoverage = coverages.Average();
            stats.StdDevCoverage = coverages.SafeStandardDeviation();

            // Diversity statistics
            var diversities = group.Select(r => r.DiversityScore).ToList();
            stats.MeanDiversity = diversities.Average();
            stats.StdDevDiversity = diversities.SafeStandardDeviation();

            // Boundary ratio statistics
            var boundaries = group.Select(r => r.BoundaryRatio).ToList();
            stats.MeanBoundaryRatio = boundaries.Average();
            stats.StdDevBoundaryRatio = boundaries.SafeStandardDeviation();

            // Generation time statistics
            var times = group.Select(r => r.GenerationTimeMs).ToList();
            stats.MeanGenerationTime = times.Average();
            stats.StdDevGenerationTime = times.SafeStandardDeviation();

            // ABC score statistics
            var abcScores = group.Select(r => r.ABCScoreMean).ToList();
            stats.MeanABCScore = abcScores.Average();
            stats.StdDevABCScore = abcScores.SafeStandardDeviation();

            // Composite score statistics per scheme
            stats.WeightSchemeStats = new Dictionary<string, WeightSchemeStats>();
            foreach (var scheme in _compositeWeightSchemes.Keys)
            {
                var compositeScores = group.Select(r => r.CompositeScores[scheme]).ToList();
                stats.WeightSchemeStats[scheme] = new WeightSchemeStats
                {
                    MeanComposite = compositeScores.Average(),
                    StdDevComposite = compositeScores.SafeStandardDeviation(),
                    MinComposite = compositeScores.Min(),
                    MaxComposite = compositeScores.Max(),
                };
            }

            statistics[group.Key] = stats;
        }

        // Calculate rankings
        foreach (var scheme in _compositeWeightSchemes.Keys)
        {
            var ranked = statistics.Values
                .OrderByDescending(s => s.WeightSchemeStats[scheme].MeanComposite)
                .Select((s, i) => new { s.ConfigName, Rank = i + 1 })
                .ToList();

            foreach (var r in ranked)
            {
                statistics[r.ConfigName].RankByScheme[scheme] = r.Rank;
            }
        }

        // Calculate average rank and stability
        foreach (var stat in statistics.Values)
        {
            var ranks = stat.RankByScheme.Values.Select(r => (double)r).ToList();
            stat.AverageRank = ranks.Average();
            stat.RankStdDev = ranks.SafeStandardDeviation();
        }

        return statistics;
    }

    private void PerformSensitivityAnalysis(Dictionary<string, ConfigurationStatistics> statistics)
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("SENSITIVITY ANALYSIS");
        Console.WriteLine(new string('=', 100));

        // Spearman rank correlation between schemes
        Console.WriteLine("\nSpearman Rank Correlation Between Evaluation Schemes:");
        Console.WriteLine("(Values > 0.7 indicate consistent ranking across schemes)");
        Console.WriteLine(new string('-', 100));

        var schemes = _compositeWeightSchemes.Keys.ToList();
        Console.Write($"{"Scheme",-20}");
        foreach (var s in schemes)
        {
            Console.Write($"{s.Substring(0, Math.Min(10, s.Length)),-12}");
        }
        Console.WriteLine();

        foreach (var s1 in schemes)
        {
            Console.Write($"{s1,-20}");
            foreach (var s2 in schemes)
            {
                if (s1 == s2)
                {
                    Console.Write($"{"1.000",-12}");
                    continue;
                }

                var configs = statistics.Keys.OrderBy(k => k).ToList();
                var ranks1 = configs.Select(c => (double)statistics[c].RankByScheme[s1]).ToArray();
                var ranks2 = configs.Select(c => (double)statistics[c].RankByScheme[s2]).ToArray();
                var corr = Correlation.Spearman(ranks1, ranks2);

                var display = Math.Abs(corr) > 0.7 ? $"{corr:F3}✓" : $"{corr:F3}";
                Console.Write($"{display,-12}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("\nRanking Stability Analysis:");
        Console.WriteLine(new string('-', 100));
        Console.WriteLine($"{"Configuration",-25} {"Avg Rank",-12} {"Rank StdDev",-12} {"Min Rank",-10} {"Max Rank",-10} {"Stability",-15}");
        Console.WriteLine(new string('-', 100));

        foreach (var stat in statistics.Values.OrderBy(s => s.RankStdDev))
        {
            var minRank = stat.RankByScheme.Values.Min();
            var maxRank = stat.RankByScheme.Values.Max();
            var stability = stat.RankStdDev switch
            {
                < 0.5 => "Very Stable ✓✓",
                < 1.0 => "Stable ✓",
                < 2.0 => "Moderate",
                _ => "Unstable ⚠"
            };

            Console.WriteLine($"{stat.ConfigName,-25} {stat.AverageRank,-12:F2} {stat.RankStdDev,-12:F2} " +
                            $"{minRank,-10} {maxRank,-10} {stability,-15}");
        }
    }

    private void DisplayDetailedResults(Dictionary<string, ConfigurationStatistics> statistics)
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("DETAILED RESULTS BY COMPOSITE WEIGHT SCHEME");
        Console.WriteLine(new string('=', 100));

        foreach (var scheme in _compositeWeightSchemes)
        {
            Console.WriteLine($"\n{scheme.Key} Scheme ({scheme.Value.Description}):");
            Console.WriteLine(new string('-', 100));
            Console.WriteLine($"{"Configuration",-25} {"Mean Score",-15} {"Std Dev",-12} {"Min",-10} {"Max",-10} {"Rank",-8}");
            Console.WriteLine(new string('-', 100));

            var ordered = statistics.OrderByDescending(s => s.Value.WeightSchemeStats[scheme.Key].MeanComposite);
            foreach (var stat in ordered)
            {
                var w = stat.Value.WeightSchemeStats[scheme.Key];
                Console.WriteLine($"{stat.Key,-25} {w.MeanComposite,-15:F4} {w.StdDevComposite,-12:F4} " +
                                $"{w.MinComposite,-10:F4} {w.MaxComposite,-10:F4} " +
                                $"{stat.Value.RankByScheme[scheme.Key],-8}");
            }
        }

        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("INDIVIDUAL METRICS SUMMARY");
        Console.WriteLine(new string('=', 100));
        Console.WriteLine($"{"Configuration",-25} {"Coverage %",-15} {"Diversity",-15} {"Boundary %",-15} {"ABC Score",-15} {"Time (ms)",-15}");
        Console.WriteLine(new string('-', 100));

        foreach (var stat in statistics.Values.OrderBy(s => s.AverageRank))
        {
            Console.WriteLine($"{stat.ConfigName,-25} {stat.MeanCoverage * 100,-15:F2} {stat.MeanDiversity,-15:F2} " +
                            $"{stat.MeanBoundaryRatio * 100,-15:F2} {stat.MeanABCScore,-15:F2} " +
                            $"{stat.MeanGenerationTime,-15:F1}");
        }
    }

    private void DisplayRankingAnalysis(Dictionary<string, ConfigurationStatistics> statistics)
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("COMPREHENSIVE RANKING ANALYSIS");
        Console.WriteLine(new string('=', 100));

        Console.WriteLine("\nOverall Ranking (by average rank across all schemes):");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"{"Rank",-6} {"Configuration",-25} {"Avg Rank",-12} {"Best Rank",-12} {"Worst Rank",-12}");
        Console.WriteLine(new string('-', 80));

        var overallRanked = statistics.Values.OrderBy(s => s.AverageRank).ToList();
        for (int i = 0; i < overallRanked.Count; i++)
        {
            var stat = overallRanked[i];
            var bestRank = stat.RankByScheme.Values.Min();
            var worstRank = stat.RankByScheme.Values.Max();
            Console.WriteLine($"{i + 1,-6} {stat.ConfigName,-25} {stat.AverageRank,-12:F2} {bestRank,-12} {worstRank,-12}");
        }
    }

    private void PerformStatisticalTests(Dictionary<string, ConfigurationStatistics> statistics)
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("STATISTICAL SIGNIFICANCE TESTS");
        Console.WriteLine(new string('=', 100));

        var baseline = statistics.Values.FirstOrDefault(s =>
            s.ConfigName.Contains("Default", StringComparison.OrdinalIgnoreCase) ||
            s.ConfigName.Contains("Baseline", StringComparison.OrdinalIgnoreCase) ||
            s.ConfigName == "Current (Baseline)")
            ?? statistics.Values.First();

        Console.WriteLine($"\nComparing all configurations to baseline: {baseline.ConfigName}");
        Console.WriteLine($"Using Welch's t-test with significance level α = {SignificanceLevel}");
        Console.WriteLine($"Note: p-values are not adjusted for multiple testing");

        foreach (var scheme in _compositeWeightSchemes.Keys)
        {
            Console.WriteLine($"\n{scheme} Scheme:");
            Console.WriteLine(new string('-', 100));
            Console.WriteLine($"{"Configuration",-25} {"Diff %",-12} {"p-value",-12} {"Cohen's d",-12} {"Effect Size",-15} {"Sig",-8}");
            Console.WriteLine(new string('-', 100));

            foreach (var stat in statistics.Values.Where(s => s.ConfigName != baseline.ConfigName))
            {
                var baselineScheme = baseline.WeightSchemeStats[scheme];
                var currentScheme = stat.WeightSchemeStats[scheme];

                var improvement = baselineScheme.MeanComposite != 0
                    ? ((currentScheme.MeanComposite - baselineScheme.MeanComposite) / Math.Abs(baselineScheme.MeanComposite)) * 100
                    : 0;

                // Proper Welch's t-test with Welch-Satterthwaite degrees of freedom
                var (t, df) = CalculateWelchTTest(
                    currentScheme.MeanComposite, currentScheme.StdDevComposite, stat.TotalRuns,
                    baselineScheme.MeanComposite, baselineScheme.StdDevComposite, baseline.TotalRuns
                );

                var pValue = 2 * (1 - StudentT.CDF(0, 1, df, Math.Abs(t)));

                // Cohen's d for effect size
                var pooledSd = CalculatePooledStandardDeviation(
                    baselineScheme.StdDevComposite, baseline.TotalRuns,
                    currentScheme.StdDevComposite, stat.TotalRuns
                );

                var cohensD = pooledSd > 0 ? (currentScheme.MeanComposite - baselineScheme.MeanComposite) / pooledSd : 0;

                var effectSize = Math.Abs(cohensD) switch
                {
                    < 0.2 => "Negligible",
                    < 0.5 => "Small",
                    < 0.8 => "Medium",
                    _ => "Large"
                };

                var significance = pValue switch
                {
                    < 0.001 => "***",
                    < 0.01 => "**",
                    < 0.05 => "*",
                    _ => "ns"
                };

                Console.WriteLine($"{stat.ConfigName,-25} {improvement,-12:F2} {pValue,-12:F4} " +
                                $"{cohensD,-12:F3} {effectSize,-15} {significance,-8}");
            }
        }
    }

    private void GenerateRecommendations(Dictionary<string, ConfigurationStatistics> statistics)
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("FINAL RECOMMENDATIONS");
        Console.WriteLine(new string('=', 100));

        var bestByAvgRank = statistics.Values.OrderBy(s => s.AverageRank).First();
        var currentBaseline = statistics.Values.FirstOrDefault(s =>
            s.ConfigName.Contains("Default") ||
            s.ConfigName.Contains("Baseline") ||
            s.ConfigName == "Current (Baseline)")
            ?? statistics.Values.First();

        Console.WriteLine($"\n1. MOST ROBUST CONFIGURATION (lowest average rank across all schemes):");
        Console.WriteLine($"   → {bestByAvgRank.ConfigName}");
        Console.WriteLine($"     • Average Rank: {bestByAvgRank.AverageRank:F2} (σ = {bestByAvgRank.RankStdDev:F2})");
        Console.WriteLine($"     • Mean ABC Score: {bestByAvgRank.MeanABCScore:F2} ± {bestByAvgRank.StdDevABCScore:F2}");
        Console.WriteLine($"     • Coverage: {bestByAvgRank.MeanCoverage * 100:F2}%");
        Console.WriteLine($"     • Boundary Ratio: {bestByAvgRank.MeanBoundaryRatio * 100:F2}%");

        Console.WriteLine($"\n2. BEST CONFIGURATION BY USE CASE:");
        foreach (var scheme in _compositeWeightSchemes)
        {
            var best = statistics.Values.OrderBy(s => s.RankByScheme[scheme.Key]).First();
            var score = best.WeightSchemeStats[scheme.Key].MeanComposite;
            Console.WriteLine($"   • {scheme.Key}: {best.ConfigName} (Score: {score:F4})");
            Console.WriteLine($"     {scheme.Value.Description}");
        }

        Console.WriteLine($"\n3. CURRENT BASELINE PERFORMANCE:");
        Console.WriteLine($"   → {currentBaseline.ConfigName}");
        Console.WriteLine($"     • Average Rank: {currentBaseline.AverageRank:F2} out of {statistics.Count}");
        Console.WriteLine($"     • Mean ABC Score: {currentBaseline.MeanABCScore:F2} (Selection Pressure)");

        if (currentBaseline.ConfigName == bestByAvgRank.ConfigName)
        {
            Console.WriteLine($"     ✓ Current baseline IS the most robust configuration!");
        }
        else
        {
            var rankDiff = currentBaseline.AverageRank - bestByAvgRank.AverageRank;
            if (rankDiff < 1.0)
            {
                Console.WriteLine($"     ✓ Current baseline is near-optimal (within 1 rank of best)");
            }
            else
            {
                Console.WriteLine($"     ⚠ Consider switching to {bestByAvgRank.ConfigName} for {rankDiff:F1} rank improvement");
            }
        }

        Console.WriteLine($"\n4. KEY OBSERVATIONS:");

        var coverageRange = statistics.Values.Max(s => s.MeanCoverage) - statistics.Values.Min(s => s.MeanCoverage);
        var abcScoreRange = statistics.Values.Max(s => s.MeanABCScore) - statistics.Values.Min(s => s.MeanABCScore);

        Console.WriteLine(coverageRange < 0.05
            ? $"   • Low coverage variation ({coverageRange * 100:F2}%) - limited differentiation"
            : $"   • Good coverage variation ({coverageRange * 100:F2}%) - proper differentiation");

        Console.WriteLine(abcScoreRange < 10
            ? $"   • Low ABC score range ({abcScoreRange:F2}) - possible convergence issues"
            : $"   • Substantial ABC score range ({abcScoreRange:F2}) - confirms weight impact");

        // Penalty Paradox observation
        var highPenalty = statistics.Values.FirstOrDefault(s => s.ConfigName.Contains("High") && s.ConfigName.Contains("Penalty"));
        if (highPenalty != null && highPenalty.AverageRank < statistics.Count / 2.0)
        {
            Console.WriteLine($"   • ✓ 'Penalty Paradox' confirmed: Harsh penalties improve quality (Rank: {highPenalty.AverageRank:F1})");
        }

        Console.WriteLine($"\n5. STATISTICAL CONFIDENCE:");
        Console.WriteLine($"   • Total experiments: {statistics.Values.Sum(s => s.TotalRuns)}");
        Console.WriteLine($"   • Experiments per config: {statistics.Values.First().TotalRuns}");
        Console.WriteLine($"   • Statistical power: {(statistics.Values.First().TotalRuns >= 30 ? "High" : "Moderate")}");
    }

    // Statistical helper methods
    private static (double t, double df) CalculateWelchTTest(
        double mean1, double sd1, int n1,
        double mean2, double sd2, int n2)
    {
        if (n1 <= 1 || n2 <= 1) return (0, 1);

        var var1 = sd1 * sd1;
        var var2 = sd2 * sd2;

        // Welch's t-statistic
        var t = (mean1 - mean2) / Math.Sqrt(var1 / n1 + var2 / n2);

        // Welch-Satterthwaite degrees of freedom
        var numerator = Math.Pow(var1 / n1 + var2 / n2, 2);
        var denominator = (var1 * var1) / (n1 * n1 * (n1 - 1)) +
                         (var2 * var2) / (n2 * n2 * (n2 - 1));

        var df = denominator > 0 ? numerator / denominator : n1 + n2 - 2;

        return (t, df);
    }

    private static double CalculatePooledStandardDeviation(double sd1, int n1, double sd2, int n2)
    {
        if (n1 <= 1 || n2 <= 1) return 0;

        var pooledVariance = ((n1 - 1) * sd1 * sd1 + (n2 - 1) * sd2 * sd2) / (n1 + n2 - 2);
        return pooledVariance > 0 ? Math.Sqrt(pooledVariance) : 0;
    }

    private static int StableSeed(string value)
    {
        // FNV-1a 32-bit hash for stable, deterministic seed generation
        const uint fnvOffset = 2166136261;
        const uint fnvPrime = 16777619;

        uint hash = fnvOffset;
        foreach (var b in System.Text.Encoding.UTF8.GetBytes(value))
        {
            hash ^= b;
            hash *= fnvPrime;
        }

        // Ensure non-negative result
        return unchecked((int)(hash & 0x7FFFFFFF));
    }

    // Test scenario initialization methods
    private void InitializeTestScenarios()
    {
        _testScenarios = new Dictionary<string, List<IInputParameter>>
        {
            ["Small"] = CreateSmallScenario(),
            ["Medium"] = CreateMediumScenario(),
            ["Large"] = CreateLargeScenario(),
        };
    }

    private void InitializeABCConfigurations()
    {
        _abcConfigurations = new List<ABCGenerationSettings>
        {
            // Low convergence - Quick exploration
            new ABCGenerationSettings
            {
                TotalPopulationGenerations = 20,
                FinalPopulationSelectionRatio = 0.3,
                MutationRate = 0.3,
                EliteSelectionRatio = 0.1,
                EnableOnlookerSelection = true,
                OnlookerSelectionRatio = 0.5,
                EnableScoutPhase = true,
                ScoutSelectionRatio = 0.1,
                StagnationThresholdPercentage = 30,
                CoolingRate = 0.9,
                EnforceMutationUniqueness = true,
                AllowMultipleInvalidInputs = false,
            },
            // Medium convergence - Balanced
            new ABCGenerationSettings
            {
                TotalPopulationGenerations = 50,
                FinalPopulationSelectionRatio = 0.5,
                MutationRate = 0.2,
                EliteSelectionRatio = 0.2,
                EnableOnlookerSelection = true,
                OnlookerSelectionRatio = 0.5,
                EnableScoutPhase = true,
                ScoutSelectionRatio = 0.1,
                StagnationThresholdPercentage = 20,
                CoolingRate = 0.95,
                EnforceMutationUniqueness = true,
                AllowMultipleInvalidInputs = false,
            },
            // High convergence - Thorough optimization
            new ABCGenerationSettings
            {
                TotalPopulationGenerations = 100,
                FinalPopulationSelectionRatio = 0.7,
                MutationRate = 0.1,
                EliteSelectionRatio = 0.3,
                EnableOnlookerSelection = true,
                OnlookerSelectionRatio = 0.5,
                EnableScoutPhase = true,
                ScoutSelectionRatio = 0.1,
                StagnationThresholdPercentage = 10,
                CoolingRate = 0.98,
                EnforceMutationUniqueness = true,
                AllowMultipleInvalidInputs = false,
            },
        };
    }

    private List<IInputParameter> CreateSmallScenario()
    {
        var parameters = new List<IInputParameter>();

        // Text parameter with boundaries
        var textParam = new TextDataParameter(false, 3, 20, true, true, true);
        textParam.TestValues.Add(new TestValue("ab", TestValueCategory.BoundaryInvalid));
        textParam.TestValues.Add(new TestValue("abc", TestValueCategory.BoundaryValid));
        textParam.TestValues.Add(new TestValue("hello", TestValueCategory.Valid));
        textParam.TestValues.Add(new TestValue("test123", TestValueCategory.Valid));
        textParam.TestValues.Add(new TestValue("twentycharacterslong", TestValueCategory.BoundaryValid));
        textParam.TestValues.Add(new TestValue("twentyonecharacterss!", TestValueCategory.BoundaryInvalid));
        parameters.Add(textParam);

        // Integer parameter with boundaries
        var numberParam = new IntegerDataParameter(false, 1, 100);
        numberParam.TestValues.Add(new TestValue(0, TestValueCategory.BoundaryInvalid));
        numberParam.TestValues.Add(new TestValue(1, TestValueCategory.BoundaryValid));
        numberParam.TestValues.Add(new TestValue(50, TestValueCategory.Valid));
        numberParam.TestValues.Add(new TestValue(100, TestValueCategory.BoundaryValid));
        numberParam.TestValues.Add(new TestValue(101, TestValueCategory.BoundaryInvalid));
        parameters.Add(numberParam);

        // Boolean parameter
        var boolParam = new BooleanDataParameter();
        boolParam.TestValues.Add(new TestValue(true, TestValueCategory.Valid));
        boolParam.TestValues.Add(new TestValue(false, TestValueCategory.Valid));
        parameters.Add(boolParam);

        return parameters;
    }

    private List<IInputParameter> CreateMediumScenario()
    {
        var parameters = new List<IInputParameter>();
        parameters.AddRange(CreateSmallScenario());

        // Email parameter
        var emailParam = new EmailDataParameter(false);
        emailParam.TestValues.Add(new TestValue("test@example.com", TestValueCategory.Valid));
        emailParam.TestValues.Add(new TestValue("user.name@domain.co.uk", TestValueCategory.Valid));
        emailParam.TestValues.Add(new TestValue("invalid.email", TestValueCategory.Invalid));
        emailParam.TestValues.Add(new TestValue("@invalid.com", TestValueCategory.Invalid));
        emailParam.TestValues.Add(new TestValue("a@b.c", TestValueCategory.BoundaryValid));
        parameters.Add(emailParam);

        // Date parameter
        var dateParam = new DateDataParameter(false, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
        dateParam.TestValues.Add(new TestValue(new DateTime(2023, 12, 31), TestValueCategory.BoundaryInvalid));
        dateParam.TestValues.Add(new TestValue(new DateTime(2024, 1, 1), TestValueCategory.BoundaryValid));
        dateParam.TestValues.Add(new TestValue(new DateTime(2024, 6, 15), TestValueCategory.Valid));
        dateParam.TestValues.Add(new TestValue(new DateTime(2024, 12, 31), TestValueCategory.BoundaryValid));
        dateParam.TestValues.Add(new TestValue(new DateTime(2025, 1, 1), TestValueCategory.BoundaryInvalid));
        parameters.Add(dateParam);

        return parameters;
    }

    private List<IInputParameter> CreateLargeScenario()
    {
        var parameters = new List<IInputParameter>();
        parameters.AddRange(CreateMediumScenario());

        // Phone parameter
        var phoneParam = new PhoneDataParameter(false);
        phoneParam.TestValues.Add(new TestValue("+1234567890", TestValueCategory.Valid));
        phoneParam.TestValues.Add(new TestValue("555-1234", TestValueCategory.Invalid));
        phoneParam.TestValues.Add(new TestValue("+44 20 7946 0958", TestValueCategory.Valid));
        phoneParam.TestValues.Add(new TestValue("", TestValueCategory.Invalid));
        parameters.Add(phoneParam);

        // Single select parameter
        var dropdownParam = new SingleSelectDataParameter(false);
        dropdownParam.TestValues.Add(new TestValue("Option1", TestValueCategory.Valid));
        dropdownParam.TestValues.Add(new TestValue("Option2", TestValueCategory.Valid));
        dropdownParam.TestValues.Add(new TestValue("Option3", TestValueCategory.Valid));
        dropdownParam.TestValues.Add(new TestValue("InvalidOption", TestValueCategory.Invalid));
        parameters.Add(dropdownParam);

        // Additional text parameter
        var textParam2 = new TextDataParameter(false, 5, 50, true, true, true);
        textParam2.TestValues.Add(new TestValue("test", TestValueCategory.BoundaryInvalid));
        textParam2.TestValues.Add(new TestValue("valid", TestValueCategory.BoundaryValid));
        textParam2.TestValues.Add(new TestValue("sample text", TestValueCategory.Valid));
        textParam2.TestValues.Add(new TestValue("another valid text", TestValueCategory.Valid));
        textParam2.TestValues.Add(new TestValue(new string('x', 50), TestValueCategory.BoundaryValid));
        textParam2.TestValues.Add(new TestValue(new string('x', 51), TestValueCategory.BoundaryInvalid));
        parameters.Add(textParam2);

        // Additional integer parameter
        var intParam2 = new IntegerDataParameter(false, 0, 1000);
        intParam2.TestValues.Add(new TestValue(-1, TestValueCategory.BoundaryInvalid));
        intParam2.TestValues.Add(new TestValue(0, TestValueCategory.BoundaryValid));
        intParam2.TestValues.Add(new TestValue(1, TestValueCategory.Valid));
        intParam2.TestValues.Add(new TestValue(500, TestValueCategory.Valid));
        intParam2.TestValues.Add(new TestValue(1000, TestValueCategory.BoundaryValid));
        intParam2.TestValues.Add(new TestValue(1001, TestValueCategory.BoundaryInvalid));
        parameters.Add(intParam2);

        return parameters;
    }
}
