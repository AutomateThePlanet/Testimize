// <copyright file="JsonPlaceholderPostTests.cs" company="Automate The Planet Ltd.">
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
using Testimize.Parameters.Core;
using Testimize.OutputGenerators;
using Testimize.Usage;

namespace Testimize.Tests.RealWorld;

[TestFixture]
public class JsonPlaceholderPostTests
{
    public static List<TestCase> ConfigureEngine() =>
        TestimizeEngine.Configure(
            parameters => parameters
                .AddText(5, 100)                         // title
                .AddText(20, 500)                        // body
                .AddInteger(1, 100)                      // userId
                .AddUsername(6, 15)                      // authorUsername
                .AddEmail(10, 30)                        // authorEmail
                .AddBoolean()                            // isPublished
                .AddDateTime(
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow)                     // publishDate
            ,
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
    [ABCTestCaseSource2(nameof(ConfigureEngine))]
    public void CreateFullPost_WithGeneratedMetadata_ShouldSucceed(
        string title,
        string body,
        string userId,
        string authorUsername,
        string authorEmail,
        string isPublished,
        string publishDate)
    {
        // Uncomment to execute live API request to JSONPlaceholder
        /*
        var client = new RestClient("https://jsonplaceholder.typicode.com");
        var request = new RestRequest("/posts", Method.Post);
        request.AddJsonBody(new
        {
            title,
            body,
            userId = int.Parse(userId),
            metadata = new
            {
                username = authorUsername,
                email = authorEmail,
                isPublished = bool.Parse(isPublished),
                publishDate = DateTime.Parse(publishDate)
            }
        });

        var response = client.Execute(request);

        Console.WriteLine($"→ Posting data: {title} by {authorUsername} on {publishDate}");
        Console.WriteLine($"← Response: {response.StatusCode}, Body: {response.Content}");

        Assert.That((int)response.StatusCode, Is.EqualTo(201));
        */

        // Simulated assertion for dry-run or offline mode
        Assert.Pass($"Simulated post: '{title}' by '{authorUsername}' on {publishDate}");
    }
}
