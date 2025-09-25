using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MathNet.Numerics.Statistics;
using NUnit.Framework;
using Testimize;
using Testimize.Contracts;
using Testimize.Parameters;
using Testimize.Parameters.Core;
using Testimize.TestCaseGenerators;

namespace Testimize.Tests.ParameterSensitivityAnalysis;

/// <summary>
/// Comprehensive parameter sensitivity analysis using detailed form validation scenario
/// Tests all 8 numeric ABC parameters with full statistical validation
/// </summary>
[TestFixture]
public class ComprehensiveParameterTests
{
    private List<IInputParameter> _detailedFormScenario;
    private const int TRIALS_PER_CONFIG = 5;
    private const int DEFAULT_GENERATIONS = 100; // Increased to see full parameter effects

    [SetUp]
    public void SetUp()
    {
        _detailedFormScenario = CreateDetailedFormValidationScenario();
    }

    private List<IInputParameter> CreateDetailedFormValidationScenario()
    {
        // More complex scenario to properly test parameter effects
        return new List<IInputParameter>
        {
            // Parameter 1: Username (Text)
            new TextDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("ValidUser1", TestValueCategory.Valid),
                new TestValue("ValidUser2", TestValueCategory.Valid),
                new TestValue("Min", TestValueCategory.BoundaryValid),
                new TestValue("MaximumLengthUsername", TestValueCategory.BoundaryValid),
                new TestValue("Mi", TestValueCategory.BoundaryInvalid),
                new TestValue("ExceedsMaximumLengthUsername", TestValueCategory.BoundaryInvalid),
                new TestValue("", TestValueCategory.Invalid),
                new TestValue("Invalid@User", TestValueCategory.Invalid)
            }),

