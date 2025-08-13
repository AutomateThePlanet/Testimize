# ?? Complete Testimize API Test Guide

## ?? Overview

Testimize is a smart test data generation engine that supports multiple generation strategies:
- **Precise Mode**: Full control with explicit test values
- **Exploratory Mode**: Automatic generation using boundaries and equivalence classes
- **Hybrid Modes**: Combining both approaches for optimal coverage

## ?? Quick Start Examples

### Simple Precise Mode Test{
  "parameters": [
    {
      "ParameterType": "Text",
      "PreciseMode": true,
      "PreciseTestValues": [
        { "Value": "Valid input", "Category": "Valid" },
        { "Value": "Edge case", "Category": "BoundaryValid" },
        { "Value": "", "Category": "Invalid" }
      ]
    }
  ],
  "settings": {
    "Mode": 1,
    "TestCaseCategory": 0,
    "MethodName": "SimpleTest"
  }
}
### Exploratory Mode with Boundaries{
  "parameters": [
    {
      "ParameterType": "Text",
      "PreciseMode": false,
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true,
      "MinBoundary": 3,
      "MaxBoundary": 50
    }
  ],
  "settings": {
    "Mode": 1,
    "TestCaseCategory": 0,
    "MethodName": "ExploratoryTest"
  }
}
## ?? All Available Parameter Types

### ?? Text & String Parameters

#### Text Parameter{
  "ParameterType": "Text", // or "TextDataParameter" or "Testimize.Parameters.TextDataParameter"
  "PreciseMode": true,
  "MinBoundary": 3,
  "MaxBoundary": 50,
  "IncludeBoundaryValues": true,
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true,
  "PreciseTestValues": [
    { "Value": "Valid text", "Category": "Valid" },
    { "Value": "Min", "Category": "BoundaryValid" },
    { "Value": "Maximum length text up to boundary", "Category": "BoundaryValid" },
    { "Value": "Ab", "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Text too short" },
    { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "Text is required" }
  ]
}
#### Email Parameter{
  "ParameterType": "Email",
  "PreciseMode": true,
  "MinBoundary": 6,
  "MaxBoundary": 30,
  "PreciseTestValues": [
    { "Value": "valid@example.com", "Category": "Valid" },
    { "Value": "a@b.co", "Category": "BoundaryValid" },
    { "Value": "verylongemailaddress@domain.org", "Category": "BoundaryValid" },
    { "Value": "a@b.c", "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Email too short" },
    { "Value": "invalid-email", "Category": "Invalid", "ExpectedInvalidMessage": "Invalid email format" },
    { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "Email is required" }
  ]
}
#### Username Parameter{
  "ParameterType": "Username",
  "PreciseMode": true,
  "MinBoundary": 3,
  "MaxBoundary": 20,
  "PreciseTestValues": [
    { "Value": "validuser", "Category": "Valid" },
    { "Value": "usr", "Category": "BoundaryValid" },
    { "Value": "maxlengthusername20", "Category": "BoundaryValid" },
    { "Value": "ab", "Category": "BoundaryInvalid" },
    { "Value": "", "Category": "Invalid" }
  ]
}
#### Password Parameter{
  "ParameterType": "Password",
  "PreciseMode": true,
  "MinBoundary": 8,
  "MaxBoundary": 20,
  "PreciseTestValues": [
    { "Value": "SecurePass1!", "Category": "Valid" },
    { "Value": "Aa1@abcd", "Category": "BoundaryValid" },
    { "Value": "StrongP@ssw0rd123456", "Category": "BoundaryValid" },
    { "Value": "weak", "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Password too short" },
    { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "Password is required" }
  ]
}
#### Phone Parameter{
  "ParameterType": "Phone",
  "PreciseMode": true,
  "MinBoundary": 7,
  "MaxBoundary": 15,
  "PreciseTestValues": [
    { "Value": "+1234567890", "Category": "Valid" },
    { "Value": "+123456", "Category": "BoundaryValid" },
    { "Value": "+123456789012345", "Category": "BoundaryValid" },
    { "Value": "+12345", "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Phone too short" },
    { "Value": "invalid-phone", "Category": "Invalid", "ExpectedInvalidMessage": "Invalid phone format" }
  ]
}
#### URL Parameter{
  "ParameterType": "Url",
  "PreciseMode": true,
  "MinBoundary": 10,
  "MaxBoundary": 100,
  "PreciseTestValues": [
    { "Value": "https://example.com", "Category": "Valid" },
    { "Value": "http://a.co", "Category": "BoundaryValid" },
    { "Value": "https://very-long-domain-name.com/with/path/and/query", "Category": "BoundaryValid" },
    { "Value": "invalid.url", "Category": "Invalid", "ExpectedInvalidMessage": "Invalid URL format" },
    { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "URL is required" }
  ]
}
#### Address Parameter{
  "ParameterType": "Address",
  "PreciseMode": true,
  "MinBoundary": 10,
  "MaxBoundary": 100,
  "PreciseTestValues": [
    { "Value": "123 Main St, City, State", "Category": "Valid" },
    { "Value": "Short Addr", "Category": "BoundaryValid" },
    { "Value": "Very long address with detailed street information and apartment number", "Category": "BoundaryValid" },
    { "Value": "Short", "Category": "BoundaryInvalid" },
    { "Value": "", "Category": "Invalid" }
  ]
}
### ?? Numeric Parameters

