// <copyright file="TestimizeEngine.cs" company="Automate The Planet Ltd.">
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

namespace Testimize.Usage;
public partial class TestimizeEngine
{
    private readonly List<IInputParameter> _parameters;
    private readonly PreciseTestEngineSettings _config;

    private TestimizeEngine(List<IInputParameter> parameters, PreciseTestEngineSettings config)
    {
        _parameters = parameters;
        _config = config;
    }

    public static TestSuiteBuilder Configure(
        Action<TestInputSetBuilder> parametersConfig,
        Action<PreciseTestEngineSettings> configOverrides = null)
    {
        var composer = new TestInputSetBuilder();
        parametersConfig(composer);

        var config = new PreciseTestEngineSettings();
        configOverrides?.Invoke(config);

        return new TestSuiteBuilder(composer.Build(), config);
    }
}