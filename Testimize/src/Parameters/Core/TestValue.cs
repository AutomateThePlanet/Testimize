// // <copyright file="TestValue.cs" company="Automate The Planet Ltd.">
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

namespace Testimize.Parameters.Core;

// Represents a single test value with a category (Boundary, Normal, Invalid)
public class TestValue
{
    public TestValue(object value, TestValueCategory category)
    {
        Value = value;
        Category = category;
    }

    public TestValue(object value, TestValueCategory category, string expectedInvalidMessage)
        : this(value, category)
    {
        ExpectedInvalidMessage = expectedInvalidMessage;
    }

    public object Value { get; }
    public string ExpectedInvalidMessage { get; }
    public TestValueCategory Category { get; }

    public override bool Equals(object obj)
    {
        if (obj is not TestValue other)
        {
            return false;
        }

        if (Value is null || other.Value is null)
        {
            return false;
        }

        return Value.Equals(other.Value);
    }

    public override int GetHashCode()
    {
        var hash = Value?.GetHashCode() ?? 17;
        return hash;
    }
}