#### Integer Parameter{
  "ParameterType": "Integer", // or "Int"
  "PreciseMode": true,
  "MinBoundary": 1,
  "MaxBoundary": 100,
  "PreciseTestValues": [
    { "Value": 25, "Category": "Valid" },
    { "Value": 1, "Category": "BoundaryValid" },
    { "Value": 100, "Category": "BoundaryValid" },
    { "Value": 0, "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Value too low" },
    { "Value": 101, "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Value too high" },
    { "Value": -5, "Category": "Invalid", "ExpectedInvalidMessage": "Negative values not allowed" }
  ]
}
#### Currency Parameter{
  "ParameterType": "Currency",
  "PreciseMode": true,
  "MinBoundary": 0.01,
  "MaxBoundary": 9999.99,
  "PreciseTestValues": [
    { "Value": 100.50, "Category": "Valid" },
    { "Value": 0.01, "Category": "BoundaryValid" },
    { "Value": 9999.99, "Category": "BoundaryValid" },
    { "Value": 0.00, "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Amount too low" },
    { "Value": 10000.00, "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Amount too high" },
    { "Value": -5.00, "Category": "Invalid", "ExpectedInvalidMessage": "Negative amounts not allowed" }
  ]
}
#### Percentage Parameter{
  "ParameterType": "Percentage",
  "PreciseMode": true,
  "MinBoundary": 0.0,
  "MaxBoundary": 100.0,
  "PreciseTestValues": [
    { "Value": 50.5, "Category": "Valid" },
    { "Value": 0.0, "Category": "BoundaryValid" },
    { "Value": 100.0, "Category": "BoundaryValid" },
    { "Value": -0.1, "Category": "BoundaryInvalid" },
    { "Value": 100.1, "Category": "BoundaryInvalid" },
    { "Value": -10.0, "Category": "Invalid" }
  ]
}
#### GeoCoordinate Parameter{
  "ParameterType": "GeoCoordinate",
  "PreciseMode": true,
  "MinBoundary": -180.0,
  "MaxBoundary": 180.0,
  "PreciseTestValues": [
    { "Value": 40.7128, "Category": "Valid" },
    { "Value": -180.0, "Category": "BoundaryValid" },
    { "Value": 180.0, "Category": "BoundaryValid" },
    { "Value": -180.1, "Category": "BoundaryInvalid" },
    { "Value": 180.1, "Category": "BoundaryInvalid" }
  ]
}
### ? Date & Time Parameters

