using System;

namespace Testimize.Tests.WeightOptimizationStudy;

/// <summary>
/// Configuration class for TestCaseEvaluator weights used in scientific optimization studies.
/// </summary>
public class EvaluatorWeights
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
    public static EvaluatorWeights Default() => new()
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

    public static EvaluatorWeights AllEqual() => new()
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

    public static EvaluatorWeights NoDiversity() => new()
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

    public static EvaluatorWeights HighPenalty() => new()
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

    public static EvaluatorWeights BoundaryFocused() => new()
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

    public static EvaluatorWeights DiversityFocused() => new()
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

    public static EvaluatorWeights Conservative() => new()
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

    public static EvaluatorWeights Aggressive() => new()
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