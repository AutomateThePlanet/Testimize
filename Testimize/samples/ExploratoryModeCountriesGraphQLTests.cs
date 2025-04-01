// <copyright file="ExploratoryModeCountriesGraphQLTests.cs" company="Automate The Planet Ltd.">
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
using Testimize.OutputGenerators;
using Testimize.Parameters.Core;
using Testimize.Usage;

namespace Testimize.Tests.RealWorld;

[TestFixture]
public class ExploratoryModeCountriesGraphQLTests
{
    public static List<TestCase> ConfigureEngine() =>
        TestimizeEngine.Configure(
            parameters => parameters
                .AddSingleSelect(s => s
                    .Valid("US")
                    .Valid("BG")
                    .Valid("FR")
                    .Invalid("XX").WithoutMessage()
                    .Invalid("U1").WithoutMessage()
                    .Invalid("").WithoutMessage())
                .AddSingleSelect(s => s
                    .Valid("en")
                    .Valid("fr")
                    .Valid("de")
                    .Invalid("zz").WithoutMessage()
                    .Invalid("123").WithoutMessage())
                .AddSingleSelect(s => s
                    .Valid("EU")
                    .Valid("AF")
                    .Valid("AS")
                    .Invalid("999").WithoutMessage()
                    .Invalid("X").WithoutMessage()
                    .Invalid("").WithoutMessage()),
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
    public void QueryCountry_WithLanguageAndContinentFilters_ShouldReturn200(
        string countryCode, string languageCode, string continentCode)
    {
        // Uncomment this block to run live GraphQL query to https://countries.trevorblades.com

        /*
        var client = new RestClient("https://countries.trevorblades.com/");
        var request = new RestRequest("", Method.Post);
        request.AddHeader("Content-Type", "application/json");

        var graphql = new
        {
            query = @"
                query FilteredQuery($code: ID!, $lang: String!, $cont: String!) {
                    country(code: $code) {
                        name
                        capital
                        languages { code name }
                        continent { code name }
                    }
                }",
            variables = new
            {
                code = countryCode.ToUpperInvariant(),
                lang = languageCode.ToLowerInvariant(),
                cont = continentCode.ToUpperInvariant()
            }
        };

        request.AddJsonBody(graphql);
        var response = client.Execute(request);

        Console.WriteLine($"→ Querying {countryCode}, Language: {languageCode}, Continent: {continentCode}");
        Console.WriteLine($"← Response: {response.StatusCode}, Body: {response.Content}");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        */

        // Placeholder assertion for when API call is disabled
        //Assert.Pass($"Simulated query with: {countryCode}, {languageCode}, {continentCode}");
    }
}