#### DateTime Parameter{
  "ParameterType": "DateTime",
  "PreciseMode": true,
  "MinBoundary": "1920-01-01T00:00:00",
  "MaxBoundary": "2030-12-31T23:59:59",
  "PreciseTestValues": [
    { "Value": "1990-01-01T12:00:00", "Category": "Valid" },
    { "Value": "1920-01-01T00:00:00", "Category": "BoundaryValid" },
    { "Value": "2030-12-31T23:59:59", "Category": "BoundaryValid" },
    { "Value": "1919-12-31T23:59:59", "Category": "BoundaryInvalid" },
    { "Value": "2031-01-01T00:00:00", "Category": "BoundaryInvalid" }
  ]
}
#### Date Parameter{
  "ParameterType": "Date",
  "PreciseMode": true,
  "MinBoundary": "1920-01-01",
  "MaxBoundary": "2030-12-31",
  "PreciseTestValues": [
    { "Value": "1990-01-01", "Category": "Valid" },
    { "Value": "1920-01-01", "Category": "BoundaryValid" },
    { "Value": "2030-12-31", "Category": "BoundaryValid" },
    { "Value": "1919-12-31", "Category": "BoundaryInvalid" },
    { "Value": "2031-01-01", "Category": "BoundaryInvalid" }
  ]
}
#### Time Parameter{
  "ParameterType": "Time",
  "PreciseMode": true,
  "MinBoundary": "00:00:00",
  "MaxBoundary": "23:59:59",
  "PreciseTestValues": [
    { "Value": "12:30:45", "Category": "Valid" },
    { "Value": "00:00:00", "Category": "BoundaryValid" },
    { "Value": "23:59:59", "Category": "BoundaryValid" },
    { "Value": "24:00:00", "Category": "BoundaryInvalid" }
  ]
}
#### Week Parameter{
  "ParameterType": "Week",
  "PreciseMode": true,
  "MinBoundary": "2020-01-01",
  "MaxBoundary": "2030-12-31",
  "PreciseTestValues": [
    { "Value": "2025-W15", "Category": "Valid" },
    { "Value": "2020-W01", "Category": "BoundaryValid" },
    { "Value": "2030-W52", "Category": "BoundaryValid" }
  ]
}
#### Month Parameter{
  "ParameterType": "Month",
  "PreciseMode": true,
  "MinBoundary": "2020-01-01",
  "MaxBoundary": "2030-12-31",
  "PreciseTestValues": [
    { "Value": "2025-06", "Category": "Valid" },
    { "Value": "2020-01", "Category": "BoundaryValid" },
    { "Value": "2030-12", "Category": "BoundaryValid" }
  ]
}
### ? Boolean & Selection Parameters

#### Boolean Parameter{
  "ParameterType": "Boolean", // or "Bool"
  "PreciseMode": true,
  "PreciseTestValues": [
    { "Value": true, "Category": "Valid" },
    { "Value": false, "Category": "Valid" }
  ]
}
#### SingleSelect Parameter{
  "ParameterType": "SingleSelect",
  "PreciseMode": true,
  "Options": ["Option1", "Option2", "Option3"],
  "PreciseTestValues": [
    { "Value": "Option1", "Category": "Valid" },
    { "Value": "Option2", "Category": "Valid" },
    { "Value": "Option3", "Category": "Valid" },
    { "Value": null, "Category": "Invalid", "ExpectedInvalidMessage": "Selection is required" }
  ]
}
#### MultiSelect Parameter{
  "ParameterType": "MultiSelect",
  "PreciseMode": true,
  "Multiple": true,
  "Options": ["English", "French", "German", "Spanish"],
  "PreciseTestValues": [
    { "Value": ["English"], "Category": "Valid" },
    { "Value": ["English", "French"], "Category": "Valid" },
    { "Value": ["English", "French", "German", "Spanish"], "Category": "BoundaryValid" },
    { "Value": [], "Category": "Invalid", "ExpectedInvalidMessage": "At least one selection required" },
    { "Value": null, "Category": "Invalid", "ExpectedInvalidMessage": "Selection is required" }
  ]
}
### ?? Special Parameters

#### Color Parameter{
  "ParameterType": "Color",
  "PreciseMode": true,
  "PreciseTestValues": [
    { "Value": "#FF0000", "Category": "Valid" },
    { "Value": "#00FF00", "Category": "Valid" },
    { "Value": "#0000FF", "Category": "Valid" },
    { "Value": "invalid-color", "Category": "Invalid", "ExpectedInvalidMessage": "Invalid color format" }
  ]
}
## ?? Test Generation Settings

### Test Generation Modes
"settings": {
  "Mode": 1, // Choose from available modes below
  "TestCaseCategory": 0, // Choose category filter
  "MethodName": "YourTestMethodName"
}
#### Available Modes
| Mode | Value | Description | Best For |
|------|-------|-------------|----------|
| **Combinatorial** | `0` | All possible combinations | Exhaustive testing (small parameter sets) |
| **Pairwise** | `1` | All pairs of parameter values | Balanced coverage vs. size |
| **CombinatorialOptimized** | `2` | Optimized combinatorial using ABC | Quality over quantity combinatorial |
| **PairwiseOptimized** | `3` | Optimized pairwise using ABC | Enhanced pairwise with optimization |
| **HybridArtificialBeeColony** | `4` | AI-driven test case selection | Complex scenarios, exploratory testing |

#### Test Case Categories
| Category | Value | Description |
|----------|-------|-------------|
| **All** | `0` | Include all test cases |
| **Valid** | `1` | Only valid test cases |
| **Validation** | `2` | Only validation/invalid test cases |
| **Invalid** | `3` | Only invalid test cases |

