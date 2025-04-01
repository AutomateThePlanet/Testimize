// <copyright file="PreciseModeCountriesGraphQLTests.cs" company="Automate The Planet Ltd.">
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

using Testimize.OutputGenerators;
using Testimize.Usage;
using NUnit.Framework;

namespace Testimize.Tests.RealWorld;

[TestFixture]
public class PreciseModeCountriesGraphQLTests
{
    [Test]
    public void GenerateTests() =>
        TestimizeEngine
        .Configure(
            parameters => parameters
                .AddSingleSelect(s => s
                    .Valid("US")
                    .Valid("BG")
                    .Valid("FR")
                    .Invalid("XX").WithExpectedMessage("Country code is invalid")
                    .Invalid("U1").WithExpectedMessage("Country code must contain only letteABCTestCaseGeneratorTestsrs")
                    .Invalid("").WithExpectedMessage("Country code is required"))
                .AddSingleSelect(s => s
                    .Valid("en")
                    .Valid("fr")
                    .Valid("de")
                    .Invalid("zz").WithExpectedMessage("Language code not supported")
                    .Invalid("123").WithExpectedMessage("Language code must be alphabetic"))
                .AddSingleSelect(s => s
                    .Valid("EU")
                    .Valid("AF")
                    .Valid("AS")
                    .Invalid("999").WithExpectedMessage("Continent code cannot be numeric")
                    .Invalid("X").WithExpectedMessage("Continent code too short")
                    .Invalid("").WithExpectedMessage("Continent code is required")),
            settings =>
            {
                settings.Mode = TestGenerationMode.HybridArtificialBeeColony;

                settings.ABCSettings = new ABCGenerationSettings
                {
                    TotalPopulationGenerations = 20,
                    MutationRate = 0.3,
                    FinalPopulationSelectionRatio = 0.5,
                    EliteSelectionRatio = 0.5,
                    OnlookerSelectionRatio = 0.1,
                    ScoutSelectionRatio = 0.3,
                    EnableOnlookerSelection = true,
                    EnableScoutPhase = false,
                    EnforceMutationUniqueness = true,
                    StagnationThresholdPercentage = 0.75,
                    CoolingRate = 0.95,
                    AllowMultipleInvalidInputs = false,
                    OutputGenerator = new NUnitTestCaseAttributeOutputGenerator()
                };
            })
        .Generate();
}
