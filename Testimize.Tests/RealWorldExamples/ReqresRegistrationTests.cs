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
using RestSharp;
using Testimize.OutputGenerators;
using Testimize.Parameters.Core;
using Testimize.Usage;

namespace Testimize.Tests.RealWorldExamples;

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
            }).Generate();

    [Test]
    [TestimizeGeneratedTestCases(nameof(ConfigureEngine))]
    public void RegisterUser_WithGeneratedEmailAndPassword(string email, string password)
    {
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

        // Reqres returns:
        // - 200 when email/password are both valid and known to the system
        // - 400 for missing or invalid data
        Assert.That((int)response.StatusCode, Is.EqualTo(200).Or.EqualTo(400));
    }
}
