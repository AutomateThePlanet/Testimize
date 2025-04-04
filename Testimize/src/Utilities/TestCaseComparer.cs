﻿// <copyright file="TestCaseComparer.cs" company="Automate The Planet Ltd.">
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

namespace Testimize.Utilities;
public class TestCaseComparer : IEqualityComparer<string[]>
{
    public bool Equals(string[] x, string[] y)
    {
        return x.SequenceEqual(y);
    }

    public int GetHashCode(string[] obj)
    {
        unchecked
        {
            var hash = 17;
            foreach (var val in obj)
            {
                hash = hash * 31 + (val?.GetHashCode() ?? 0);
            }
            return hash;
        }
    }
}