### Advanced ABC Settings

For `Mode: 4` (HybridArtificialBeeColony), you can include advanced configuration:
"settings": {
  "Mode": 4,
  "TestCaseCategory": 2,
  "MethodName": "AdvancedABCTest",
  "ABCSettings": {
    "TotalPopulationGenerations": 50,
    "MutationRate": 0.4,
    "FinalPopulationSelectionRatio": 0.5,
    "EliteSelectionRatio": 0.3,
    "OnlookerSelectionRatio": 0.1,
    "ScoutSelectionRatio": 0.3,
    "EnableOnlookerSelection": true,
    "EnableScoutPhase": true,
    "EnforceMutationUniqueness": true,
    "StagnationThresholdPercentage": 0.75,
    "CoolingRate": 0.95,
    "AllowMultipleInvalidInputs": false,
    "Seed": 42
  }
}
#### ABC Settings Explained
| Setting | Range | Description |
|---------|-------|-------------|
| **TotalPopulationGenerations** | 10-200 | Number of evolution cycles |
| **MutationRate** | 0.1-0.9 | Rate of test case mutations |
| **FinalPopulationSelectionRatio** | 0.1-1.0 | Percentage of final population to select |
| **EliteSelectionRatio** | 0.1-0.9 | Percentage of elite individuals |
| **OnlookerSelectionRatio** | 0.05-0.5 | Percentage for onlooker phase |
| **ScoutSelectionRatio** | 0.1-0.5 | Percentage for scout phase |
| **EnableOnlookerSelection** | true/false | Enable onlooker bee phase |
| **EnableScoutPhase** | true/false | Enable scout bee phase |
| **EnforceMutationUniqueness** | true/false | Ensure unique mutations |
| **StagnationThresholdPercentage** | 0.5-1.0 | Threshold for stagnation detection |
| **CoolingRate** | 0.8-0.99 | Cooling rate for simulated annealing |
| **AllowMultipleInvalidInputs** | true/false | Allow multiple invalid inputs per test case |
| **Seed** | any integer | Random seed for reproducible results |

## ?? Real-World Complete Examples

### Form Validation Testing{
  "parameters": [
    {
      "ParameterType": "Text",
      "PreciseMode": true,
      "MinBoundary": 3,
      "MaxBoundary": 20,
      "PreciseTestValues": [
        { "Value": "John Doe", "Category": "Valid" },
        { "Value": "Ann", "Category": "BoundaryValid" },
        { "Value": "VeryLongNameThatExceeds", "Category": "BoundaryValid" },
        { "Value": "Jo", "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Name too short" },
        { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "Name is required" }
      ]
    },
    {
      "ParameterType": "Email",
      "PreciseMode": true,
      "MinBoundary": 6,
      "MaxBoundary": 50,
      "PreciseTestValues": [
        { "Value": "user@example.com", "Category": "Valid" },
        { "Value": "a@b.co", "Category": "BoundaryValid" },
        { "Value": "invalid-email", "Category": "Invalid", "ExpectedInvalidMessage": "Invalid email" },
        { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "Email required" }
      ]
    },
    {
      "ParameterType": "Integer",
      "PreciseMode": true,
      "MinBoundary": 18,
      "MaxBoundary": 100,
      "PreciseTestValues": [
        { "Value": 25, "Category": "Valid" },
        { "Value": 18, "Category": "BoundaryValid" },
        { "Value": 100, "Category": "BoundaryValid" },
        { "Value": 17, "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Must be 18 or older" },
        { "Value": 101, "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Age too high" }
      ]
    },
    {
      "ParameterType": "Boolean",
      "PreciseMode": true,
      "PreciseTestValues": [
        { "Value": true, "Category": "Valid" },
        { "Value": false, "Category": "Invalid", "ExpectedInvalidMessage": "Must accept terms" }
      ]
    }
  ],
  "settings": {
    "Mode": 4,
    "TestCaseCategory": 2,
    "MethodName": "FormValidationTest",
    "ABCSettings": {
      "TotalPopulationGenerations": 50,
      "MutationRate": 0.4,
      "FinalPopulationSelectionRatio": 0.5,
      "EliteSelectionRatio": 0.3,
      "OnlookerSelectionRatio": 0.1,
      "ScoutSelectionRatio": 0.3,
      "EnableOnlookerSelection": true,
      "EnableScoutPhase": true,
      "EnforceMutationUniqueness": true,
      "StagnationThresholdPercentage": 0.75,
      "CoolingRate": 0.95,
      "AllowMultipleInvalidInputs": false,
      "Seed": 42
    }
  }
}
### API Testing - Registration Endpoint{
  "parameters": [
    {
      "ParameterType": "Email",
      "PreciseMode": false,
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true,
      "MinBoundary": 10,
      "MaxBoundary": 30
    },
    {
      "ParameterType": "Password",
      "PreciseMode": false,
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true,
      "MinBoundary": 8,
      "MaxBoundary": 20
    }
  ],
  "settings": {
    "Mode": 1,
    "TestCaseCategory": 0,
    "MethodName": "RegisterUserTest"
  }
}
### E-commerce Product Testing{
  "parameters": [
    {
      "ParameterType": "Text",
      "PreciseMode": true,
      "PreciseTestValues": [
        { "Value": "Laptop Computer", "Category": "Valid" },
        { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "Product name required" }
      ]
    },
    {
      "ParameterType": "Currency",
      "PreciseMode": true,
      "MinBoundary": 0.01,
      "MaxBoundary": 9999.99,
      "PreciseTestValues": [
        { "Value": 999.99, "Category": "Valid" },
        { "Value": 0.01, "Category": "BoundaryValid" },
        { "Value": 9999.99, "Category": "BoundaryValid" },
        { "Value": 0.00, "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Price must be positive" },
        { "Value": -10.00, "Category": "Invalid", "ExpectedInvalidMessage": "Negative prices not allowed" }
      ]
    },
    {
      "ParameterType": "MultiSelect",
      "PreciseMode": true,
      "Multiple": true,
      "Options": ["Electronics", "Computers", "Gaming", "Office"],
      "PreciseTestValues": [
        { "Value": ["Electronics", "Computers"], "Category": "Valid" },
        { "Value": ["Gaming"], "Category": "Valid" },
        { "Value": [], "Category": "Invalid", "ExpectedInvalidMessage": "Select at least one category" }
      ]
    }
  ],
  "settings": {
    "Mode": 3,
    "TestCaseCategory": 0,
    "MethodName": "ProductCreationTest"
  }
}
## ??? Mode Selection Guide

