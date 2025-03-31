// <copyright file="TestCaseOutputGenerator.cs" company="Automate The Planet Ltd.">
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

using Testimize.Parameters.Core;

namespace Testimize.OutputGenerators;

public abstract class TestCaseOutputGenerator : ITestCaseOutputGenerator
{
    public abstract void GenerateOutput(string methodName, IEnumerable<TestCase> testCases, TestCaseCategory testCaseCategoty = TestCaseCategory.All);

    protected IEnumerable<TestCase> FilterTestCasesByCategory(IEnumerable<TestCase> testCases, TestCaseCategory testCaseCategoty)
    {
        return testCaseCategoty switch
        {
            TestCaseCategory.Valid => testCases.Where(tc => tc.Values.All(v => v.Category == TestValueCategory.Valid || v.Category == TestValueCategory.BoundaryValid)).ToHashSet(),
            TestCaseCategory.Validation => testCases.Where(tc => tc.Values.Any(v => v.Category == TestValueCategory.Invalid || v.Category == TestValueCategory.BoundaryInvalid)).ToHashSet(),
            _ => testCases // TestCaseCategoty.All - Keep all test cases
        };
    }
}
