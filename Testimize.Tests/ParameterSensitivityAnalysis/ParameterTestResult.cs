namespace Testimize.Tests.ParameterSensitivityAnalysis;

public partial class ComprehensiveParameterTests
{
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
}