# 🤖 Copilot Agent Assistant Guide for Testimize API

## 📖 Overview

This guide has been enhanced with detailed parameter specifications and limitations based on the actual Testimize data parameter classes and settings. You can provide plain text explanations about field requirements, and the assistant will automatically construct and execute the correct MCP call.

---

## 🧠 Core Features

### 🎯 Precise Mode vs 🔍 Exploratory Mode

| Aspect | **Precise Mode** | **Exploratory Mode** |
|--------|------------------|---------------------|
| **Control Level** | Full manual control | Automatic generation |
| **Input Definition** | Explicit `PreciseTestValues` array | `MinBoundary`, `MaxBoundary`, and flags |
| **Test Values** | You provide all values | Library generates values |
| **Use Case** | Known edge cases, validation scenarios | Discovery, boundary testing, exploration |
| **Configuration** | `PreciseMode: true` + `PreciseTestValues` | `PreciseMode: false` + boundary settings |

---

## 📋 Complete Parameter Types Reference

### 🔤 Text & String Parameters

#### TextDataParameter
- **Aliases**: `"Text"`, `"TextDataParameter"`
- **Boundary Type**: `int` (character length)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Character count (e.g., 3, 50)

#### EmailDataParameter  
- **Aliases**: `"Email"`, `"EmailDataParameter"`
- **Boundary Type**: `int` (email length)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Character count (e.g., 6, 50)

#### UsernameDataParameter
- **Aliases**: `"Username"`, `"UsernameDataParameter"`
- **Boundary Type**: `int` (username length)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Character count (e.g., 3, 20)

#### PasswordDataParameter
- **Aliases**: `"Password"`, `"PasswordDataParameter"`
- **Boundary Type**: `int` (password length)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Character count (e.g., 8, 20)

#### PhoneDataParameter
- **Aliases**: `"Phone"`, `"PhoneDataParameter"`
- **Boundary Type**: `int` (phone number length)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Character count (e.g., 6, 15)

#### UrlDataParameter
- **Aliases**: `"Url"`, `"UrlDataParameter"` ⚠️ (Note: NOT "URL")
- **Boundary Type**: `int` (URL length)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Character count (e.g., 10, 100)

#### AddressDataParameter
- **Aliases**: `"Address"`, `"AddressDataParameter"`
- **Boundary Type**: `int` (address length)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Character count (e.g., 10, 200)

---

### 🔢 Numeric Parameters

#### IntegerDataParameter
- **Aliases**: `"Integer"`, `"Int"`, `"IntegerDataParameter"`
- **Boundary Type**: `int` (numeric value)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Integer values (e.g., 1, 100)

#### CurrencyDataParameter
- **Aliases**: `"Currency"`, `"CurrencyDataParameter"`
- **Boundary Type**: `decimal` (currency amount)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Decimal values (e.g., 0.01, 9999.99)

#### PercentageDataParameter
- **Aliases**: `"Percentage"`, `"PercentageDataParameter"`
- **Boundary Type**: `decimal` (percentage value)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Decimal values (e.g., 0.0, 100.0)

#### GeoCoordinateDataParameter
- **Aliases**: `"GeoCoordinate"`, `"GeoCoordinateDataParameter"`
- **Boundary Type**: `double` (coordinate value)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Double values (e.g., -180.0, 180.0)

---

### ⏰ Date & Time Parameters

#### DateTimeDataParameter
- **Aliases**: `"DateTime"`, `"DateTimeDataParameter"`
- **Boundary Type**: ISO DateTime string (`"2023-01-01T12:00:00"`)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: ISO DateTime strings

#### DateDataParameter
- **Aliases**: `"Date"`, `"DateDataParameter"`
- **Boundary Type**: ISO Date string (`"2023-01-01"`)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: ISO Date strings

#### TimeDataParameter
- **Aliases**: `"Time"`, `"TimeDataParameter"`
- **Boundary Type**: TimeSpan string (`"12:30:45"`)
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: TimeSpan strings

#### WeekDataParameter
- **Aliases**: `"Week"`, `"WeekDataParameter"`
- **Boundary Type**: ISO Date string for week boundaries
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: ISO Date strings

#### MonthDataParameter
- **Aliases**: `"Month"`, `"MonthDataParameter"`
- **Boundary Type**: ISO Date string for month boundaries
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: ISO Date strings

---

### ✅ Boolean & Selection Parameters

#### BooleanDataParameter
- **Aliases**: `"Boolean"`, `"Bool"`, `"BooleanDataParameter"`
- **Boundary Type**: None (no boundaries)
- **Has Equivalence Classes**: ✅ Yes (but limited)
- **MinBoundary/MaxBoundary**: Not used

