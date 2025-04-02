// <copyright file="ABCTestCaseSourceAttribute.cs" company="Automate The Planet Ltd.">
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Testimize.Parameters.Core;
using Xunit.Sdk;

namespace Testimize.Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestimizeGeneratedTestCasesAttribute : DataAttribute
{
    private readonly string _configMethodName;

    public TestimizeGeneratedTestCasesAttribute(string configMethodName)
    {
        _configMethodName = configMethodName;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        if (testMethod == null)
            throw new ArgumentNullException(nameof(testMethod));

        var declaringType = testMethod.DeclaringType;
        if (declaringType == null)
            throw new InvalidOperationException("Test method must be defined in a class.");

        var sourceMethod = declaringType.GetMethod(
            _configMethodName,
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (sourceMethod == null)
            throw new InvalidOperationException($"Static method '{_configMethodName}' not found in {declaringType.FullName}.");

        var testCases = (List<TestCase>)sourceMethod.Invoke(null, null);
        if (testCases == null)
            throw new InvalidOperationException($"Method '{_configMethodName}' returned null.");

        foreach (var testCase in testCases)
        {
            yield return testCase.Values.Select(v => (object)v.Value).ToArray();
        }
    }
}