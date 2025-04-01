// <copyright file="NUnitTestCaseAttributeOutputGenerator.cs" company="Automate The Planet Ltd.">
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
using System.Text;
using Testimize.Parameters.Core;
using TextCopy;

namespace Testimize.OutputGenerators;


public class NUnitTestCaseAttributeOutputGenerator : TestCaseOutputGenerator
{
    public override void GenerateOutput(string methodName, IEnumerable<TestCase> testCases, TestCaseCategory testCaseCategory = TestCaseCategory.All)
    {
        var sb = new StringBuilder();

        var multiInvalidCount = testCases.Count(
            tc => tc.Values.Count(v =>
                v.Category is TestValueCategory.Invalid or TestValueCategory.BoundaryInvalid) > 1);
        var testCasesToBeGenerated = FilterTestCasesByCategory(testCases, testCaseCategory);
        Debug.WriteLine($"🧪 Total test cases to be generated: {testCasesToBeGenerated.Count()}");
        Console.WriteLine($"\U0001f9ea Total test cases to be generated: {testCasesToBeGenerated.Count()}");

        Debug.WriteLine($"🧪 Total test cases with more than one invalid input: {multiInvalidCount}");
        Console.WriteLine($"🧪 Total test cases with more than one invalid input: {multiInvalidCount}");

        Console.WriteLine("🔹 **Generated NUnit [TestCase(...)] Attributes:**\n");
        Debug.WriteLine("🔹 **Generated NUnit [TestCase(...)] Attributes:**\n");

        foreach (var testCase in testCasesToBeGenerated)
        {
            var values = testCase.Values.Select(x => ToLiteral(x.Value)).ToList();

            var expectedMessage = testCase.Values.FirstOrDefault(v =>
                !string.IsNullOrWhiteSpace(v.ExpectedInvalidMessage))?.ExpectedInvalidMessage;

            values.Add(ToLiteral(expectedMessage ?? string.Empty));

            sb.AppendLine($"[TestCase({string.Join(", ", values)})]");
        }

        var output = sb.ToString();

        Console.WriteLine(output);
        Debug.WriteLine(output);

        ClipboardService.SetText(output);
        Console.WriteLine("✅ Attributes copied to clipboard.");
        Debug.WriteLine("✅ Attributes copied to clipboard.");
    }

    private static string ToLiteral(object value)
    {
        return value switch
        {
            null => "null",
            string s => $"\"{EscapeString(s)}\"",
            bool b => b.ToString().ToLowerInvariant(),
            DateTime dt => $"\"{dt:dd-MM-yyyy}\"", // ISO format
            string[] arr => $"new[] {{ {string.Join(", ", arr.Select(a => $"\"{EscapeString(a)}\""))} }}",
            _ => value.ToString()
        };
    }

    private static string EscapeString(string input) =>
        input.Replace("\\", "\\\\").Replace("\"", "\\\"");
}

//Example Output
//[TestCase("US", "en", "EU")]
//[TestCase("XX", "en", "EU", ExpectedResult = "Country code is invalid")]
//[TestCase("U1", "fr", "AS", ExpectedResult = "Country code must contain only letters")]
