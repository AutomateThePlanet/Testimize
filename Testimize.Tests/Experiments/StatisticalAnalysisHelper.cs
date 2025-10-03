// <copyright file="StatisticalAnalysisHelper.cs" company="Automate The Planet Ltd.">
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
using MathNet.Numerics.Distributions;

namespace Testimize.Tests.Experiments;

/// <summary>
/// Utility class for performing statistical analysis, including confidence intervals,
/// paired t-tests, and descriptive statistics.
/// </summary>
public static class StatisticalAnalysisHelper
{
    /// <summary>
    /// Calculates a 95% confidence interval using the t-distribution.
    /// </summary>
    /// <param name="mean">The mean of the sample.</param>
    /// <param name="standardError">The standard error of the sample.</param>
    /// <param name="df">The degrees of freedom (sample size - 1).</param>
    /// <returns>
    /// A tuple containing the lower (lo) and upper (hi) bounds of the confidence interval.
    /// Returns (NaN, NaN) if input is invalid.
    /// </returns>
    public static (double lo, double hi) ConfidenceInterval95T(double mean, double standardError, int df)
    {
        if (df <= 0 || double.IsNaN(standardError) || double.IsInfinity(standardError))
            return (double.NaN, double.NaN);

        double tCrit = StudentT.InvCDF(0.0, 1.0, df, 0.975);
        double marginOfError = tCrit * standardError;
        return (mean - marginOfError, mean + marginOfError);
    }

    /// <summary>
    /// Performs a paired t-test to determine if there's a statistically significant difference
    /// between two paired samples.
    /// </summary>
    /// <param name="diffs">An array of differences between paired samples.</param>
    /// <returns>
    /// A tuple containing the t-statistic, degrees of freedom, and two-tailed p-value.
    /// Returns (NaN, 0, NaN) if input is invalid.
    /// </returns>
    public static (double tStatistic, int degreesOfFreedom, double pValueTwoTailed) PairedTTest(double[] diffs)
    {
        int n = diffs.Length;
        if (n <= 1) return (double.NaN, 0, double.NaN);

        double mean = diffs.Average();
        double sd = diffs.StandardDeviation();
        double se = sd / Math.Sqrt(n);

        if (se == 0) return (double.PositiveInfinity, n - 1, 0.0);

        double t = mean / se;
        int df = n - 1;

        var tDist = new StudentT(0.0, 1.0, df);
        double p = 2.0 * (1.0 - tDist.CumulativeDistribution(Math.Abs(t)));

        return (t, df, Math.Clamp(p, 0, 1));
    }

    /// <summary>
    /// Calculates descriptive statistics for a given list of values.
    /// </summary>
    /// <param name="values">A list of numeric values.</param>
    /// <returns>
    /// A tuple containing the mean, standard deviation, and standard error.
    /// Returns (NaN, NaN, NaN) if the input list is null or empty.
    /// </returns>
    public static (double mean, double sd, double se) CalculateDescriptiveStatistics(List<double> values)
    {
        if (values == null || values.Count == 0)
            return (double.NaN, double.NaN, double.NaN);

        double mean = values.Average();
        double sd = values.StandardDeviation();
        double se = sd / Math.Sqrt(values.Count);
        return (mean, sd, se);
    }

    /// <summary>
    /// Calculates the percentage improvement between two averages.
    /// </summary>
    /// <param name="avgAbc">The average value of the first sample (e.g., ABC scores).</param>
    /// <param name="avgPw">The average value of the second sample (e.g., pairwise scores).</param>
    /// <returns>
    /// The percentage improvement from avgPw to avgAbc. Returns NaN if avgPw is zero.
    /// </returns>
    public static double CalculateImprovement(double avgAbc, double avgPw)
    {
        if (avgPw == 0) return double.NaN;
        return (avgAbc - avgPw) / avgPw * 100.0;
    }