### When to Use Each Mode

**Combinatorial (0)**: 
- Small parameter sets (?4 parameters)
- Complete coverage required
- High-risk scenarios

**Pairwise (1)**:
- Medium parameter sets (5-10 parameters)
- Good coverage vs. test count balance
- Most common choice

**CombinatorialOptimized (2)**:
- Small-medium parameter sets
- Quality over quantity
- When you want fewer but better combinatorial tests

**PairwiseOptimized (3)**:
- Medium-large parameter sets
- Enhanced pairwise with AI optimization
- Complex business logic

**HybridArtificialBeeColony (4)**:
- Large parameter sets (10+ parameters)
- Complex validation scenarios
- Exploratory testing
- When you want the AI to find optimal test combinations

### Performance Comparison

| Mode | Parameters | Typical Test Count | Generation Time | Coverage |
|------|------------|-------------------|-----------------|----------|
| Combinatorial | 2-4 | 100-1000s | Fast | 100% |
| Pairwise | 5-10 | 10-50 | Fast | ~90% |
| CombinatorialOptimized | 2-6 | 20-100 | Medium | 95%+ |
| PairwiseOptimized | 5-15 | 15-30 | Medium | 95%+ |
| HybridArtificialBeeColony | 5-20+ | 10-50 | Slow | 85-95% |

## ?? Testing Instructions

1. **Start the API**: `dotnet run --project UsefulApi`
2. **Choose your approach**:
   - **Precise Mode**: Full control with explicit test values
   - **Exploratory Mode**: Automatic generation with boundaries
   - **Hybrid Mode**: Mix both approaches
3. **Select generation mode** based on your parameter count and requirements
4. **Test the endpoint**: POST to `http://localhost:5000/generate-test-cases`
5. **Review results**: The API returns structured test cases ready for your test framework

## ?? Tips for Best Results

1. **Start Simple**: Begin with Pairwise mode for most scenarios
2. **Use Precise Mode**: When you have specific edge cases to test
3. **Use Exploratory Mode**: For discovering unexpected combinations
4. **Combine Approaches**: Mix precise values with boundary testing
5. **Tune ABC Settings**: For complex scenarios, adjust ABC parameters
6. **Set Expectations**: Use `ExpectedInvalidMessage` for validation testing
7. **Choose Categories**: Filter results by Valid/Invalid based on your test goals

This guide covers all available features of the Testimize API. Choose the combination that best fits your testing scenario!