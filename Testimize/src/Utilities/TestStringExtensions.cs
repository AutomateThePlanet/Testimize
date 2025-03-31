// <copyright file="TestStringExtensions.cs" company="Automate The Planet Ltd.">
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

public static class TestStringExtensions
{
    /// <summary>
    /// Trims the string to the specified maximum length if it exceeds it.
    /// </summary>
    public static string EnsureMaxLength(this string input, int maxLength)
    {
        if (input == null) return string.Empty;
        return input.Length <= maxLength ? input : input.Substring(0, maxLength);
    }

    /// <summary>
    /// Ensures the string is at least the specified minimum length by padding with a given character.
    /// </summary>
    public static string EnsureMinLength(this string input, int minLength, char paddingChar = 'A')
    {
        if (input == null) input = string.Empty;
        return input.Length >= minLength
            ? input
            : input + new string(paddingChar, minLength - input.Length);
    }

    /// <summary>
    /// Ensures the string is within a min and max range. Trims or pads accordingly.
    /// </summary>
    public static string EnsureRangeLength(this string input, int minLength, int maxLength, char paddingChar = 'A')
    {
        return input
            .EnsureMaxLength(maxLength)
            .EnsureMinLength(minLength, paddingChar);
    }
}