#### SingleSelectDataParameter
- **Aliases**: `"SingleSelect"`, `"SingleSelectDataParameter"`
- **Boundary Type**: None
- **Has Equivalence Classes**: ✅ Yes
- **Special Properties**: Requires `Options` array
- **MinBoundary/MaxBoundary**: Not used

#### MultiSelectDataParameter
- **Aliases**: `"MultiSelect"`, `"MultiSelectDataParameter"`
- **Boundary Type**: None
- **Has Equivalence Classes**: ✅ Yes
- **Special Properties**: Requires `Options` array and `Multiple: true`
- **MinBoundary/MaxBoundary**: Not used

---

### 🎨 Special Parameters

#### ColorDataParameter
- **Aliases**: `"Color"`, `"ColorDataParameter"`
- **Boundary Type**: None
- **Has Equivalence Classes**: ✅ Yes
- **MinBoundary/MaxBoundary**: Not used

---

## ⚠️ Critical Limitations & Requirements

### Parameters WITHOUT Equivalence Classes
These parameters have **NO** equivalence classes defined in testimizeSettings.json and will fail if you try to use exploratory mode with equivalence classes:

❌ **Missing Equivalence Classes**: None (all parameters have equivalence classes defined)

### Parameters WITH Equivalence Classes
✅ All parameter types have equivalence classes defined in testimizeSettings.json.

**Note**: `SingleSelect` and `MultiSelect` parameters were previously missing from the settings configuration, causing dictionary key errors. This has been fixed by adding their equivalence class definitions to all testimizeSettings.json files.

### Boundary Requirements by Parameter Type

| Parameter Type | MinBoundary Type | MaxBoundary Type | Example |
|---------------|------------------|------------------|---------|
| `Text`, `Email`, `Username`, `Password`, `Phone`, `Url`, `Address` | `int` | `int` | `3`, `50` |
| `Integer` | `int` | `int` | `1`, `100` |
| `Currency`, `Percentage` | `decimal` | `decimal` | `0.01`, `9999.99` |
| `GeoCoordinate` | `double` | `double` | `-180.0`, `180.0` |
| `Date`, `DateTime`, `Week`, `Month` | `string` (ISO) | `string` (ISO) | `"2020-01-01"`, `"2030-12-31"` |
| `Time` | `string` (TimeSpan) | `string` (TimeSpan) | `"00:00:00"`, `"23:59:59"` |
| `Boolean`, `Color` | Not used | Not used | N/A |
| `SingleSelect`, `MultiSelect` | Not used | Not used | Use `Options` array |

---

## 🚀 Correct Usage Patterns

### ✅ Exploratory Mode (Recommended){
  "ParameterType": "Text",
  "PreciseMode": false,
  "MinBoundary": 6,
  "MaxBoundary": 12,
  "IncludeBoundaryValues": true,
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
}
### ✅ Precise Mode (Advanced){
  "ParameterType": "Email",
  "PreciseMode": true,
  "PreciseTestValues": [
    { "Value": "user@example.com", "Category": "Valid" },
    { "Value": "invalid-email", "Category": "Invalid", "ExpectedInvalidMessage": "Invalid email format" }
  ]
}
### ✅ Selection Parameters{
  "ParameterType": "SingleSelect",
  "PreciseMode": false,
  "Options": ["United States", "France", "Germany"],
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
}
### ✅ MultiSelect Parameters{
  "ParameterType": "MultiSelect",
  "PreciseMode": false,
  "Options": ["English", "French", "German"],
  "Multiple": true,
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
}
---

## 🧬 Default Settings

### Mode Defaults
- **Mode**: 4 (Hybrid ABC)
- **TestCaseCategory**: 2 (Validation)
- **MethodName**: "GeneratedTestMethod"

### Exploratory Mode Defaults
- **IncludeBoundaryValues**: `true`
- **AllowValidEquivalenceClasses**: `true`
- **AllowInvalidEquivalenceClasses**: `true`

### ABC Settings Defaults{
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
---

## ❌ Common Mistakes to Avoid

1. **Wrong Parameter Type**: Using `"URL"` instead of `"Url"`
2. **Missing Required Properties**: 
   - Precise mode without `PreciseTestValues`
   - Exploratory mode without boundary flags
   - Selection parameters without `Options`
3. **Wrong Boundary Types**: Using string for integer boundaries
4. **Invalid Properties**: Adding non-existent properties like `"Name"`
5. **Mixed Modes**: Using both `PreciseTestValues` and exploratory settings
6. **❗ CRITICAL**: **NEVER mix `PreciseTestValues` with `MinBoundary/MaxBoundary`** - they are mutually exclusive!

