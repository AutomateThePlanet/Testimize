# ?? Copilot Agent Assistant Guide for Testimize API

## ?? Overview

This comprehensive guide explains how to use the **Testimize API** for intelligent test data generation. The API supports two primary modes:

- **?? Precise Mode**: Full control with explicit test values and expected messages
- **?? Exploratory Mode**: Automatic generation using boundaries, equivalence classes, and boundary value analysis

---

## ?? Core Concepts

### ?? Precise Mode vs ?? Exploratory Mode

| Aspect | **Precise Mode** | **Exploratory Mode** |
|--------|------------------|---------------------|
| **Control Level** | Full manual control | Automatic generation |
| **Input Definition** | Explicit `PreciseTestValues` array | `MinBoundary`, `MaxBoundary`, and flags |
| **Test Values** | You provide all values | Library generates values |
| **Use Case** | Known edge cases, validation scenarios | Discovery, boundary testing, exploration |
| **Configuration** | `PreciseMode: true` + `PreciseTestValues` | `PreciseMode: false` + boundary settings |

#### Precise Mode Example{
  "ParameterType": "Email",
  "PreciseMode": true,
  "PreciseTestValues": [
    { "Value": "valid@example.com", "Category": "Valid" },
    { "Value": "invalid-email", "Category": "Invalid", "ExpectedInvalidMessage": "Invalid email format" }
  ]
}
#### Exploratory Mode Example{
  "ParameterType": "Email",
  "PreciseMode": false,
  "MinBoundary": 6,
  "MaxBoundary": 50,
  "IncludeBoundaryValues": true,
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
}
### ??? Test Value Categories

| Category | Value | Description | When Generated |
|----------|-------|-------------|----------------|
| **Valid** | `"Valid"` | Normal, valid input values | All modes |
| **BoundaryValid** | `"BoundaryValid"` | Valid values at boundaries (min/max) | When `IncludeBoundaryValues: true` |
| **BoundaryInvalid** | `"BoundaryInvalid"` | Invalid values just outside boundaries | When `IncludeBoundaryValues: true` |
| **Invalid** | `"Invalid"` | Invalid input values with expected errors | When `AllowInvalidEquivalenceClasses: true` |

---

## ?? Complete Parameter Reference

### ?? Universal Parameter Properties

Every parameter supports these core properties:
{
  "ParameterType": "string",           // Required: Parameter type identifier
  "PreciseMode": true/false,           // Required: Mode selection
  
  // Exploratory Mode Properties
  "MinBoundary": "value",              // Min boundary (type-specific)
  "MaxBoundary": "value",              // Max boundary (type-specific)
  "IncludeBoundaryValues": true/false, // Generate boundary test values
  "AllowValidEquivalenceClasses": true/false,   // Generate valid equivalence classes
  "AllowInvalidEquivalenceClasses": true/false, // Generate invalid equivalence classes
  
  // Precise Mode Properties
  "PreciseTestValues": [               // Array of explicit test values
    {
      "Value": "actual_value",         // The test value
      "Category": "Valid|BoundaryValid|BoundaryInvalid|Invalid", // Test category
      "ExpectedInvalidMessage": "string" // Optional: Expected validation message
    }
  ],
  
  // Selection Parameters Only
  "Options": ["option1", "option2"],   // Available options for select parameters
  "Multiple": true/false               // Allow multiple selections (MultiSelect only)
}
---

## ?? Text & String Parameters

### TextDataParameter
**Aliases**: `"Text"`, `"TextDataParameter"`, `"Testimize.Parameters.TextDataParameter"`
**Boundary Type**: `int` (character length)
{
  "ParameterType": "Text",
  "PreciseMode": false,
  "MinBoundary": 3,
  "MaxBoundary": 50,
  "IncludeBoundaryValues": true,
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
}
**Exploratory Mode Generates**:
- Valid text of various lengths
- Boundary values (exactly 3 and 50 characters)
- Invalid values (2 chars, 51 chars, empty string)
- Special characters, numbers, unicode text

### EmailDataParameter
**Aliases**: `"Email"`, `"EmailDataParameter"`, `"Testimize.Parameters.EmailDataParameter"`
**Boundary Type**: `int` (email length)
{
  "ParameterType": "Email",
  "PreciseMode": true,
  "MinBoundary": 6,
  "MaxBoundary": 30,
  "PreciseTestValues": [
    { "Value": "user@example.com", "Category": "Valid" },
    { "Value": "a@b.co", "Category": "BoundaryValid" },
    { "Value": "verylongemailaddress@domain.com", "Category": "BoundaryValid" },
    { "Value": "a@b", "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Email too short" },
    { "Value": "invalid-email", "Category": "Invalid", "ExpectedInvalidMessage": "Invalid email format" },
    { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "Email is required" }
  ]
}
### UsernameDataParameter
**Aliases**: `"Username"`, `"UsernameDataParameter"`, `"Testimize.Parameters.UsernameDataParameter"`
**Boundary Type**: `int` (username length)

