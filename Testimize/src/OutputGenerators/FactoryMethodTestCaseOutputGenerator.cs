// <copyright file="FactoryMethodTestCaseOutputGenerator.cs" company="Automate The Planet Ltd.">
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

public class FactoryMethodTestCaseOutputGenerator : TestCaseOutputGenerator
{
    private readonly string _modelName;
    private readonly string _methodName;

    public FactoryMethodTestCaseOutputGenerator(string modelName = "CheckoutFormModel", string methodName = "CreateTestCases")
    {
        _modelName = modelName;
        _methodName = methodName;
    }

    public override void GenerateOutput(string methodName, IEnumerable<TestCase> testCases, TestCaseCategory testCaseCategory = TestCaseCategory.All)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"\n🔹 **Generated Factory Method Output for {_modelName}:**\n");
        sb.AppendLine($"public static IEnumerable<{_modelName}> {_methodName}()");
        sb.AppendLine("{");
        sb.AppendLine($"    return new List<{_modelName}>");
        sb.AppendLine("    {");

        foreach (var testCase in FilterTestCasesByCategory(testCases, testCaseCategory))
        {
            sb.AppendLine($"        new {_modelName}");
            sb.AppendLine("        {");
            sb.AppendLine($"            FirstName = \"{testCase.Values[0].Value}\",");
            sb.AppendLine($"            LastName = \"{testCase.Values[1].Value}\",");
            sb.AppendLine($"            ZipCode = \"{testCase.Values[2].Value}\",");
            sb.AppendLine($"            Phone = \"{testCase.Values[3].Value}\",");
            sb.AppendLine($"            Email = \"{testCase.Values[4].Value}\",");
            sb.AppendLine($"            Company = \"{testCase.Values[5].Value}\",");
            sb.AppendLine($"            Address = \"{testCase.Values[6].Value}\"");

            var message = testCase.Values.FirstOrDefault(v => !string.IsNullOrEmpty(v.ExpectedInvalidMessage))?.ExpectedInvalidMessage;
            if (!string.IsNullOrEmpty(message))
            {
                sb.AppendLine($"            ,ExpectedInvalidMessage = \"{message}\"");
            }

            sb.AppendLine("        },");
        }

        sb.AppendLine("    };");
        sb.AppendLine("}");

        var output = sb.ToString();

        Console.WriteLine(output);
        Debug.WriteLine(output);

        ClipboardService.SetText(output);
        Console.WriteLine("✅ Factory method output copied to clipboard.");
    }
}


// Example generated output:
//public static IEnumerable<CheckoutFormModel> CreateTestCases()
//{
//    return new List<CheckoutFormModel>
//    {
//        new CheckoutFormModel
//        {
//            FirstName = "John",
//            LastName = "Doe",
//            ZipCode = "12345",
//            Phone = "+359888888888",
//            Email = "john.doe@example.com",
//            Company = "SomeCompany",
//            Address = "123 Main St"
//        },
//        new CheckoutFormModel
//        {
//            FirstName = "A",
//            LastName = "B",
//            ZipCode = "99999",
//            Phone = "+359777777777",
//            Email = "invalid-email@",
//            Company = "Short",
//            Address = "XYZ Street"
//        },
//        new CheckoutFormModel
//        {
//            FirstName = "LongFirstNameExceedsLimit",
//            LastName = "ValidLastName",
//            ZipCode = "00000",
//            Phone = "+359000000000",
//            Email = "test@mail.com",
//            Company = "Enterprise",
//            Address = "456 Business Ave"
//        }
//    };
//}