---

## 🚨 Mode Selection Rules (CRITICAL)

### ⚠️ Rule #1: Choose ONE Mode Per Parameter
- **Precise Mode**: `PreciseMode: true` + `PreciseTestValues` (NO boundaries)
- **Exploratory Mode**: `PreciseMode: false` + boundaries + flags (NO PreciseTestValues)

### ⚠️ Rule #2: Required Properties by Mode

#### Precise Mode Requirements:{
  "ParameterType": "Text",
  "PreciseMode": true,
  "PreciseTestValues": [
    { "Value": "test", "Category": "Valid" }
  ]
  // ❌ NO MinBoundary, MaxBoundary, or exploratory flags
}
#### Exploratory Mode Requirements:{
  "ParameterType": "Text", 
  "PreciseMode": false,
  "MinBoundary": 3,
  "MaxBoundary": 20,
  "IncludeBoundaryValues": true,
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
  // ❌ NO PreciseTestValues
}
### ⚠️ Rule #3: Special Parameter Requirements

#### Selection Parameters (Both Modes):{
  "ParameterType": "SingleSelect",
  "PreciseMode": false, // or true
  "Options": ["Option1", "Option2"], // ✅ ALWAYS required
  // + mode-specific properties
}
#### MultiSelect Parameters:{
  "ParameterType": "MultiSelect", 
  "PreciseMode": false, // or true
  "Options": ["Option1", "Option2"], // ✅ ALWAYS required
  "Multiple": true, // ✅ ALWAYS required
  // + mode-specific properties  
}
---

## 🎯 CORRECTED Example Templates

### ✅ Form Validation (Full Exploratory Mode){
  "parameters": [
    {
      "ParameterType": "Text",
      "PreciseMode": false,
      "MinBoundary": 3,
      "MaxBoundary": 20,
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    },
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
      "ParameterType": "Integer",
      "PreciseMode": false, 
      "MinBoundary": 18,
      "MaxBoundary": 100,
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    },
    {
      "ParameterType": "Url",
      "PreciseMode": false,
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    },
    {
      "ParameterType": "Boolean",
      "PreciseMode": false,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    },
    {
      "ParameterType": "SingleSelect",
      "PreciseMode": false,
      "Options": ["United States", "France", "Germany"],
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    }
  ],
  "settings": {
    "Mode": 4,
    "TestCaseCategory": 2,
    "MethodName": "FormValidation"
  }
}
### ✅ Form Validation (Full Precise Mode){
  "parameters": [
    {
      "ParameterType": "Text",
      "PreciseMode": true,
      "PreciseTestValues": [
        { "Value": "John Doe", "Category": "Valid" },
        { "Value": "AB", "Category": "BoundaryInvalid", "ExpectedInvalidMessage": "Too short" },
        { "Value": "", "Category": "Invalid", "ExpectedInvalidMessage": "Required" }
      ]
    },
    {
      "ParameterType": "Email",
      "PreciseMode": true, 
      "PreciseTestValues": [
        { "Value": "user@example.com", "Category": "Valid" },
        { "Value": "invalid-email", "Category": "Invalid", "ExpectedInvalidMessage": "Invalid format" }
      ]
    },
    {
      "ParameterType": "Boolean",
      "PreciseMode": true,
      "PreciseTestValues": [
        { "Value": true, "Category": "Valid" },
        { "Value": false, "Category": "Invalid", "ExpectedInvalidMessage": "Must accept terms" }
      ]
    },
    {
      "ParameterType": "SingleSelect",
      "PreciseMode": true,
      "Options": ["United States", "France", "Germany"],
      "PreciseTestValues": [
        { "Value": "United States", "Category": "Valid" },
        { "Value": null, "Category": "Invalid", "ExpectedInvalidMessage": "Required" }
      ]
    }
  ],
  "settings": {
    "Mode": 1,
    "TestCaseCategory": 2,
    "MethodName": "FormValidation"
  }
}
---

## 🔧 Quick Fix for Common Errors

### Error: "Total test cases to be generated: 0"
**Cause**: Mixed mode configuration or missing required properties
**Fix**: Choose consistent mode and include all required properties

### Error: Wrong parameter type "URL"  
**Fix**: Change to `"Url"` (case sensitive)

### Error: Empty testValues arrays
**Cause**: Using `PreciseMode: true` without `PreciseTestValues`
**Fix**: Either add `PreciseTestValues` OR switch to exploratory mode

This comprehensive guide ensures the assistant generates correct MCP calls based on the actual Testimize implementation!