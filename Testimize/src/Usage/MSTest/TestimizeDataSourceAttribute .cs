// <copyright file="TestimizeGeneratedTestCasesAttribute.cs" company="Automate The Planet Ltd.">
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Testimize.Parameters.Core;

namespace Testimize.MSTest;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestimizeGeneratedTestCasesAttribute : Attribute, ITestDataSource
{
    private readonly string _configMethodName;

    public TestimizeGeneratedTestCasesAttribute(string configMethodName)
    {
        _configMethodName = configMethodName;
    }

    public IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        var method = methodInfo.DeclaringType.GetMethod(_configMethodName, BindingFlags.Static | BindingFlags.Public);
        if (method == null)
            throw new InvalidOperationException($"Static method '{_configMethodName}' not found.");

        var testCases = (List<TestCase>)method.Invoke(null, null);
        return testCases.Select(tc => tc.Values.Select(v => (object)v.Value).ToArray());
    }

    public string GetDisplayName(MethodInfo methodInfo, object[] data) =>
        $"{methodInfo.Name}({string.Join(", ", data)})";
}