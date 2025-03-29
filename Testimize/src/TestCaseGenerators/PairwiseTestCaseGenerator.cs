using NUnit.Framework.Internal.Builders;
using Testimize.Contracts;
using Testimize.Parameters.Core;

namespace Testimize.TestCaseGenerators;

public static class PairwiseTestCaseGenerator
{
    public static HashSet<TestCase> GenerateTestCases(List<IInputParameter> parameters)
    {
        if (parameters == null || parameters.Count < 2)
        {
            throw new ArgumentException("Pairwise testing requires at least two parameters.");
        }

        // Convert parameters into a format suitable for NUnit's PairwiseStrategy
        var parameterValues = parameters.Select(p => p.TestValues.Cast<object>().ToArray()).ToArray();
        var pairwiseCombinations = GeneratePairwiseCombinations(parameterValues);

        // Convert NUnit's test case format to Testimize TestCase objects
        var uniqueTestCases = new HashSet<TestCase>(pairwiseCombinations);
        return uniqueTestCases;
    }

    private static List<TestCase> GeneratePairwiseCombinations(object[][] parameterValues)
    {
        var testCases = new List<TestCase>();

        // Use NUnit's PairwiseStrategy
        var strategy = new PairwiseStrategy();
        var generatedPairs = strategy.GetTestCases(parameterValues);

        // Convert generated pairs into TestCase objects
        foreach (var generatedPair in generatedPairs)
        {
            var testCase = new TestCase();
            testCase.Values.AddRange(generatedPair.Arguments.Cast<TestValue>());
            testCases.Add(testCase);
        }

        return testCases;
    }
}
