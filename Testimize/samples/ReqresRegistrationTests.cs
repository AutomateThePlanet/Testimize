// <copyright file="ReqresRegistrationTests.cs" company="Automate The Planet Ltd.">
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

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Testimize.Contracts;
using Testimize.NUnit;
using Testimize.Parameters.Core;
using Testimize.OutputGenerators;
using Testimize.Usage;

namespace Testimize.Tests.RealWorld;

[TestFixture]
public class ReqresRegistrationTests
{
    public static List<TestCase> ConfigureEngine() =>
        TestimizeEngine.Configure(
            parameters => parameters
                .AddEmail(10, 20)
                .AddPassword(8, 16),
            settings =>
            {
                settings.Mode = TestGenerationMode.HybridArtificialBeeColony;
                settings.TestCaseCategory = TestCaseCategory.Validation;
                settings.ABCSettings = new ABCGenerationSettings
                {
                    TotalPopulationGenerations = 50,
                    MutationRate = 0.4,
                    FinalPopulationSelectionRatio = 0.5,
                    EliteSelectionRatio = 0.3,
                    OnlookerSelectionRatio = 0.1,
                    ScoutSelectionRatio = 0.3,
                    EnableOnlookerSelection = true,
                    EnableScoutPhase = true,
                    EnforceMutationUniqueness = true,
                    StagnationThresholdPercentage = 0.75,
                    CoolingRate = 0.95,
                    AllowMultipleInvalidInputs = false,
                    OutputGenerator = new NUnitTestCaseAttributeOutputGenerator()
                };
            }).Generate();

    [Test]
    [TestimizeGeneratedTestCases(nameof(ConfigureEngine))]
    public void RegisterUser_WithGeneratedEmailAndPassword(string email, string password)
    {
        // Uncomment this section to execute actual HTTP registration call to Reqres.in

        /*
        var client = new RestClient("https://reqres.in");
        var request = new RestRequest("/api/register", Method.Post);
        request.AddJsonBody(new
        {
            email,
            password
        });

        var response = client.Execute(request);

        Console.WriteLine($"→ Attempted registration with Email: {email}, Password: {password}");
        Console.WriteLine($"← Response: {response.StatusCode}, Content: {response.Content}");

        // Expect 200 for known valid email or 400 otherwise (per Reqres.in behavior)
        Assert.That((int)response.StatusCode, Is.EqualTo(200).Or.EqualTo(400));
        */

        // Dry-run simulation fallback
        Assert.Pass($"Simulated registration attempt: Email='{email}', Password='{password}'");
    }
}