### PasswordDataParameter
**Aliases**: `"Password"`, `"PasswordDataParameter"`, `"Testimize.Parameters.PasswordDataParameter"`
**Boundary Type**: `int` (password length)
{
  "ParameterType": "Password",
  "PreciseMode": false,
  "MinBoundary": 8,
  "MaxBoundary": 20,
  "IncludeBoundaryValues": true,
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
}
**Exploratory Mode Generates**:
- Strong passwords with various complexity
- Boundary passwords (8 and 20 characters)
- Weak passwords (too short, no special chars)
- Common password patterns

### PhoneDataParameter
**Aliases**: `"Phone"`, `"PhoneDataParameter"`, `"Testimize.Parameters.PhoneDataParameter"`
**Boundary Type**: `int` (phone number length)

### UrlDataParameter
**Aliases**: `"Url"`, `"UrlDataParameter"`, `"Testimize.Parameters.UrlDataParameter"`
**Boundary Type**: `int` (URL length)

### AddressDataParameter
**Aliases**: `"Address"`, `"AddressDataParameter"`, `"Testimize.Parameters.AddressDataParameter"`
**Boundary Type**: `int` (address length)

---

## ?? Numeric Parameters

### IntegerDataParameter
**Aliases**: `"Integer"`, `"Int"`, `"IntegerDataParameter"`, `"Testimize.Parameters.IntegerDataParameter"`
**Boundary Type**: `int` (numeric value)
{
  "ParameterType": "Integer",
  "PreciseMode": false,
  "MinBoundary": 1,
  "MaxBoundary": 100,
  "IncludeBoundaryValues": true,
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
}
**Exploratory Mode Generates**:
- Random integers within range
- Boundary values (1, 100)
- Invalid values (0, 101, negative numbers)
- Edge cases (very large numbers, zero)

