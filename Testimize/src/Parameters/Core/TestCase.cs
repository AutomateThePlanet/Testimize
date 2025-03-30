// // <copyright file="TestCase.cs" company="Automate The Planet Ltd.">
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

public class TestCase : ICloneable
{
    public List<TestValue> Values { get; set; } = new List<TestValue>();
    public double Score { get; set; }

    public object Clone()
    {
        return new TestCase
        {
            Values = Values.Select(v => new TestValue(v.Value, v.Category, v.ExpectedInvalidMessage)).ToList(),
            Score = Score
        };
    }

    public override bool Equals(object obj)
    {
        if (obj is not TestCase other) return false;
        return Values.SequenceEqual(other.Values);
    }


    public override int GetHashCode()
    {
        var hash = 17;
        var index = 1;
        foreach (var value in Values) // Ensure order consistency
        {
            hash = hash * index++ + (value.Value?.GetHashCode() ?? 0); // Use value string hash
        }
        return hash;
    }
}