    /// <summary>
    /// Performs a two-sample t-test (Welch's t-test) to compare two independent samples with unequal variances.
    /// </summary>
    /// <param name="sample1">The first sample of values.</param>
    /// <param name="sample2">The second sample of values.</param>
    /// <returns>
    /// A tuple containing the t-statistic and two-tailed p-value.
    /// Returns (NaN, NaN) if either sample is too small (less than 2 values).
    /// </returns>
    public static (double tStatistic, double pValue) PerformTTest(double[] sample1, double[] sample2)
    {
        if (sample1.Length < 2 || sample2.Length < 2)
            return (double.NaN, double.NaN);

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

    /// <summary>
    /// Calculates Cohen's d effect size for two independent samples.
    /// </summary>
    /// <param name="sample1">The first sample of values.</param>
    /// <param name="sample2">The second sample of values.</param>
    /// <returns>
    /// Cohen's d effect size. Values: 0.2=small, 0.5=medium, 0.8=large.
    /// Returns NaN if samples are too small.
    /// </returns>
    public static double CalculateCohenD(double[] sample1, double[] sample2)
    {
        if (sample1.Length < 2 || sample2.Length < 2)
            return double.NaN;

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

    /// <summary>
    /// Formats a double value for statistical display with appropriate precision.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="decimalPlaces">Number of decimal places (default: 2).</param>
    /// <returns>Formatted string representation of the value.</returns>
    public static string FormatStatValue(double value, int decimalPlaces = 2)
    {
        if (double.IsNaN(value)) return "N/A";
        if (double.IsInfinity(value)) return value > 0 ? "∞" : "-∞";

        string format = $"F{decimalPlaces}";
        return value.ToString(format);
    }

    /// <summary>
    /// Formats a p-value for display with appropriate precision and notation.
    /// </summary>
    /// <param name="pValue">The p-value to format.</param>
    /// <returns>Formatted string representation of the p-value.</returns>
    public static string FormatPValue(double pValue)
    {
        if (double.IsNaN(pValue)) return "N/A";
        if (pValue < 0.001) return "<0.001";
        if (pValue > 0.999) return ">0.999";
        return pValue.ToString("F3");
    }

    /// <summary>
    /// Formats a percentage value for display.
    /// </summary>
    /// <param name="percentage">The percentage value to format.</param>
    /// <param name="decimalPlaces">Number of decimal places (default: 1).</param>
    /// <param name="includeSign">Whether to include + sign for positive values (default: false).</param>
    /// <returns>Formatted string representation of the percentage.</returns>
    public static string FormatPercentage(double percentage, int decimalPlaces = 1, bool includeSign = false)
    {
        if (double.IsNaN(percentage)) return "N/A";

        string format = includeSign && percentage > 0
            ? $"+{{0:F{decimalPlaces}}}%"
            : $"{{0:F{decimalPlaces}}}%";

        return string.Format(format, percentage);
    }

    /// <summary>
    /// Interprets Cohen's d effect size value.
    /// </summary>
    /// <param name="cohenD">Cohen's d value.</param>
    /// <returns>String interpretation of the effect size.</returns>
    public static string GetEffectSizeInterpretation(double cohenD)
    {
        var absD = Math.Abs(cohenD);
        if (absD < 0.2) return "Negligible";
        if (absD < 0.5) return "Small";
        if (absD < 0.8) return "Medium";
        return "Large";
    }

    /// <summary>
    /// Formats statistical results with mean ± standard deviation notation.
    /// </summary>
    /// <param name="mean">The mean value.</param>
    /// <param name="stdDev">The standard deviation.</param>
    /// <param name="decimalPlaces">Number of decimal places (default: 2).</param>
    /// <returns>Formatted string in "mean ± stdDev" format.</returns>
    public static string FormatMeanWithStdDev(double mean, double stdDev, int decimalPlaces = 2)
    {
        return $"{FormatStatValue(mean, decimalPlaces)} ± {FormatStatValue(stdDev, decimalPlaces)}";
    }

    /// <summary>
    /// Formats a confidence interval for display.
    /// </summary>
    /// <param name="lowerBound">The lower bound of the interval.</param>
    /// <param name="upperBound">The upper bound of the interval.</param>
    /// <param name="decimalPlaces">Number of decimal places (default: 2).</param>
    /// <returns>Formatted string in "[lower, upper]" format.</returns>
    public static string FormatConfidenceInterval(double lowerBound, double upperBound, int decimalPlaces = 2)
    {
        return $"[{FormatStatValue(lowerBound, decimalPlaces)}, {FormatStatValue(upperBound, decimalPlaces)}]";
    }
}