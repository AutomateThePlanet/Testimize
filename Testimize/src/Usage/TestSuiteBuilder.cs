// <copyright file="TestSuiteBuilder.cs" company="Automate The Planet Ltd.">
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

using Testimize.Contracts;
using Testimize.OutputGenerators;
using Testimize.Parameters.Core;
using Testimize.TestCaseGenerators;

namespace Testimize.Usage;
public class TestSuiteBuilder
{
    private readonly List<IInputParameter> _parameters;
    private readonly PreciseTestEngineSettings _settings;

    internal TestSuiteBuilder(List<IInputParameter> parameters, PreciseTestEngineSettings config)
    {
        _parameters = parameters;
        _settings = config;
    }

    public List<TestCase> Generate()
    {
        return _settings.Mode switch
        {
            TestGenerationMode.HybridArtificialBeeColony => GenerateUsingABC(_settings.MethodName, _settings.TestCaseCategory),
            TestGenerationMode.Pairwise => GenerateUsingPairwise(_settings.MethodName, _settings.TestCaseCategory),
            _ => GenerateUsingPairwise(_settings.MethodName, _settings.TestCaseCategory)
        };
    }

    private List<TestCase> GenerateUsingPairwise(string methodName, TestCaseCategory category)
    {
        var testCases = PairwiseTestCaseGenerator.GenerateTestCases(_parameters).ToList();

        _settings.OutputGenerator?.GenerateOutput(methodName, testCases, category);

        return testCases;
    }

    private List<TestCase> GenerateUsingABC(string methodName, TestCaseCategory category)
    {
        var abcConfig = _settings.ABCSettings ?? new ABCGenerationSettings();
        var abc = new HybridArtificialBeeColonyTestCaseGenerator(abcConfig);
        return abc.GenerateTestCases(methodName, _parameters, category).ToList();
    }
}