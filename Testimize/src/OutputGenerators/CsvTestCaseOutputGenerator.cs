// // <copyright file="CsvTestCaseOutputGenerator.cs" company="Automate The Planet Ltd.">
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

public class CsvTestCaseOutputGenerator : TestCaseOutputGenerator
{
    public override void GenerateOutput(string methodName, IEnumerable<TestCase> testCases, TestCaseCategory testCaseCategory = TestCaseCategory.All)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"\n🔹 **Generated CSV Output ({methodName}):**\n");

        foreach (var testCase in FilterTestCasesByCategory(testCases, testCaseCategory))
        {
            var csvValues = testCase.Values.Select(v => $"\"{v.Value}\"").ToList();
            var message = testCase.Values.FirstOrDefault(v => !string.IsNullOrEmpty(v.ExpectedInvalidMessage))?.ExpectedInvalidMessage;
            if (!string.IsNullOrEmpty(message))
            {
                csvValues.Add($"\"{message}\"");
            }

            var csvLine = string.Join(",", csvValues);
            sb.AppendLine(csvLine);
        }

        var output = sb.ToString();

        Console.WriteLine(output);
        Debug.WriteLine(output);

        ClipboardService.SetText(output);
        Console.WriteLine("✅ CSV output copied to clipboard.");
    }
}