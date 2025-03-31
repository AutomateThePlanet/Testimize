// <copyright file="ABCTestCaseSourceAttribute.cs" company="Automate The Planet Ltd.">
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

using System.Diagnostics;
using System.Reflection;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Builders;
using NUnit.Framework.Internal;
using Testimize.Contracts;
using Testimize.OutputGenerators;
using Testimize.Parameters.Core;
using Testimize.TestCaseGenerators;
using Testimize;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ABCTestCaseSourceAttribute : Attribute, ITestBuilder
{
    private readonly string _sourceMethodName;
    private readonly TestCaseCategory _category;
    private readonly ABCGenerationSettings _abcConfig;

    public ABCTestCaseSourceAttribute(
        string sourceMethodName,
        TestCaseCategory category,
        int totalPopulationGenerations = 100,
        double mutationRate = 0.5,
        double finalPopulationSelectionRatio = 0.6,
        double eliteSelectionRatio = 0.6,
        double onlookerSelectionRatio = 0.5,
        double scoutSelectionRatio = 0.5,
        bool enableOnlookerSelection = true,
        bool enableScoutPhase = true,
        bool enforceMutationUniqueness = false,
        double stagnationThresholdPercentage = 0.75,
        double coolingRate = 85,
        bool allowMultipleInvalidInputs = false)
    {
        _sourceMethodName = sourceMethodName;
        _category = category;
        _abcConfig = new ABCGenerationSettings
        {
            TotalPopulationGenerations = totalPopulationGenerations,
            MutationRate = mutationRate,
            FinalPopulationSelectionRatio = finalPopulationSelectionRatio,
            EliteSelectionRatio = eliteSelectionRatio,
            OnlookerSelectionRatio = onlookerSelectionRatio,
            ScoutSelectionRatio = scoutSelectionRatio,
            EnableOnlookerSelection = enableOnlookerSelection,
            EnableScoutPhase = enableScoutPhase,
            EnforceMutationUniqueness = enforceMutationUniqueness,
            StagnationThresholdPercentage = stagnationThresholdPercentage,
            CoolingRate = coolingRate,
            AllowMultipleInvalidInputs = allowMultipleInvalidInputs
        };
    }

    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
    {
        // Retrieve test parameter method dynamically via reflection
        MethodInfo sourceMethod = method.TypeInfo.Type.GetMethod(_sourceMethodName, BindingFlags.Public | BindingFlags.Static);
        if (sourceMethod == null)
        {
            throw new InvalidOperationException($"No static method named '{_sourceMethodName}' found in test class.");
        }

        // Get test parameters from the test class
        var parameters = sourceMethod.Invoke(null, null) as List<IInputParameter>;
        if (parameters == null)
        {
            throw new InvalidOperationException("The method did not return a valid List<IInputParameter>.");
        }

        // Start measuring time
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Initialize ABC Generator with overridden config
        var abcGenerator = new HybridArtificialBeeColonyTestCaseGenerator(_abcConfig);
        var testCases = abcGenerator.RunABCAlgorithm(parameters);

        // Stop measuring time
        stopwatch.Stop();

        // Debug output for generation time
        Console.WriteLine($"Test case generation completed in {stopwatch.ElapsedMilliseconds} ms.");

        // Filter test cases based on TestCaseCategory
        IEnumerable<TestCase> filteredCases = FilterTestCasesByCategory(testCases, _category);

        // Create NUnit test cases dynamically
        foreach (var testCase in filteredCases)
        {
            var parameters1 = new TestCaseParameters(testCase.Values.Select(v => (object)v.Value).ToArray());

            yield return new NUnitTestCaseBuilder().BuildTestMethod(
                method,
                suite,
                parameters1);
        }
    }

    private IEnumerable<TestCase> FilterTestCasesByCategory(IEnumerable<TestCase> testCases, TestCaseCategory category)
    {
        return category switch
        {
            TestCaseCategory.Valid => testCases.Where(tc => tc.Values.All(v => v.Category is TestValueCategory.Valid or TestValueCategory.BoundaryValid)),
            TestCaseCategory.Validation => testCases.Where(tc => tc.Values.Any(v => v.Category is TestValueCategory.Invalid or TestValueCategory.BoundaryInvalid)),
            _ => testCases // All cases
        };
    }
}
