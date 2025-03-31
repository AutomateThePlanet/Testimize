// <copyright file="PairwiseTestCaseGeneratorTests.cs" company="Automate The Planet Ltd.">
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
using Testimize.Contracts;
using Testimize.Parameters.Core;
using Testimize.TestCaseGenerators;

namespace Testimize.Tests.UnitTests;

[TestFixture]
public class PairwiseTestCaseGeneratorTests
{
    private class MockInputParameter : IInputParameter
    {
        public string Name { get; }
        public List<TestValue> TestValues { get; }

        public MockInputParameter(string name, params string[] values)
        {
            Name = name;
            TestValues = values.Select(v => new TestValue(v, TestValueCategory.Valid)).ToList();
        }
    }

    [Test]
    [Category(Categories.CI)]
    public void GeneratesValidPairwiseCombinations()
    {
        var parameters = new List<IInputParameter>
        {
            new MockInputParameter("Param1", "A", "B", "C"),
            new MockInputParameter("Param2", "1", "2"),
            new MockInputParameter("Param3", "X", "Y")
        };

        var testCases = PairwiseTestCaseGenerator.GenerateTestCases(parameters);

        Assert.That(testCases, Is.Not.Null);
        Assert.That(testCases.Count, Is.GreaterThan(0), "Test cases should be generated.");

        var allPairs = new HashSet<(object, object)>();

        foreach (var testCase in testCases)
        {
            var values = testCase.Values.Select(v => v.Value).ToList();
            for (var i = 0; i < values.Count; i++)
            {
                for (var j = i + 1; j < values.Count; j++)
                {
                    allPairs.Add((values[i], values[j]));
                }
            }
        }

        var expectedPairs = new HashSet<(object, object)>();
        for (var i = 0; i < parameters.Count; i++)
        {
            for (int j = i + 1; j < parameters.Count; j++)
            {
                foreach (var value1 in parameters[i].TestValues)
                {
                    foreach (var value2 in parameters[j].TestValues)
                    {
                        expectedPairs.Add((value1.Value, value2.Value));
                    }
                }
            }
        }

        foreach (var pair in expectedPairs)
        {
            Assert.That(allPairs.Contains(pair), Is.True, $"Missing pair: {pair}");
        }
    }

    [Test]
    [Category(Categories.CI)]
    public void GeneratesMinimalNumberOfTestCases()
    {
        var parameters = new List<IInputParameter>
        {
            new MockInputParameter("Param1", "A", "B", "C"),
            new MockInputParameter("Param2", "1", "2"),
            new MockInputParameter("Param3", "X", "Y")
        };

        var testCases = PairwiseTestCaseGenerator.GenerateTestCases(parameters);
        Assert.That(testCases.Count, Is.LessThan(12), "Pairwise should generate fewer test cases than full cartesian.");
    }

    [Test]
    [Category(Categories.CI)]
    public void HandlesSingleParameterCase()
    {
        var parameters = new List<IInputParameter>
        {
            new MockInputParameter("Param1", "A", "B", "C")
        };

        var ex = Assert.Throws<ArgumentException>(() => PairwiseTestCaseGenerator.GenerateTestCases(parameters));
        Assert.That(ex.Message, Is.EqualTo("Pairwise testing requires at least two parameters."));
    }

    [Test]
    [Category(Categories.CI)]
    public void HandlesEmptyParameterList()
    {
        var ex = Assert.Throws<ArgumentException>(() => PairwiseTestCaseGenerator.GenerateTestCases(new List<IInputParameter>()));
        Assert.That(ex.Message, Is.EqualTo("Pairwise testing requires at least two parameters."));
    }

    [Test]
    [Category(Categories.CI)]
    public void ValidatesNoDuplicateTestCases()
    {
        var parameters = new List<IInputParameter>
        {
            new MockInputParameter("Param1", "A", "B", "C"),
            new MockInputParameter("Param2", "1", "2"),
            new MockInputParameter("Param3", "X", "Y")
        };

        var testCases = PairwiseTestCaseGenerator.GenerateTestCases(parameters);
        var uniqueTestCases = new HashSet<string>(testCases.Select(tc => string.Join(",", tc.Values.Select(v => v.Value))));

        Assert.That(uniqueTestCases.Count, Is.EqualTo(testCases.Count), "No duplicate test cases should be generated.");
    }
}