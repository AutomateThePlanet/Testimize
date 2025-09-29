// <copyright file="ConfigurationStatistics.cs" company="Automate The Planet Ltd.">
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
using System.Collections.Generic;
using static Testimize.Tests.WeightOptimizationStudy.ComprehensiveWeightOptimizationTests;

namespace Testimize.Tests.WeightOptimizationStudy;

public class ConfigurationStatistics
{
    public string ConfigName { get; set; } = string.Empty;
    public Dictionary<string, WeightSchemeStats> WeightSchemeStats { get; set; } = new();
    public double MeanCoverage { get; set; }
    public double StdDevCoverage { get; set; }
    public double MeanDiversity { get; set; }
    public double StdDevDiversity { get; set; }
    public double MeanBoundaryRatio { get; set; }
    public double StdDevBoundaryRatio { get; set; }
    public double MeanGenerationTime { get; set; }
    public double StdDevGenerationTime { get; set; }
    public int TotalRuns { get; set; }
    public double MeanABCScore { get; set; }
    public double StdDevABCScore { get; set; }
    public Dictionary<string, int> RankByScheme { get; set; } = new();
    public double AverageRank { get; set; }
    public double RankStdDev { get; set; }
}