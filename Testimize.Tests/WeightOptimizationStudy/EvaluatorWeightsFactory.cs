// <copyright file="EvaluatorWeightsFactory.cs" company="Automate The Planet Ltd.">
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
/// Configuration class for TestCaseEvaluator weights used in scientific optimization studies.
/// </summary>
public class EvaluatorWeightsFactory
{
    public double BoundaryValidWeight { get; set; } = 20;
    public double ValidWeight { get; set; } = 2;
    public double BoundaryInvalidWeight { get; set; } = -1;
    public double InvalidWeight { get; set; } = -2;
    public double FirstTimeValueBonus { get; set; } = 25;
    public double MultipleInvalidPenaltyFactor { get; set; } = 50;
    public double FirstTimeMultiplierIncrement { get; set; } = 0.25;

    public string Name { get; set; } = "Custom";

    // Factory methods for predefined weight configurations
    public static EvaluatorWeightsFactory Default() => new()
    {
        Name = "Current (Baseline)",
        BoundaryValidWeight = 20,
        ValidWeight = 2,
        BoundaryInvalidWeight = -1,
        InvalidWeight = -2,
        FirstTimeValueBonus = 25,
        MultipleInvalidPenaltyFactor = 50,
        FirstTimeMultiplierIncrement = 0.25
    };

    public static EvaluatorWeightsFactory AllEqual() => new()
    {
        Name = "All Equal",
        BoundaryValidWeight = 10,
        ValidWeight = 10,
        BoundaryInvalidWeight = 10,
        InvalidWeight = 10,
        FirstTimeValueBonus = 10,
        MultipleInvalidPenaltyFactor = 10,
        FirstTimeMultiplierIncrement = 0.1
    };

    public static EvaluatorWeightsFactory NoDiversity() => new()
    {
        Name = "No Diversity Bonus",
        BoundaryValidWeight = 20,
        ValidWeight = 2,
        BoundaryInvalidWeight = -1,
        InvalidWeight = -2,
        FirstTimeValueBonus = 0,
        MultipleInvalidPenaltyFactor = 50,
        FirstTimeMultiplierIncrement = 0
    };

    public static EvaluatorWeightsFactory HighPenalty() => new()
    {
        Name = "High Invalid Penalty",
        BoundaryValidWeight = 20,
        ValidWeight = 2,
        BoundaryInvalidWeight = -5,
        InvalidWeight = -10,
        FirstTimeValueBonus = 25,
        MultipleInvalidPenaltyFactor = 100,
        FirstTimeMultiplierIncrement = 0.25
    };

    public static EvaluatorWeightsFactory BoundaryFocused() => new()
    {
        Name = "Boundary Focused",
        BoundaryValidWeight = 60,
        ValidWeight = 2,
        BoundaryInvalidWeight = -3,
        InvalidWeight = -2,
        FirstTimeValueBonus = 25,
        MultipleInvalidPenaltyFactor = 50,
        FirstTimeMultiplierIncrement = 0.25
    };

    public static EvaluatorWeightsFactory DiversityFocused() => new()
    {
        Name = "Diversity Focused",
        BoundaryValidWeight = 20,
        ValidWeight = 2,
        BoundaryInvalidWeight = -1,
        InvalidWeight = -2,
        FirstTimeValueBonus = 50,
        MultipleInvalidPenaltyFactor = 50,
        FirstTimeMultiplierIncrement = 0.5
    };

    public static EvaluatorWeightsFactory Conservative() => new()
    {
        Name = "Conservative",
        BoundaryValidWeight = 15,
        ValidWeight = 1,
        BoundaryInvalidWeight = -3,
        InvalidWeight = -5,
        FirstTimeValueBonus = 20,
        MultipleInvalidPenaltyFactor = 75,
        FirstTimeMultiplierIncrement = 0.15
    };

    public static EvaluatorWeightsFactory Aggressive() => new()
    {
        Name = "Aggressive",
        BoundaryValidWeight = 30,
        ValidWeight = 5,
        BoundaryInvalidWeight = -0.5,
        InvalidWeight = -1,
        FirstTimeValueBonus = 35,
        MultipleInvalidPenaltyFactor = 25,
        FirstTimeMultiplierIncrement = 0.35
    };
}