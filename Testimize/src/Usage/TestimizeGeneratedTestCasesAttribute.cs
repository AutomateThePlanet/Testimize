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
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Builders;
using NUnit.Framework.Internal;
using System.Reflection;
using Testimize.Parameters.Core;

namespace Testimize;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestimizeGeneratedTestCasesAttribute : Attribute, ITestBuilder
{
    private readonly string _configMethodName;

    public TestimizeGeneratedTestCasesAttribute(string configMethodName)
    {
        _configMethodName = configMethodName;
    }

    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
    {
        MethodInfo sourceMethod = method.TypeInfo.Type.GetMethod(_configMethodName, BindingFlags.Public | BindingFlags.Static);
        if (sourceMethod == null)
        {
            throw new InvalidOperationException($"Static method '{_configMethodName}' not found in {method.TypeInfo.Type.Name}.");
        }

        var testCases = (List<TestCase>)sourceMethod.Invoke(null, null);

        foreach (var testCase in testCases)
        {
            var parameters = new TestCaseParameters(testCase.Values.Select(v => (object)v.Value).ToArray());
            yield return new NUnitTestCaseBuilder().BuildTestMethod(method, suite, parameters);
        }
    }
}
