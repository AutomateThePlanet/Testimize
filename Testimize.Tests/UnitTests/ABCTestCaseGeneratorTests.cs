﻿// <copyright file="ABCTestCaseGeneratorTests.cs" company="Automate The Planet Ltd.">
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

using System;
using System.Collections.Generic;
using System.Linq;
using Testimize.Parameters;
using Testimize.Contracts;
using Testimize.Parameters.Core;
using Testimize.TestCaseGenerators;

namespace Testimize.Tests.UnitTests;

[TestFixture]
public class ABCTestCaseGeneratorTests
{
    private List<IInputParameter> _parameters;
    private HybridArtificialBeeColonyTestCaseGenerator _abcGenerator;
    private HashSet<TestCase> _abcTestCases;
    private HashSet<TestCase> _pairwiseTestCases;
    private HashSet<TestCase> _filteredPairwiseTestCases;

    [SetUp]
    public void SetUp()
    {
        _parameters = new List<IInputParameter>
    {
        // new TextDataParameter(minBoundary: 6, maxBoundary: 12),
        //new EmailDataParameter(minBoundary: 5, maxBoundary: 10),
        //new PhoneDataParameter(minBoundary: 6, maxBoundary: 8),
        //new TextDataParameter(minBoundary: 4, maxBoundary: 10),
        new TextDataParameter(preciseMode: true, preciseTestValues: new[]
        {
            new TestValue("Normal1", TestValueCategory.Valid),
            new TestValue("BoundaryMin-1", TestValueCategory.BoundaryInvalid),
            new TestValue("BoundaryMin", TestValueCategory.BoundaryValid),
            new TestValue("BoundaryMax", TestValueCategory.BoundaryValid),
            new TestValue("BoundaryMax+1", TestValueCategory.BoundaryInvalid),
            new TestValue("Invalid1", TestValueCategory.Invalid)
        }),
        new EmailDataParameter(preciseMode: true, preciseTestValues: new[]
        {
            new TestValue("test@mail.comMIN-1", TestValueCategory.BoundaryInvalid),
            new TestValue("test@mail.comMIN", TestValueCategory.BoundaryValid),
            new TestValue("test@mail.comMAX", TestValueCategory.BoundaryValid),
            new TestValue("test@mail.comMAX+1", TestValueCategory.BoundaryInvalid),
            new TestValue("test@mail.com", TestValueCategory.Valid),
            new TestValue("invalid@mail", TestValueCategory.Invalid)
        }),
        new PhoneDataParameter(preciseMode: true, preciseTestValues: new[]
        {
            new TestValue("+359888888888", TestValueCategory.Valid),
            new TestValue("000000", TestValueCategory.Invalid)
        }),
        new TextDataParameter(preciseMode: true, preciseTestValues: new[]
        {
            new TestValue("NormalX", TestValueCategory.Valid)
        }),
    };
        var config = new ABCGenerationSettings
        {
            FinalPopulationSelectionRatio = 0.5,
            EliteSelectionRatio = 0.4,
            TotalPopulationGenerations = 20,
            MutationRate = 0.5,
            AllowMultipleInvalidInputs = false,
            EnableOnlookerSelection = true,
            EnableScoutPhase = true
        };
        _abcGenerator = new HybridArtificialBeeColonyTestCaseGenerator(config);
        _abcTestCases = _abcGenerator.RunABCAlgorithm(_parameters);

        _pairwiseTestCases = new PairwiseTestCaseGenerator().GenerateTestCases(_parameters);

        _filteredPairwiseTestCases = _pairwiseTestCases
            .Where(tc => tc.Values.Count(value =>
                _parameters.Any(p => p.TestValues.Any(tv => tv.Value == value.Value && tv.Category == TestValueCategory.Invalid))) <= 1)
            .ToHashSet();
    }

