// // <copyright file="ExploratoryModeCountriesGraphQLTests.cs" company="Automate The Planet Ltd.">
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

using Testimize.Contracts;
using Testimize.OutputGenerators;
using Testimize.Parameters.Core;
using NUnit.Framework;

namespace Testimize.Tests.RealWorld;

[TestFixture]
public class ExploratoryModeCountriesGraphQLTests
{
    public static List<IInputParameter> ABCGeneratedTestParameters() =>
    TestimizeInputBuilder
        .Start()
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
            .Invalid("").WithoutMessage())
        .Build();

    [Test]
    [ABCTestCaseSource(nameof(ABCGeneratedTestParameters), TestCaseCategory.Validation)]
    public void QueryCountry_WithLanguageAndContinentFilters_ShouldReturn200(
     string countryCode, string languageCode, string continentCode)
    {
        //var client = new RestClient("https://countries.trevorblades.com/");
        //var request = new RestRequest("", Method.Post);
        //request.AddHeader("Content-Type", "application/json");

        //var graphql = new
        //{
        //    query = @"
        //            query FilteredQuery($code: ID!, $lang: String!, $cont: String!) {
        //                country(code: $code) {
        //                    name
        //                    capital
        //                    languages { code name }
        //                    continent { code name }
        //                }
        //            }",
        //    variables = new
        //    {
        //        code = countryCode.ToUpperInvariant(),
        //        lang = languageCode.ToLowerInvariant(),
        //        cont = continentCode.ToUpperInvariant()
        //    }
        //};

        //request.AddJsonBody(graphql);
        //var response = client.Execute(request);

        //Console.WriteLine($"→ Querying {countryCode}, Language: {languageCode}, Continent: {continentCode}");
        //Console.WriteLine($"← Response: {response.StatusCode}, Body: {response.Content}");

        //Assert.That((int)response.StatusCode, Is.EqualTo(200));
    }
}
