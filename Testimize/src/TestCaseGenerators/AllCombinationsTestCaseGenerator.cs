// <copyright file="AllCombinationsTestCaseGenerator.cs" company="Automate The Planet Ltd.">
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

using System.Collections.Concurrent;
using Testimize.Contracts;
using Testimize.Parameters.Core;

namespace Testimize.TestCaseGenerators;

public static class AllCombinationsTestCaseGenerator
{
    public static HashSet<TestCase> GenerateTestCases(List<IInputParameter> parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            throw new ArgumentException("Test case generation requires at least one parameter.");
        }

        return GenerateAllCombinations(parameters);
    }

    private static HashSet<TestCase> GenerateAllCombinations(List<IInputParameter> parameters)
    {
        // Extract possible values for each parameter
        var parameterValues = parameters.Select(p => p.TestValues).ToList();

        // Concurrent set for thread-safe operations
        var testCases = new ConcurrentBag<TestCase>();

        // Generate Cartesian Product in Parallel
        var allCombinations = parameterValues
            .AsParallel() // Enables parallel execution
            .Aggregate(
                (IEnumerable<IEnumerable<TestValue>>)new[] { Enumerable.Empty<TestValue>() },
                (prev, next) => prev.SelectMany(
                    x => next.AsParallel(), // Parallelize selection of next parameter
                    (x, y) => x.Append(y)
                )
            );

        // Convert to TestCase objects in Parallel
        Parallel.ForEach(allCombinations, combination =>
        {
            testCases.Add(new TestCase { Values = combination.ToList() });
        });

        return testCases.ToHashSet(); // Enforce uniqueness
    }
}