### CurrencyDataParameter
**Aliases**: `"Currency"`, `"CurrencyDataParameter"`, `"Testimize.Parameters.CurrencyDataParameter"`
**Boundary Type**: `decimal` (currency amount)
{
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
### PercentageDataParameter
**Aliases**: `"Percentage"`, `"PercentageDataParameter"`, `"Testimize.Parameters.PercentageDataParameter"`
**Boundary Type**: `decimal` (percentage value)

### GeoCoordinateDataParameter
**Aliases**: `"GeoCoordinate"`, `"GeoCoordinateDataParameter"`, `"Testimize.Parameters.GeoCoordinateDataParameter"`
**Boundary Type**: `double` (coordinate value)

---

## ? Date & Time Parameters

### DateTimeDataParameter
**Aliases**: `"DateTime"`, `"DateTimeDataParameter"`, `"Testimize.Parameters.DateTimeDataParameter"`
**Boundary Type**: ISO DateTime string (`"2023-01-01T12:00:00"`)
{
  "ParameterType": "DateTime",
  "PreciseMode": false,
  "MinBoundary": "1920-01-01T00:00:00",
  "MaxBoundary": "2030-12-31T23:59:59",
  "IncludeBoundaryValues": true,
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
}
### DateDataParameter
**Aliases**: `"Date"`, `"DateDataParameter"`, `"Testimize.Parameters.DateDataParameter"`
**Boundary Type**: ISO Date string (`"2023-01-01"`)

### TimeDataParameter
**Aliases**: `"Time"`, `"TimeDataParameter"`, `"Testimize.Parameters.TimeDataParameter"`
**Boundary Type**: TimeSpan string (`"12:30:45"`)

### WeekDataParameter
**Aliases**: `"Week"`, `"WeekDataParameter"`, `"Testimize.Parameters.WeekDataParameter"`
**Boundary Type**: ISO Date string for week boundaries

### MonthDataParameter
**Aliases**: `"Month"`, `"MonthDataParameter"`, `"Testimize.Parameters.MonthDataParameter"`
**Boundary Type**: ISO Date string for month boundaries

---

## ? Boolean & Selection Parameters

### BooleanDataParameter
**Aliases**: `"Boolean"`, `"Bool"`, `"BooleanDataParameter"`, `"Testimize.Parameters.BooleanDataParameter"`
**No Boundaries**: Boolean parameters don't use MinBoundary/MaxBoundary
{
  "ParameterType": "Boolean",
  "PreciseMode": true,
  "PreciseTestValues": [
    { "Value": true, "Category": "Valid" },
    { "Value": false, "Category": "Invalid", "ExpectedInvalidMessage": "Must accept terms" }
  ]
}
### SingleSelectDataParameter
**Aliases**: `"SingleSelect"`, `"SingleSelectDataParameter"`, `"Testimize.Parameters.SingleSelectDataParameter"`
**Special Properties**: Requires `Options` array
{
  "ParameterType": "SingleSelect",
  "PreciseMode": false,
  "Options": ["Option1", "Option2", "Option3"],
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
}
**Exploratory Mode Generates**:
- Each option from Options array
- null/empty selection (invalid)
- Invalid option values not in the list

### MultiSelectDataParameter
**Aliases**: `"MultiSelect"`, `"MultiSelectDataParameter"`, `"Testimize.Parameters.MultiSelectDataParameter"`
**Special Properties**: Requires `Options` array and `Multiple: true`
{
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
---

## ?? Special Parameters

### ColorDataParameter
**Aliases**: `"Color"`, `"ColorDataParameter"`, `"Testimize.Parameters.ColorDataParameter"`
**No Boundaries**: Generates color codes and invalid color formats
{
  "ParameterType": "Color",
  "PreciseMode": false,
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
}
**Exploratory Mode Generates**:
- Valid hex colors (#FF0000, #00FF00)
- Named colors (red, blue, green)
- Invalid color formats (not-a-color, #GGGGGG)

---

## ?? Generation Settings

### Test Generation Modes"settings": {
  "Mode": 1,                    // Generation algorithm
  "TestCaseCategory": 0,        // Filter by category
  "MethodName": "TestMethod",   // Output method name
  "ABCSettings": { }            // Advanced AI settings (Mode 4 only)
}
#### Available Modes
| Mode | Value | Algorithm | Best For | Parameter Count |
|------|-------|-----------|----------|-----------------|
| **Combinatorial** | `0` | All combinations | Critical systems | 2-4 parameters |
| **Pairwise** | `1` | All pairs | Balanced testing | 5-10 parameters |
| **CombinatorialOptimized** | `2` | AI-optimized combinatorial | Quality over quantity | 2-6 parameters |
| **PairwiseOptimized** | `3` | AI-optimized pairwise | Enhanced coverage | 5-15 parameters |
| **HybridArtificialBeeColony** | `4` | AI-driven selection | Complex scenarios | 5-20+ parameters |

#### Test Case Categories
| Category | Value | Includes |
|----------|-------|----------|
| **All** | `0` | All generated test cases |
| **Valid** | `1` | Only Valid + BoundaryValid |
| **Validation** | `2` | Only Invalid + BoundaryInvalid |
| **Invalid** | `3` | Only Invalid |

---

## ?? Advanced ABC Settings (Mode 4)

When using `Mode: 4`, you can fine-tune the AI algorithm:
"ABCSettings": {
  "TotalPopulationGenerations": 50,      // Evolution cycles (10-200)
  "MutationRate": 0.4,                   // Mutation rate (0.1-0.9)
  "FinalPopulationSelectionRatio": 0.5,  // Final selection ratio (0.1-1.0)
  "EliteSelectionRatio": 0.3,            // Elite preservation (0.1-0.9)
  "OnlookerSelectionRatio": 0.1,         // Onlooker phase ratio (0.05-0.5)
  "ScoutSelectionRatio": 0.3,            // Scout phase ratio (0.1-0.5)
  "EnableOnlookerSelection": true,       // Enable onlooker bees
  "EnableScoutPhase": true,              // Enable scout bees
  "EnforceMutationUniqueness": true,     // Ensure unique mutations
  "StagnationThresholdPercentage": 0.75, // Stagnation detection (0.5-1.0)
  "CoolingRate": 0.95,                   // Simulated annealing (0.8-0.99)
  "AllowMultipleInvalidInputs": false,   // Multiple invalid inputs per case
  "Seed": 42                             // Reproducible results
}
---

## ?? Complete Working Examples

### 1. Precise Mode - Form Validation{
  "parameters": [
    {
      "ParameterType": "Text",
      "PreciseMode": true,
      "PreciseTestValues": [
        { "Value": "John Doe", "Category": "Valid" },
        { "Value": "A", "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Name too short" },
        { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "Name is required" }
      ]
    },
    {
      "ParameterType": "Email",
      "PreciseMode": true,
      "PreciseTestValues": [
        { "Value": "user@example.com", "Category": "Valid" },
        { "Value": "invalid-email", "Category": "Invalid", "ExpectedInvalidMessage": "Invalid email" }
      ]
    }
  ],
  "settings": {
    "Mode": 1,
    "TestCaseCategory": 2,
    "MethodName": "ValidateFormInput"
  }
}
### 2. Exploratory Mode - API Testing{
  "parameters": [
    {
      "ParameterType": "Integer",
      "PreciseMode": false,
      "MinBoundary": 1,
      "MaxBoundary": 100,
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    },
    {
      "ParameterType": "Currency",
      "PreciseMode": false,
      "MinBoundary": 0.01,
      "MaxBoundary": 9999.99,
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    }
  ],
  "settings": {
    "Mode": 4,
    "TestCaseCategory": 0,
    "MethodName": "TestProductAPI",
    "ABCSettings": {
      "TotalPopulationGenerations": 50,
      "MutationRate": 0.4,
      "AllowMultipleInvalidInputs": false,
      "Seed": 42
    }
  }
}
### 3. Hybrid Mode - Complex Scenario{
  "parameters": [
    {
      "ParameterType": "Text",
      "PreciseMode": true,
      "PreciseTestValues": [
        { "Value": "Known Edge Case", "Category": "Valid" },
        { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "Required field" }
      ]
    },
    {
      "ParameterType": "Integer",
      "PreciseMode": false,
      "MinBoundary": 18,
      "MaxBoundary": 100,
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    },
    {
      "ParameterType": "MultiSelect",
      "PreciseMode": false,
      "Multiple": true,
      "Options": ["Option1", "Option2", "Option3"],
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    }
  ],
  "settings": {
    "Mode": 4,
    "TestCaseCategory": 0,
    "MethodName": "ComplexBusinessLogic"
  }
}
---