            // Parameter 2: Email
            new EmailDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("user@example.com", TestValueCategory.Valid),
                new TestValue("test@domain.org", TestValueCategory.Valid),
                new TestValue("a@b.c", TestValueCategory.BoundaryValid),
                new TestValue("very.long.email@subdomain.example.com", TestValueCategory.BoundaryValid),
                new TestValue("@b.c", TestValueCategory.BoundaryInvalid),
                new TestValue("toolong.email@subdomain.example.company.com", TestValueCategory.BoundaryInvalid),
                new TestValue("notanemail", TestValueCategory.Invalid),
                new TestValue("", TestValueCategory.Invalid)
            }),

            // Parameter 3: Age (Integer)
            new IntegerDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue(25, TestValueCategory.Valid),
                new TestValue(50, TestValueCategory.Valid),
                new TestValue(18, TestValueCategory.BoundaryValid),
                new TestValue(120, TestValueCategory.BoundaryValid),
                new TestValue(17, TestValueCategory.BoundaryInvalid),
                new TestValue(121, TestValueCategory.BoundaryInvalid),
                new TestValue(0, TestValueCategory.Invalid),
                new TestValue(-5, TestValueCategory.Invalid)
            }),

            // Parameter 4: Phone
            new PhoneDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("+12025551234", TestValueCategory.Valid),
                new TestValue("+442071234567", TestValueCategory.Valid),
                new TestValue("+1234", TestValueCategory.BoundaryValid),
                new TestValue("+123456789012345", TestValueCategory.BoundaryValid),
                new TestValue("+123", TestValueCategory.BoundaryInvalid),
                new TestValue("+1234567890123456", TestValueCategory.BoundaryInvalid),
                new TestValue("notaphone", TestValueCategory.Invalid),
                new TestValue("", TestValueCategory.Invalid)
            }),

            // Parameter 5: Country Selection
            new TextDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue("USA", TestValueCategory.Valid),
                new TestValue("UK", TestValueCategory.Valid),
                new TestValue("Germany", TestValueCategory.Valid),
                new TestValue("France", TestValueCategory.Valid),
                new TestValue("A", TestValueCategory.BoundaryInvalid),
                new TestValue("", TestValueCategory.Invalid)
            }),

            // Parameter 6: Accept Terms (Boolean)
            new BooleanDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue(true, TestValueCategory.Valid),
                new TestValue(false, TestValueCategory.Invalid)
            }),

            // Parameter 7: Priority Level
            new IntegerDataParameter(preciseMode: true, preciseTestValues: new[]
            {
                new TestValue(1, TestValueCategory.BoundaryValid),
                new TestValue(2, TestValueCategory.Valid),
                new TestValue(3, TestValueCategory.Valid),
                new TestValue(4, TestValueCategory.Valid),
                new TestValue(5, TestValueCategory.BoundaryValid),
                new TestValue(0, TestValueCategory.BoundaryInvalid),
                new TestValue(6, TestValueCategory.BoundaryInvalid)
            })
        };
    }

    [Test]
    public void Test_01_FinalPopulationSelectionRatio()
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("FINALOPOPULATIONSELECTIONRATIO - COMPREHENSIVE ANALYSIS");
        Console.WriteLine("Scenario: Complex Form Validation (7 parameters, 53 test values)");
        Console.WriteLine(new string('=', 100) + "\n");

        var testValues = new[] { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };
        var results = new List<ParameterTestResult>();

        foreach (var ratio in testValues)
        {
            var trialResults = RunTrials(config =>
            {
                config.FinalPopulationSelectionRatio = ratio;
                config.TotalPopulationGenerations = DEFAULT_GENERATIONS;
            });

            results.Add(new ParameterTestResult
            {
                Value = ratio,
                MeanScore = trialResults.Select(r => r.TotalScore).Average(),
                StdDev = trialResults.Select(r => r.TotalScore).StandardDeviation(),
                MeanTestCount = trialResults.Select(r => r.TestCount).Average(),
                MeanCoverage = trialResults.Select(r => r.Coverage).Average(),
                MeanDiversity = trialResults.Select(r => r.Diversity).Average()
            });
        }

        PrintDetailedResults("FinalPopulationSelectionRatio", results);
        PerformStatisticalAnalysis("FinalPopulationSelectionRatio", results);
    }

    [Test]
    public void Test_02_EliteSelectionRatio()
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("ELITESELECTIONRATIO - COMPREHENSIVE ANALYSIS");
        Console.WriteLine("Scenario: Complex Form Validation (7 parameters, 53 test values)");
        Console.WriteLine(new string('=', 100) + "\n");

        var testValues = new[] { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8 };
        var results = new List<ParameterTestResult>();

        foreach (var ratio in testValues)
        {
            var trialResults = RunTrials(config =>
            {
                config.EliteSelectionRatio = ratio;
                config.TotalPopulationGenerations = DEFAULT_GENERATIONS;
            });

            results.Add(new ParameterTestResult
            {
                Value = ratio,
                MeanScore = trialResults.Select(r => r.TotalScore).Average(),
                StdDev = trialResults.Select(r => r.TotalScore).StandardDeviation(),
                MeanTestCount = trialResults.Select(r => r.TestCount).Average(),
                MeanCoverage = trialResults.Select(r => r.Coverage).Average(),
                MeanDiversity = trialResults.Select(r => r.Diversity).Average()
            });
        }

        PrintDetailedResults("EliteSelectionRatio", results);
        PerformStatisticalAnalysis("EliteSelectionRatio", results);
    }

    [Test]
    public void Test_03_TotalGenerations()
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("TOTALGENERATIONS - COMPREHENSIVE ANALYSIS");
        Console.WriteLine("Scenario: Complex Form Validation (7 parameters, 53 test values)");
        Console.WriteLine(new string('=', 100) + "\n");

        var testValues = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 120, 150 };
        var results = new List<ParameterTestResult>();

        foreach (var generations in testValues)
        {
            var sw = Stopwatch.StartNew();
            var trialResults = RunTrials(config =>
            {
                config.TotalPopulationGenerations = generations;
            });
            sw.Stop();

            results.Add(new ParameterTestResult
            {
                Value = generations,
                MeanScore = trialResults.Select(r => r.TotalScore).Average(),
                StdDev = trialResults.Select(r => r.TotalScore).StandardDeviation(),
                MeanTestCount = trialResults.Select(r => r.TestCount).Average(),
                MeanCoverage = trialResults.Select(r => r.Coverage).Average(),
                MeanDiversity = trialResults.Select(r => r.Diversity).Average(),
                ExecutionTime = sw.ElapsedMilliseconds / TRIALS_PER_CONFIG
            });
        }

        PrintDetailedResults("TotalGenerations", results, includeTime: true);
        PerformStatisticalAnalysis("TotalGenerations", results);
    }

    [Test]
    public void Test_04_MutationRate()
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("MUTATIONRATE - COMPREHENSIVE ANALYSIS");
        Console.WriteLine("Scenario: Complex Form Validation (7 parameters, 53 test values)");
        Console.WriteLine(new string('=', 100) + "\n");

        var testValues = new[] { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };
        var results = new List<ParameterTestResult>();

        foreach (var rate in testValues)
        {
            var trialResults = RunTrials(config =>
            {
                config.MutationRate = rate;
                config.TotalPopulationGenerations = DEFAULT_GENERATIONS;
            });

            results.Add(new ParameterTestResult
            {
                Value = rate,
                MeanScore = trialResults.Select(r => r.TotalScore).Average(),
                StdDev = trialResults.Select(r => r.TotalScore).StandardDeviation(),
                MeanTestCount = trialResults.Select(r => r.TestCount).Average(),
                MeanCoverage = trialResults.Select(r => r.Coverage).Average(),
                MeanDiversity = trialResults.Select(r => r.Diversity).Average()
            });
        }

        PrintDetailedResults("MutationRate", results);
        PerformStatisticalAnalysis("MutationRate", results);
    }

    [Test]
    public void Test_05_CoolingRate()
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("COOLINGRATE - COMPREHENSIVE ANALYSIS");
        Console.WriteLine("Scenario: Complex Form Validation (7 parameters, 53 test values)");
        Console.WriteLine("NOTE: Testing with EnforceMutationUniqueness=false to enable SA behavior");
        Console.WriteLine(new string('=', 100) + "\n");

        var testValues = new[] { 0.5, 0.6, 0.7, 0.8, 0.85, 0.9, 0.95, 0.99 };
        var results = new List<ParameterTestResult>();

        foreach (var rate in testValues)
        {
            var trialResults = RunTrials(config =>
            {
                config.CoolingRate = rate;
                config.EnforceMutationUniqueness = false; // Enable SA mode
                config.TotalPopulationGenerations = DEFAULT_GENERATIONS; // Use 100 generations
            });

            var temperature25 = Math.Max(0.1, Math.Pow(rate, 25));
            results.Add(new ParameterTestResult
            {
                Value = rate,
                MeanScore = trialResults.Select(r => r.TotalScore).Average(),
                StdDev = trialResults.Select(r => r.TotalScore).StandardDeviation(),
                MeanTestCount = trialResults.Select(r => r.TestCount).Average(),
                MeanCoverage = trialResults.Select(r => r.Coverage).Average(),
                MeanDiversity = trialResults.Select(r => r.Diversity).Average(),
                AdditionalMetric = temperature25 // Temperature at generation 25
            });
        }

        PrintDetailedResults("CoolingRate", results, additionalMetricName: "Temp@25");
        PerformStatisticalAnalysis("CoolingRate", results);
    }

    [Test]
    public void Test_06_OnlookerSelectionRatio()
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("ONLOOKERSELECTIONRATIO - COMPREHENSIVE ANALYSIS");
        Console.WriteLine("Scenario: Complex Form Validation (7 parameters, 53 test values)");
        Console.WriteLine(new string('=', 100) + "\n");

        var testValues = new[] { 0.0, 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4 };
        var results = new List<ParameterTestResult>();

        foreach (var ratio in testValues)
        {
            var trialResults = RunTrials(config =>
            {
                config.OnlookerSelectionRatio = ratio;
                config.EnableOnlookerSelection = ratio > 0;
                config.TotalPopulationGenerations = DEFAULT_GENERATIONS;
            });

            results.Add(new ParameterTestResult
            {
                Value = ratio,
                MeanScore = trialResults.Select(r => r.TotalScore).Average(),
                StdDev = trialResults.Select(r => r.TotalScore).StandardDeviation(),
                MeanTestCount = trialResults.Select(r => r.TestCount).Average(),
                MeanCoverage = trialResults.Select(r => r.Coverage).Average(),
                MeanDiversity = trialResults.Select(r => r.Diversity).Average()
            });
        }

        PrintDetailedResults("OnlookerSelectionRatio", results);
        PerformStatisticalAnalysis("OnlookerSelectionRatio", results);
    }

    [Test]
    public void Test_07_ScoutSelectionRatio()
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("SCOUTSELECTIONRATIO - COMPREHENSIVE ANALYSIS");
        Console.WriteLine("Scenario: Complex Form Validation (7 parameters, 53 test values)");
        Console.WriteLine(new string('=', 100) + "\n");

        var testValues = new[] { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5 };
        var results = new List<ParameterTestResult>();

        foreach (var ratio in testValues)
        {
            var trialResults = RunTrials(config =>
            {
                config.ScoutSelectionRatio = ratio;
                config.EnableScoutPhase = ratio > 0;
                config.StagnationThresholdPercentage = 0.5; // Scout phase starts after 50 generations (with 100 total)
                config.TotalPopulationGenerations = DEFAULT_GENERATIONS;
            });

            results.Add(new ParameterTestResult
            {
                Value = ratio,
                MeanScore = trialResults.Select(r => r.TotalScore).Average(),
                StdDev = trialResults.Select(r => r.TotalScore).StandardDeviation(),
                MeanTestCount = trialResults.Select(r => r.TestCount).Average(),
                MeanCoverage = trialResults.Select(r => r.Coverage).Average(),
                MeanDiversity = trialResults.Select(r => r.Diversity).Average()
            });
        }

        PrintDetailedResults("ScoutSelectionRatio", results);
        PerformStatisticalAnalysis("ScoutSelectionRatio", results);
    }

    [Test]
    public void Test_08_StagnationThresholdPercentage()
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("STAGNATIONTHRESHOLDPERCENTAGE - COMPREHENSIVE ANALYSIS");
        Console.WriteLine("Scenario: Complex Form Validation (7 parameters, 53 test values)");
        Console.WriteLine(new string('=', 100) + "\n");

        var testValues = new[] { 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9 };
        var results = new List<ParameterTestResult>();
        var totalGenerations = DEFAULT_GENERATIONS; // Use 100 generations

        foreach (var threshold in testValues)
        {
            var trialResults = RunTrials(config =>
            {
                config.StagnationThresholdPercentage = threshold;
                config.EnableScoutPhase = true;
                config.ScoutSelectionRatio = 0.3;
                config.TotalPopulationGenerations = totalGenerations;
            });

            var scoutStartGen = (int)(totalGenerations * threshold) + 1;
            var scoutActiveGens = totalGenerations - scoutStartGen + 1;

            results.Add(new ParameterTestResult
            {
                Value = threshold,
                MeanScore = trialResults.Select(r => r.TotalScore).Average(),
                StdDev = trialResults.Select(r => r.TotalScore).StandardDeviation(),
                MeanTestCount = trialResults.Select(r => r.TestCount).Average(),
                MeanCoverage = trialResults.Select(r => r.Coverage).Average(),
                MeanDiversity = trialResults.Select(r => r.Diversity).Average(),
                AdditionalMetric = scoutActiveGens // Scout active generations
            });
        }

        PrintDetailedResults("StagnationThresholdPercentage", results, additionalMetricName: "ScoutGens");
        PerformStatisticalAnalysis("StagnationThresholdPercentage", results);
    }

    [Test]
    public void Test_09_RunAllParametersSequentially()
    {
        Console.WriteLine("\n" + new string('=', 120));
        Console.WriteLine("RUNNING ALL PARAMETER TESTS SEQUENTIALLY");
        Console.WriteLine("This will take several minutes to complete...");
        Console.WriteLine(new string('=', 120) + "\n");

        var sw = Stopwatch.StartNew();

        Test_01_FinalPopulationSelectionRatio();
        Test_02_EliteSelectionRatio();
        Test_03_TotalGenerations();
        Test_04_MutationRate();
        Test_05_CoolingRate();
        Test_06_OnlookerSelectionRatio();
        Test_07_ScoutSelectionRatio();
        Test_08_StagnationThresholdPercentage();

        sw.Stop();

        Console.WriteLine("\n" + new string('=', 120));
        Console.WriteLine($"ALL TESTS COMPLETED in {sw.Elapsed.TotalMinutes:F1} minutes");
        Console.WriteLine(new string('=', 120));

        //GenerateFinalParameterRecommendations();
    }

    // Helper methods
    private List<TrialResult> RunTrials(Action<ABCGenerationSettings> configureSettings)
    {
        var results = new List<TrialResult>();

        for (int trial = 0; trial < TRIALS_PER_CONFIG; trial++)
        {
            var config = new ABCGenerationSettings
            {
                Seed = 42 + trial * 7 // Different seeds for each trial
            };
            configureSettings(config);

            var generator = new HybridArtificialBeeColonyTestCaseGenerator(config);
            var testSuite = generator.RunABCAlgorithm(_detailedFormScenario);

            results.Add(new TrialResult
            {
                TotalScore = testSuite.Sum(tc => tc.Score),
                TestCount = testSuite.Count,
                Coverage = CalculateCoverage(testSuite),
                Diversity = CalculateDiversity(testSuite)
            });
        }

        return results;
    }

    private double CalculateCoverage(HashSet<TestCase> testSuite)
    {
        if (testSuite.Count == 0) return 0;

        var coveredValues = new HashSet<string>();
        foreach (var tc in testSuite)
        {
            for (int i = 0; i < tc.Values.Count; i++)
            {
                coveredValues.Add($"{i}:{tc.Values[i].Value}");
            }
        }

        // Total possible values from the scenario (53 values across 7 parameters)
        var totalPossibleValues = _detailedFormScenario.Sum(p => p.TestValues.Count());

        return (double)coveredValues.Count / totalPossibleValues;
    }

    private double CalculateDiversity(HashSet<TestCase> testSuite)
    {
        if (testSuite.Count == 0) return 0;

        var uniqueTests = testSuite
            .Select(tc => string.Join(",", tc.Values.Select(v => v.Value?.ToString() ?? "null")))
            .Distinct()
            .Count();

        return (double)uniqueTests / testSuite.Count;
    }

    private void PrintDetailedResults(string parameterName, List<ParameterTestResult> results,
        bool includeTime = false, string additionalMetricName = null)
    {
        var header = $"{"Value",8} | {"Score",10} | {"StdDev",8} | {"Tests",6} | {"Coverage",8} | {"Diversity",9}";
        if (includeTime) header += $" | {"Time(ms)",8}";
        if (additionalMetricName != null) header += $" | {additionalMetricName,10}";

        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));

        foreach (var result in results)
        {
            var line = $"{result.Value,8:F2} | {result.MeanScore,10:F1} | {result.StdDev,8:F1} | " +
                      $"{result.MeanTestCount,6:F1} | {result.MeanCoverage,8:P1} | {result.MeanDiversity,9:F3}";
            if (includeTime) line += $" | {result.ExecutionTime,8:F0}";
            if (additionalMetricName != null) line += $" | {result.AdditionalMetric,10:F3}";

            Console.WriteLine(line);
        }
    }

    private void PerformStatisticalAnalysis(string parameterName, List<ParameterTestResult> results)
    {
        Console.WriteLine($"\n📊 Statistical Analysis for {parameterName}:");

        // Find optimal value
        var optimal = results.OrderByDescending(r => r.MeanScore).First();
        Console.WriteLine($"   Optimal value: {optimal.Value:F2} (Score: {optimal.MeanScore:F1} ± {optimal.StdDev:F1})");

        // Calculate effect size between best and worst
        var worst = results.OrderBy(r => r.MeanScore).First();
        if (optimal.StdDev > 0 || worst.StdDev > 0)
        {
            var pooledStdDev = Math.Sqrt((optimal.StdDev * optimal.StdDev + worst.StdDev * worst.StdDev) / 2);
            if (pooledStdDev > 0)
            {
                var cohensD = Math.Abs(optimal.MeanScore - worst.MeanScore) / pooledStdDev;
                Console.WriteLine($"   Effect size (Cohen's d): {cohensD:F2} ({GetEffectSizeInterpretation(cohensD)})");
            }
        }

        // Find recommended range (values within 90% of optimal)
        var threshold = optimal.MeanScore * 0.9;
        var recommendedRange = results.Where(r => r.MeanScore >= threshold).ToList();
        if (recommendedRange.Count > 1)
        {
            var minRecommended = recommendedRange.Min(r => r.Value);
            var maxRecommended = recommendedRange.Max(r => r.Value);
            Console.WriteLine($"   Recommended range: {minRecommended:F2} - {maxRecommended:F2}");
        }

        // 95% Confidence Interval for optimal
        if (optimal.StdDev > 0)
        {
            var ci95 = 1.96 * optimal.StdDev / Math.Sqrt(TRIALS_PER_CONFIG);
            Console.WriteLine($"   95% CI for optimal: [{optimal.MeanScore - ci95:F1}, {optimal.MeanScore + ci95:F1}]");
        }

        // Statistical significance test (simplified t-test comparison to median)
        var median = results.OrderBy(r => r.Value).ElementAt(results.Count / 2);
        if (optimal.StdDev > 0 && median.StdDev > 0)
        {
            var tStatistic = Math.Abs(optimal.MeanScore - median.MeanScore) /
                           Math.Sqrt((optimal.StdDev * optimal.StdDev + median.StdDev * median.StdDev) / TRIALS_PER_CONFIG);
            var pValue = tStatistic > 2.776 ? "p < 0.01" : tStatistic > 1.96 ? "p < 0.05" : "p > 0.05";
            Console.WriteLine($"   Significance vs median: {pValue} (t = {tStatistic:F2})");
        }

        Console.WriteLine();
    }

    private string GetEffectSizeInterpretation(double cohensD)
    {
        if (cohensD < 0.2) return "negligible";
        if (cohensD < 0.5) return "small";
        if (cohensD < 0.8) return "medium";
        if (cohensD >= 1.2) return "very large";
        return "large";
    }

    // Data structures
    private class TrialResult
    {
        public double TotalScore { get; set; }
        public int TestCount { get; set; }
        public double Coverage { get; set; }
        public double Diversity { get; set; }
    }

    private class ParameterTestResult
    {
        public double Value { get; set; }
        public double MeanScore { get; set; }
        public double StdDev { get; set; }
        public double MeanTestCount { get; set; }
        public double MeanCoverage { get; set; }
        public double MeanDiversity { get; set; }
        public double ExecutionTime { get; set; }
        public double AdditionalMetric { get; set; }
    }

    //[Test]
    //public void GenerateFinalParameterRecommendations()
    //{
    //    var sb = new StringBuilder();
    //    sb.AppendLine("\n" + new string('=', 120));
    //    sb.AppendLine("FINAL ABC PARAMETER RECOMMENDATIONS");
    //    sb.AppendLine("Based on Comprehensive Statistical Analysis with Detailed Form Validation Scenario");
    //    sb.AppendLine("Scenario: 7 form fields with 51 test values (Name, Email, Phone, Password, Age, Terms, Country)");
    //    sb.AppendLine(new string('=', 120));
    //    sb.AppendLine();
    //    sb.AppendLine("Parameter                      | Range      | Default | Optimal | Recommended  | Statistical Evidence");
    //    sb.AppendLine(new string('-', 120));
    //    sb.AppendLine("FinalPopulationSelectionRatio | [0.1-0.9]  | 0.5     | TBD     | 0.3-0.6      | Effect size, 95% CI");
    //    sb.AppendLine("EliteSelectionRatio           | [0.0-0.8]  | 0.3     | TBD     | 0.2-0.5      | Effect size, 95% CI");
    //    sb.AppendLine("TotalGenerations              | [5-150]    | 50      | TBD     | 20-50        | Convergence analysis");
    //    sb.AppendLine("MutationRate                  | [0.0-1.0]  | 0.3     | TBD     | 0.2-0.5      | Effect size, 95% CI");
    //    sb.AppendLine("CoolingRate*                  | [0.5-0.99] | 0.95    | TBD     | 0.85-0.95    | *Requires EnforceMutationUniqueness=false");
    //    sb.AppendLine("OnlookerSelectionRatio        | [0.0-0.4]  | 0.1     | TBD     | 0.05-0.15    | Effect size, 95% CI");
    //    sb.AppendLine("ScoutSelectionRatio           | [0.0-0.5]  | 0.3     | TBD     | 0.2-0.4      | Effect size, 95% CI");
    //    sb.AppendLine("StagnationThresholdPercentage | [0.5-0.9]  | 0.75    | TBD     | 0.6-0.7      | Scout activation timing");
    //    sb.AppendLine(new string('=', 120));
    //    sb.AppendLine();
    //    sb.AppendLine("Key Insights:");
    //    sb.AppendLine("1. CoolingRate only affects algorithm when EnforceMutationUniqueness=false (Simulated Annealing mode)");
    //    sb.AppendLine("2. Lower StagnationThresholdPercentage (0.6) allows more scout phase generations");
    //    sb.AppendLine("3. Parameters interact strongly - test in combination, not isolation");
    //    sb.AppendLine("4. Statistical validation: 5 trials per configuration, Cohen's d effect size, 95% confidence intervals");
    //    sb.AppendLine();
    //    sb.AppendLine("Testing Methodology:");
    //    sb.AppendLine("- Trials per configuration: 5 with different random seeds");
    //    sb.AppendLine("- Metrics: Score, Coverage, Diversity, Standard Deviation");
    //    sb.AppendLine("- Statistical tests: t-test, Cohen's d effect size, 95% CI");
    //    sb.AppendLine("- Recommended ranges: Values within 90% of optimal performance");

    //    Console.WriteLine(sb.ToString());
    //    System.IO.File.WriteAllText("final_parameter_recommendations.txt", sb.ToString());
    //    Console.WriteLine("\n✅ Final recommendations saved to final_parameter_recommendations.txt");
    //}
}