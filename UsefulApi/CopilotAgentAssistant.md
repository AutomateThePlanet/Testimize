# Copilot Agent Assistant Guide for Testimize API

## Overview

This guide explains how to use the `generate` tool in the Testimize API. It includes instructions for constructing `DataParameter` types based on the `ParameteryType` property and provides examples of serialized parameters for precise mode.

---

## Key Concepts

### Precise Mode Parameters
Precise mode allows for fine-grained control over test case generation. Below is an example of serialized parameters for precise mode:

```json
{
  "Mode": 4,
  "TestCaseCategory": 2,
  "MethodName": "TestMethodName",
  "OutputGenerator": {},
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
    "Seed": 42,
    "TestCaseGenerator": {},
    "TestCaseEvaluator": {},
    "OutputGenerator": {}
  }
}
```

### DataParameter Construction
The `DataParameter` types must be constructed dynamically based on the `ParameteryType` property. Below is a list of supported parameter types and their corresponding classes:

| Parameter Type | Class Name |
|----------------|------------|
| `Testimize.Parameters.TextDataParameter` | `TextDataParameter` |
| `Testimize.Parameters.IntegerDataParameter` | `IntegerDataParameter` |
| `Testimize.Parameters.UsernameDataParameter` | `UsernameDataParameter` |
| `Testimize.Parameters.EmailDataParameter` | `EmailDataParameter` |
| `Testimize.Parameters.BooleanDataParameter` | `BooleanDataParameter` |
| `Testimize.Parameters.DateTimeDataParameter` | `DateTimeDataParameter` |
| `Testimize.Parameters.CurrencyDataParameter` | `CurrencyDataParameter` |

---

## Example Serialized Parameters

### TextDataParameter
```json
{
  "Type": "Testimize.Parameters.TextDataParameter",
  "Values": [
    {
      "Value": "Hello World",
      "ExpectedInvalidMessage": null,
      "Category": 2
    },
    {
      "Value": "Sample Input",
      "ExpectedInvalidMessage": null,
      "Category": 2
    },
    {
      "Value": "",
      "ExpectedInvalidMessage": null,
      "Category": 3
    }
  ]
}
```

### IntegerDataParameter
```json
{
  "Type": "Testimize.Parameters.IntegerDataParameter",
  "Values": [
    {
      "Value": "0",
      "ExpectedInvalidMessage": null,
      "Category": 2
    },
    {
      "Value": "42",
      "ExpectedInvalidMessage": null,
      "Category": 2
    },
    {
      "Value": "-100",
      "ExpectedInvalidMessage": null,
      "Category": 2
    }
  ]
}
```

### CurrencyDataParameter
```json
{
  "Type": "Testimize.Parameters.CurrencyDataParameter",
  "Values": [
    {
      "Value": "100.00",
      "ExpectedInvalidMessage": null,
      "Category": 2
    },
    {
      "Value": "999.99",
      "ExpectedInvalidMessage": null,
      "Category": 2
    },
    {
      "Value": "-50.00",
      "ExpectedInvalidMessage": null,
      "Category": 3
    }
  ]
}
```

---

## MCP Call Example

To call the `generate` tool via MCP, construct the request as follows:

### Request Format
```json
{
  "name": "generate",
  "arguments": {
    "parameters": [
      {
        "Type": "Testimize.Parameters.TextDataParameter",
        "Values": [
          {
            "Value": "Hello World",
            "ExpectedInvalidMessage": null,
            "Category": 2
          }
        ]
      },
      {
        "Type": "Testimize.Parameters.IntegerDataParameter",
        "Values": [
          {
            "Value": "42",
            "ExpectedInvalidMessage": null,
            "Category": 2
          }
        ]
      }
    ],
    "settings": {
      "Mode": 4,
      "TestCaseCategory": 2,
      "MethodName": "TestMethodName",
      "OutputGenerator": {}
    }
  }
}
```

### Response Format
The response will include the generated test cases:
```json
{
  "testCases": [
    {
      "inputs": [
        {
          "Type": "Testimize.Parameters.TextDataParameter",
          "Value": "Hello World"
        },
        {
          "Type": "Testimize.Parameters.IntegerDataParameter",
          "Value": "42"
        }
      ],
      "expectedOutput": "Success"
    }
  ]
}
```

---

## API Endpoint Example

To call the `generate` tool via the HTTP API, use the following endpoint:

### Endpoint
`POST /generate-test-cases`

### Request Body
```json
{
  "parameters": [
    {
      "Type": "Testimize.Parameters.TextDataParameter",
      "Values": [
        {
          "Value": "Hello World",
          "ExpectedInvalidMessage": null,
          "Category": 2
        }
      ]
    },
    {
      "Type": "Testimize.Parameters.IntegerDataParameter",
      "Values": [
        {
          "Value": "42",
          "ExpectedInvalidMessage": null,
          "Category": 2
        }
      ]
    }
  ],
  "settings": {
    "Mode": 4,
    "TestCaseCategory": 2,
    "MethodName": "TestMethodName",
    "OutputGenerator": {}
  }
}
```

### Response Body
```json
{
  "testCases": [
    {
      "inputs": [
        {
          "Type": "Testimize.Parameters.TextDataParameter",
          "Value": "Hello World"
        },
        {
          "Type": "Testimize.Parameters.IntegerDataParameter",
          "Value": "42"
        }
      ],
      "expectedOutput": "Success"
    }
  ]
}
```

---

## Notes

- Ensure the `ParameteryType` property matches the class name when constructing `DataParameter` types.
- Use the serialized examples as templates for constructing requests.
- Validate the response to ensure test cases are generated correctly.

---