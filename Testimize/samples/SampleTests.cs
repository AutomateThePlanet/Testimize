// // <copyright file="SampleTests.cs" company="Automate The Planet Ltd.">
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

using Testimize.Parameters;
using System.Diagnostics;
using Testimize.Contracts;
using Testimize.OutputGenerators;
using NUnit.Framework;

namespace Testimize.Tests.RealWorldExamples;

[TestFixture]
public class SampleTests
{
    // ✅ This method provides the test parameters.
    public static List<IInputParameter> ABCGeneratedTestParameters()
    {
        return new List<IInputParameter>
        {
            new TextDataParameter(minBoundary: 6, maxBoundary: 12),
            new EmailDataParameter(minBoundary: 5, maxBoundary: 10),
            new PhoneDataParameter(minBoundary: 6, maxBoundary: 8),
            new TextDataParameter(minBoundary: 4, maxBoundary: 10),
        };
    }

    // ✅ Test method using ABC-driven test cases
    [Test, ABCTestCaseSource(nameof(ABCGeneratedTestParameters), TestCaseCategory.Validation)]
    public void TestABCGeneration(string textValue, string email, string phone, string anotherText)
    {
        Debug.WriteLine($"Running test with: {textValue}, {email}, {phone}, {anotherText}");

        Assert.That(textValue, Is.Not.Null);
        Assert.That(email, Is.Not.Null);
        Assert.That(phone, Is.Not.Null);
        Assert.That(anotherText, Is.Not.Null);

    }
}