    [Test]
    [Category(Categories.CI)]
    public void CompareEfficiencyOfABCAndPairwiseReduction()
    {
        Console.WriteLine("================ TEST CASE REDUCTION ANALYSIS ================");
        Console.WriteLine($"Pairwise Generated Test Cases: {_pairwiseTestCases.Count}");
        Console.WriteLine($"Pairwise (Filtered) Test Cases: {_filteredPairwiseTestCases.Count}");
        Console.WriteLine($"Hybrid ABC Test Cases: {_abcTestCases.Count}");

        Assert.That(_abcTestCases.Count, Is.LessThanOrEqualTo(_filteredPairwiseTestCases.Count),
            "ABC should reduce test cases at least as efficiently as pairwise filtering.");

        Assert.That(_abcTestCases.Count, Is.LessThan(_pairwiseTestCases.Count),
            "ABC should produce fewer test cases than pure pairwise.");
    }

    [Test]
    [Category(Categories.CI)]
    public void AllBoundaryValuesAreTestedAtLeastOnce()
    {
        var allBoundaryValues = _parameters
            .SelectMany(p => p.TestValues.Where(tv => tv.Category == TestValueCategory.BoundaryInvalid || tv.Category == TestValueCategory.BoundaryValid))
            .Select(tv => tv.Value)
            .ToHashSet();

        var testedValues = _abcTestCases.SelectMany(tc => tc.Values.Select(tv => tv.Value)).ToHashSet();

      
        foreach (var boundaryValue in allBoundaryValues)
        {
            Assert.That(testedValues.Contains(boundaryValue), Is.True,
                $"Boundary value '{boundaryValue}' was not included in any test case.");
        }
    }

    [Test]
    [Category(Categories.CI)]
    public void EachParameterIsUsedAtLeastOnce()
    {
        var uncoveredParameters = new List<string>();

        foreach (var parameter in _parameters)
        {
            var isParameterCovered = _abcTestCases.Any(tc =>
                parameter.TestValues.Any(tv => tc.Values.Any(v => v.Value == tv.Value)));

            if (!isParameterCovered)
            {
                uncoveredParameters.Add(parameter.ToString());
            }
        }

        if (uncoveredParameters.Any())
        {
            Console.WriteLine("The following parameters were not covered in any test case:");
            foreach (var param in uncoveredParameters)
            {
                Console.WriteLine($"- {param}");
            }
        }

        Assert.That(uncoveredParameters, Is.Empty, "Some parameters were not covered in the generated test cases.");
    }

    [Test]
    [Category(Categories.CI)]
    public void NoTestCaseHasMoreThanOneInvalidInput()
    {
        foreach (var testCase in _abcTestCases)
        {
            var invalidCount = testCase.Values.Count(value =>
                _parameters.Any(p => p.TestValues.Any(tv => tv.Value == value.Value && tv.Category == TestValueCategory.Invalid)));

            Assert.That(invalidCount, Is.LessThanOrEqualTo(1),
                $"Test case [{string.Join(", ", testCase.Values.Select(v => v.Value))}] has more than one invalid input, violating the constraints.");
        }
    }

    [Test]
    [Category(Categories.CI)]
    public void GeneratedTestCasesAreUnique()
    {
        var distinctTestCases = _abcTestCases
            .GroupBy(tc => string.Join(",", tc.Values.Select(v => v.Value)))
            .Where(group => group.Count() > 1)
            .Select(group => new { TestCase = string.Join(",", group.First().Values.Select(v => v.Value)), Count = group.Count() })
            .ToList();

        foreach (var testCase in _abcTestCases)
        {
            Console.WriteLine(string.Join(',', testCase.Values.Select(v => v.Value)));
        }

        if (distinctTestCases.Any())
        {
            Console.WriteLine("Duplicate test cases found:");
            foreach (var duplicate in distinctTestCases)
            {
                Console.WriteLine($"Test Case: {duplicate.TestCase} - Repeated {duplicate.Count} times");
            }
        }

        Assert.That(distinctTestCases.Count, Is.EqualTo(0), "Duplicate test cases found in the generated set.");
    }
}
