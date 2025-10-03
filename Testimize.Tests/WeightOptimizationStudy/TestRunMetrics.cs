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
using System.Collections.Generic;

namespace Testimize.Tests.WeightOptimizationStudy;

public class TestRunMetrics
{
    public string ConfigName { get; set; } = string.Empty;
    public string ScenarioName { get; set; } = string.Empty;
    public string ABCConfigName { get; set; } = string.Empty;

    // ABC metrics from generator
    public double ABCScoreSum { get; set; }
    public double ABCScoreMean { get; set; }
    public double ABCScoreMin { get; set; }
    public double ABCScoreMax { get; set; }

    // Test suite metrics
    public int TestCount { get; set; }
    public double Coverage { get; set; }
    public double DiversityScore { get; set; }
    public double BoundaryRatio { get; set; }
    public double ValidRatio { get; set; }
    public double InvalidRatio { get; set; }

    // Detailed category counts
    public int BoundaryValidCount { get; set; }
    public int ValidCount { get; set; }
    public int BoundaryInvalidCount { get; set; }
    public int InvalidCount { get; set; }

    // Composite scores
    public Dictionary<string, double> CompositeScores { get; set; } = new();

    // Performance metrics
    public double GenerationTimeMs { get; set; }

    // Seed for reproducibility
    public int UsedSeed { get; set; }
}