namespace Testimize.Tests.ParameterSensitivityAnalysis;

public partial class ComprehensiveParameterTests
{
    // Data structures
    private class TrialResult
    {
        public double TotalScore { get; set; }
        public int TestCount { get; set; }
        public double Coverage { get; set; }
        public double Diversity { get; set; }
    }
}