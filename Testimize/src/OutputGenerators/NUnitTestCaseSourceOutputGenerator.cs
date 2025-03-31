// <copyright file="NUnitTestCaseSourceOutputGenerator.cs" company="Automate The Planet Ltd.">
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

public class NUnitTestCaseSourceOutputGenerator : TestCaseOutputGenerator
{
    public override void GenerateOutput(string methodName, IEnumerable<TestCase> testCases, TestCaseCategory testCaseCategory = TestCaseCategory.All)
    {
        var sb = new StringBuilder();

        sb.AppendLine("\n🔹 **Generated NUnit TestCaseSource Method:**\n");
        sb.AppendLine($"public static IEnumerable<object[]> {methodName}()");
        sb.AppendLine("{");
        sb.AppendLine("    return new List<object[]");
        sb.AppendLine("    {");

        foreach (var testCase in FilterTestCasesByCategory(testCases, testCaseCategory))
        {
            var values = testCase.Values.Select(x => $"\"{x.Value}\"").ToList();
            var message = testCase.Values.FirstOrDefault(v => !string.IsNullOrEmpty(v.ExpectedInvalidMessage))?.ExpectedInvalidMessage;
            if (!string.IsNullOrEmpty(message))
            {
                values.Add($"\"{message}\"");
            }

            sb.AppendLine($"        new object[] {{ {string.Join(", ", values)} }},");
        }

        sb.AppendLine("    };");
        sb.AppendLine("}");

        var output = sb.ToString();

        Console.WriteLine(output);
        Debug.WriteLine(output);

        ClipboardService.SetText(output);
        Console.WriteLine("✅ Method copied to clipboard.");
    }
}