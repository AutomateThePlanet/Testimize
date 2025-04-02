// <copyright file="MSTestTestMethodAttributeOutputGenerator.cs" company="Automate The Planet Ltd.">
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

using System.Text;
using Testimize.Parameters.Core;
using TextCopy;

namespace Testimize.OutputGenerators;
public class MSTestTestMethodAttributeOutputGenerator : TestCaseOutputGenerator
{
    public override void GenerateOutput(string methodName, IEnumerable<TestCase> testCases, TestCaseCategory testCaseCategory = TestCaseCategory.All)
    {
        var sb = new StringBuilder();
        var testCasesToBeGenerated = FilterTestCasesByCategory(testCases, testCaseCategory);

        Console.WriteLine($"🧪 Total test cases: {testCasesToBeGenerated.Count()}");

        foreach (var testCase in testCasesToBeGenerated)
        {
            var values = testCase.Values.Select(x => ToLiteral(x.Value)).ToList();
            var expectedMessage = testCase.Values.FirstOrDefault(v =>
                !string.IsNullOrWhiteSpace(v.ExpectedInvalidMessage))?.ExpectedInvalidMessage;
            values.Add(ToLiteral(expectedMessage ?? string.Empty));

            sb.AppendLine($"[DataRow({string.Join(", ", values)})]");
        }

        var output = sb.ToString();
        Console.WriteLine(output);
        ClipboardService.SetText(output);
        Console.WriteLine("✅ MSTest DataRow attributes copied to clipboard.");
    }

    private static string ToLiteral(object value)
    {
        return value switch
        {
            null => "null",
            string s => $"\"{EscapeString(s)}\"",
            bool b => b.ToString().ToLowerInvariant(),
            DateTime dt => $"\"{dt:dd-MM-yyyy}\"",
            string[] arr => $"new[] {{ {string.Join(", ", arr.Select(a => $"\"{EscapeString(a)}\""))} }}",
            _ => value.ToString()
        };
    }

    private static string EscapeString(string input) =>
        input.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
