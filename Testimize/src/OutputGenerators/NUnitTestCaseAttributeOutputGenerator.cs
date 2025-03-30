// // <copyright file="NUnitTestCaseAttributeOutputGenerator.cs" company="Automate The Planet Ltd.">
// // Copyright 2025 Automate The Planet Ltd.
// // Licensed under the Apache License, Version 2.0 (the "License");
// // You may not use this file except in compliance with the License.
// // You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// // Unless required by applicable law or agreed to in writing,
// // software distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// // </copyright>
// // <author>Anton Angelov</author>
// // <site>https://automatetheplanet.com/</site>

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

        sb.AppendLine("🔹 **Generated NUnit [TestCase(...)] Attributes:**\n");

        foreach (var testCase in FilterTestCasesByCategory(testCases, testCaseCategory))
        {
            var values = testCase.Values.Select(x => ToLiteral(x.Value)).ToList();

            var expectedMessage = testCase.Values.FirstOrDefault(v =>
                !string.IsNullOrWhiteSpace(v.ExpectedInvalidMessage))?.ExpectedInvalidMessage;

            if (!string.IsNullOrEmpty(expectedMessage))
            {
                sb.AppendLine($"[TestCase({string.Join(", ", values)}, ExpectedResult = {ToLiteral(expectedMessage)})]");
            }
            else
            {
                sb.AppendLine($"[TestCase({string.Join(", ", values)})]");
            }
        }

        var output = sb.ToString();

        Console.WriteLine(output);
        Debug.WriteLine(output);

        ClipboardService.SetText(output);
        Console.WriteLine("✅ Attributes copied to clipboard.");
    }

    private static string ToLiteral(object value)
    {
        return value switch
        {
            null => "null",
            string s => $"\"{s.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"",
            bool b => b.ToString().ToLowerInvariant(),
            _ => value.ToString()
        };
    }
}

//Example Output
//[TestCase("US", "en", "EU")]
//[TestCase("XX", "en", "EU", ExpectedResult = "Country code is invalid")]
//[TestCase("U1", "fr", "AS", ExpectedResult = "Country code must contain only letters")]
