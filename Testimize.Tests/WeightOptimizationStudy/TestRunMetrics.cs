// <copyright file="TestRunMetrics.cs" company="Automate The Planet Ltd.">
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
/// Metrics collected for each test run
/// </summary>
public class TestRunMetrics
{
    public int TestCaseCount { get; set; }
    public double CoverageRatio { get; set; }  // Unique combinations / Total possible
    public double DiversityScore { get; set; }  // Standard deviation of value distribution
    public double BoundaryRatio { get; set; }  // Boundary values / Total values
    public double OverallScore { get; set; }   // Composite metric
    public long ExecutionTimeMs { get; set; }
}