## ?? MCP Call Examples

### Basic MCP Request{
  "name": "generate_test_cases",
  "arguments": {
    "parameters": [
      {
        "ParameterType": "Text",
        "PreciseMode": true,
        "PreciseTestValues": [
          { "Value": "Test Value", "Category": "Valid" },
          { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "Required" }
        ]
      }
    ],
    "settings": {
      "Mode": 1,
      "TestCaseCategory": 0,
      "MethodName": "TestMethod"
    }
  }
}
### Advanced MCP Request with ABC{
  "name": "generate_test_cases",
  "arguments": {
    "parameters": [
      {
        "ParameterType": "Email",
        "PreciseMode": false,
        "MinBoundary": 6,
        "MaxBoundary": 50,
        "IncludeBoundaryValues": true,
        "AllowValidEquivalenceClasses": true,
        "AllowInvalidEquivalenceClasses": true
      },
      {
        "ParameterType": "Password",
        "PreciseMode": false,
        "MinBoundary": 8,
        "MaxBoundary": 20,
        "IncludeBoundaryValues": true,
        "AllowValidEquivalenceClasses": true,
        "AllowInvalidEquivalenceClasses": true
      }
    ],
    "settings": {
      "Mode": 4,
      "TestCaseCategory": 2,
      "MethodName": "RegisterUser",
      "ABCSettings": {
        "TotalPopulationGenerations": 50,
        "MutationRate": 0.4,
        "AllowMultipleInvalidInputs": false,
        "Seed": 42
      }
    }
  }
}
---

## ?? Quick Reference

### Parameter Type Quick Lookup
- **Text/String**: Text, Email, Username, Password, Phone, Url, Address
- **Numeric**: Integer, Currency, Percentage, GeoCoordinate  
- **Date/Time**: DateTime, Date, Time, Week, Month
- **Selection**: Boolean, SingleSelect, MultiSelect
- **Special**: Color

### Mode Selection Guide
- **Mode 0-1**: Small-medium parameter sets, fast generation
- **Mode 2-3**: Medium parameter sets, optimized quality
- **Mode 4**: Large parameter sets, complex scenarios, AI optimization

### Common Patterns
- **Validation Testing**: Precise mode with Invalid categories
- **Boundary Testing**: Exploratory mode with IncludeBoundaryValues
- **Exploratory Testing**: Exploratory mode with ABC algorithm
- **Regression Testing**: Precise mode with fixed Seed

---

## ?? Important Notes

1. **Parameter Type Flexibility**: Supports full names, class names, and short aliases (case-insensitive)
2. **Boundary Types**: Match the parameter type (int for length, decimal for currency, DateTime for dates)
3. **Selection Parameters**: Always include `Options` array for SingleSelect/MultiSelect
4. **ABC Settings**: Only apply when using Mode 4 (HybridArtificialBeeColony)
5. **Expected Messages**: Use `ExpectedInvalidMessage` for validation testing scenarios
6. **Reproducibility**: Set `Seed` for consistent results across runs

This guide provides everything needed to effectively use the Testimize API for comprehensive test data generation!