// <copyright file="JsonTestCaseOutputGenerator.cs" company="Automate The Planet Ltd.">
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
using System.Text.Json;
using Testimize.Parameters.Core;
using TextCopy;

namespace Testimize.OutputGenerators;

public class JsonTestCaseOutputGenerator : TestCaseOutputGenerator
{
    public override void GenerateOutput(string methodName, IEnumerable<TestCase> testCases, TestCaseCategory testCaseCategory = TestCaseCategory.All)
    {
        var filteredTestCases = FilterTestCasesByCategory(testCases, testCaseCategory);

        var jsonReadyCases = filteredTestCases.Select(tc =>
        {
            var values = tc.Values.Select(v => v.Value).ToList();
            var message = tc.Values.FirstOrDefault(v => !string.IsNullOrEmpty(v.ExpectedInvalidMessage))?.ExpectedInvalidMessage;
            if (!string.IsNullOrEmpty(message))
            {
                values.Add(message);
            }
            return values;
        });

        var jsonOutput = JsonSerializer.Serialize(jsonReadyCases, new JsonSerializerOptions { WriteIndented = true });

        Console.WriteLine($"\n🔹 **Generated JSON Output ({methodName}):**\n");
        Console.WriteLine(jsonOutput);
        Debug.WriteLine(jsonOutput);

        try
        {
            ClipboardService.SetText(jsonOutput);
            Console.WriteLine("✅ JSON output copied to clipboard.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to copy JSON output to clipboard: {ex.Message}");
        }
    }
}