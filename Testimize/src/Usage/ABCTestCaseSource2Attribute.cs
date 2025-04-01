using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Builders;
using NUnit.Framework.Internal;
using System.Reflection;
using Testimize.Parameters.Core;

namespace Testimize;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ABCTestCaseSource2Attribute : Attribute, ITestBuilder
{
    private readonly string _configMethodName;

    public ABCTestCaseSource2Attribute(string configMethodName)
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
