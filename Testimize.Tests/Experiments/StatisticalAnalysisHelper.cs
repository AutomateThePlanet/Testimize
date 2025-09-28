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
}