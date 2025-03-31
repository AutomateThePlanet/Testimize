// <copyright file="Settings.cs" company="Automate The Planet Ltd.">
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

using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Testimize;

public sealed class Settings
{
    private static IConfigurationRoot _root;

    static Settings()
    {
        _root = InitializeConfiguration();
    }

    public static TSection GetSection<TSection>()
      where TSection : class, new()
    {
        var sectionName = MakeFirstLetterToLower(typeof(TSection).Name);
        return _root.GetSection(sectionName).Get<TSection>();
    }

    private static string MakeFirstLetterToLower(string text)
    {
        return char.ToLower(text[0]) + text.Substring(1);
    }

    private static IConfigurationRoot InitializeConfiguration()
    {
        var builder = new ConfigurationBuilder();
        var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    var filesInExecutionDir = Directory.GetFiles(assemblyFolder);
        var settingsFile = filesInExecutionDir.FirstOrDefault(x => x.EndsWith("testimizeSettings.json"));
        if (settingsFile != null)
        {
            builder.AddJsonFile(settingsFile, optional: true, reloadOnChange: true);
        }

        return builder.Build();
    }
}