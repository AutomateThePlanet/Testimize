// // <copyright file="ParameterBuilder.cs" company="Automate The Planet Ltd.">
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

namespace Testimize.Parameters.Core;

public class ParameterBuilder<TDataParameter> where TDataParameter : IInputParameter
{
    private readonly List<TestValue> _values = new();

    public ParameterBuilder<TDataParameter> Valid(object val)
    {
        _values.Add(new TestValue(val, TestValueCategory.Valid));
        return this;
    }

    public ParameterBuilder<TDataParameter> BoundaryValid(object val)
    {
        _values.Add(new TestValue(val, TestValueCategory.BoundaryValid));
        return this;
    }

    public TestValueBuilder<TDataParameter> BoundaryInvalid(object val)
    {
        return new TestValueBuilder<TDataParameter>(this, val, TestValueCategory.BoundaryInvalid);
    }

    public TestValueBuilder<TDataParameter> Invalid(object val)
    {
        return new TestValueBuilder<TDataParameter>(this, val, TestValueCategory.Invalid);
    }

    public TDataParameter Build()
    {
        var parameter = (TDataParameter)Activator.CreateInstance(
            typeof(TDataParameter),
            true,  // preciseMode
            false, // allowValidEquivalenceClasses
            false, // allowInvalidEquivalenceClasses
            false, // includeBoundaryValues
            _values.ToArray() // preciseTestValues
        );

        return parameter;
    }

    internal void Add(TestValue value) => _values.Add(value);
}
