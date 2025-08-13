# ?? MCP Test Cases for Generate Test Cases Tool

## Fixed MCP Schema Validation Issues ?

The MCP tool `generate_test_cases` now has proper schema validation with detailed `items` definitions for all array properties. The validation error "tool parameters array type must have items" has been resolved.

## Test Case 1: Basic Precise Mode ?
```json
{
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
```

## Test Case 2: Exploratory Mode ?
```json
{
  "name": "generate_test_cases",
  "arguments": {
    "parameters": [
      {
        "ParameterType": "Integer",
        "PreciseMode": false,
        "MinBoundary": 1,
        "MaxBoundary": 100,
        "IncludeBoundaryValues": true,
        "AllowValidEquivalenceClasses": true,
        "AllowInvalidEquivalenceClasses": true
      }
    ],
    "settings": {
      "Mode": 1,
      "TestCaseCategory": 0,
      "MethodName": "ExploratoryTest"
    }
  }
}
```

## Test Case 3: Multiple Parameters with ABC ?
```json
{
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
```

## Test Case 4: MultiSelect Parameter ?
```json
{
  "name": "generate_test_cases",
  "arguments": {
    "parameters": [
      {
        "ParameterType": "MultiSelect",
        "PreciseMode": true,
        "Multiple": true,
        "Options": ["English", "French", "German", "Spanish"],
        "PreciseTestValues": [
          { "Value": ["English"], "Category": "Valid" },
          { "Value": ["English", "French"], "Category": "Valid" },
          { "Value": [], "Category": "Invalid", "ExpectedInvalidMessage": "At least one selection required" }
        ]
      }
    ],
    "settings": {
      "Mode": 1,
      "TestCaseCategory": 0,
      "MethodName": "LanguageSelection"
    }
  }
}
```

## Test Case 5: Form Validation Complete ?
```json
{
  "name": "generate_test_cases",
  "arguments": {
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
      "Mode": 1,
      "TestCaseCategory": 2,
      "MethodName": "ValidateFormInput"
    }
  }
}
```

## Test Case 6: Currency and Numbers ?
```json
{
  "name": "generate_test_cases",
  "arguments": {
    "parameters": [
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
          { "Value": -5.00, "Category": "Invalid", "ExpectedInvalidMessage": "Negative amounts not allowed" }
        ]
      }
    ],
    "settings": {
      "Mode": 1,
      "TestCaseCategory": 0,
      "MethodName": "CurrencyTest"
    }
  }
}
```

## How to Use These Test Cases

### Option 1: VS Code GitHub Copilot Chat
1. **Copy any of the above JSON examples**
2. **In VS Code, open GitHub Copilot Chat**
3. **Ask**: "Use the MCP tool `generate_test_cases` with this JSON:" and paste the example
4. **The assistant should format the request properly and call the MCP tool**

### Option 2: Direct Testing
1. **Start the API**: `dotnet run --project UsefulApi --mcp`
2. **Use the PowerShell script**: `.\Test\test-useful-api-mcp.ps1`
3. **Or test via HTTP**: POST to `http://localhost:5000/generate-test-cases`

## Fixed Issues ?

- ? **Array Items Schema**: Added proper `items` definition for the `parameters` array
- ? **Parameter Schema**: Defined all properties for parameter objects including boundaries, test values, options
- ? **Settings Schema**: Defined complete settings structure including ABCSettings with all properties
- ? **Nested Arrays**: Fixed `PreciseTestValues` and `Options` array schemas with proper `items`
- ? **Validation**: MCP tool now passes VS Code validation without "must have items" error
- ? **Type Definitions**: Added proper type annotations (string, boolean, integer, number, object, array)
- ? **Required Fields**: Specified required properties for validation
- ? **Descriptions**: Added helpful descriptions for all properties

## Validation Success

The MCP tool `generate_test_cases` should now:
- ? Pass VS Code MCP validation
- ? Be discoverable in the GitHub Copilot Chat
- ? Accept properly formatted JSON requests
- ? Generate test cases using the Testimize engine
- ? Return structured test case results

## Quick Test Commands

```bash
# Test the API directly
curl -X POST http://localhost:5000/generate-test-cases \
  -H "Content-Type: application/json" \
  -d '{"parameters":[{"ParameterType":"Text","PreciseMode":true,"PreciseTestValues":[{"Value":"Test","Category":"Valid"}]}],"settings":{"Mode":1,"TestCaseCategory":0,"MethodName":"Test"}}'

# Run MCP server
dotnet run --project UsefulApi --mcp

# Test MCP functionality
.\Test\test-useful-api-mcp.ps1
```

The MCP validation error should now be completely resolved! ??