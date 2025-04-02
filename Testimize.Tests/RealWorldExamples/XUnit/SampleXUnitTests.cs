// <copyright file="SampleXUnitTests.cs" company="Automate The Planet Ltd.">
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

using System.Collections.Generic;
using Testimize.OutputGenerators;
using Testimize.Parameters.Core;
using Testimize.Usage;
using Testimize.Xunit;
using System.Diagnostics;

namespace Testimize.Tests.RealWorldExamples.Xunit;

public class SampleXUnitTests
{
    public static List<TestCase> ConfigureEngine() =>
        TestimizeEngine.Configure(
            parameters => parameters
                .AddText(6, 12)
                .AddEmail(5, 10)
                .AddPhone(6, 8)
                .AddText(4, 10)
            , settings =>
            {
                settings.Mode = TestGenerationMode.HybridArtificialBeeColony;
                settings.TestCaseCategory = TestCaseCategory.Validation;
            }
        ).Generate();

    
    //[Category(Categories.CI)]
    [Theory]
    [TestimizeGeneratedTestCases(nameof(ConfigureEngine))]
    [Ignore("There is a bug in the xUnit engine, it should be fixed in future releases.")]
    public void ValidateInputs(string textValue, string email, string phone, string anotherText)
    {
        Debug.WriteLine($"Running test with: {textValue}, {email}, {phone}, {anotherText}");

        Assert.That(textValue, Is.Not.Null);
        Assert.That(email, Is.Not.Null);
        Assert.That(phone, Is.Not.Null);
        Assert.That(anotherText, Is.Not.Null);
    }
}
