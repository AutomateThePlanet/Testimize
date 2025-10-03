// <copyright file="UniversalDataParameter.cs" company="Automate The Planet Ltd.">
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

using System.Text.Json;
using Testimize.Contracts;

namespace Testimize.Parameters.Core;

public class UniversalDataParameter : IInputParameter
{
    public string ParameterType { get; set; } = string.Empty;
    public bool PreciseMode { get; set; }
    public bool? IncludeBoundaryValues { get; set; }
    public bool? AllowValidEquivalenceClasses { get; set; }
    public bool? AllowInvalidEquivalenceClasses { get; set; }
    public TestValue[] PreciseTestValues { get; set; } = Array.Empty<TestValue>();
    
    // Boundary values as objects to handle different types
    public object? MinBoundary { get; set; }
    public object? MaxBoundary { get; set; }
    
    // Additional properties for specific parameter types
    public string[]? Options { get; set; } // For select parameters
    public bool Multiple { get; set; } // For multi-select parameters
    
    // IInputParameter implementation
    public string ParameteryType => ParameterType;
    public List<TestValue> TestValues { get; set; } = new List<TestValue>();

    // Helper methods for type-safe boundary access
    public T? GetMinBoundary<T>() where T : struct
    {
        if (MinBoundary == null) return null;
        
        if (MinBoundary is JsonElement jsonElement)
        {
            try
            {
                return JsonSerializer.Deserialize<T?>(jsonElement.GetRawText());
            }
            catch
            {
                return null;
            }
        }
        
        if (MinBoundary is T directValue)
        {
            return directValue;
        }

        // Try to convert if it's a compatible type
        try
        {
            return (T)Convert.ChangeType(MinBoundary, typeof(T));
        }
        catch
        {
            return null;
        }
    }
    
    public T? GetMaxBoundary<T>() where T : struct
    {
        if (MaxBoundary == null) return null;
        
        if (MaxBoundary is JsonElement jsonElement)
        {
            try
            {
                return JsonSerializer.Deserialize<T?>(jsonElement.GetRawText());
            }
            catch
            {
                return null;
            }
        }
        
        if (MaxBoundary is T directValue)
        {
            return directValue;
        }

        // Try to convert if it's a compatible type
        try
        {
            return (T)Convert.ChangeType(MaxBoundary, typeof(T));
        }
        catch
        {
            return null;
        }
    }

    public string[]? GetOptions()
    {
        return Options;
    }

    public bool GetMultiple()
    {
        return Multiple;
    }
}