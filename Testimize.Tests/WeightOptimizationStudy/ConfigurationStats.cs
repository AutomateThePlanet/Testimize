// <copyright file="ConfigurationStats.cs" company="Automate The Planet Ltd.">
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
namespace Testimize.Tests.WeightOptimizationStudy;

/// <summary>
/// Statistical results for a weight configuration
/// </summary>
public class ConfigurationStats
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