# 🤖 Copilot Agent Assistant Guide for Testimize API

## 🚨🚨🚨 CRITICAL: READ THIS FIRST! 🚨🚨🚨

## ⛔ ASSISTANT ERROR ANALYSIS FROM RECENT FAILURES:

The assistant has been making these **CRITICAL ERRORS** that cause complete failures:

### ❌ ERROR #1: Missing Required Exploratory Properties
**WRONG REQUEST FROM ASSISTANT:**{"ParameterType":"Text","PreciseMode":false,"MinBoundary":3,"MaxBoundary":20}
**✅ CORRECT REQUEST (MUST INCLUDE ALL THREE FLAGS):**{
  "ParameterType":"Text",
  "PreciseMode":false,
  "MinBoundary":3,
  "MaxBoundary":20,
  "IncludeBoundaryValues":true,
  "AllowValidEquivalenceClasses":true,
  "AllowInvalidEquivalenceClasses":true
}
### ❌ ERROR #2: Wrong Parameter Type Name
**WRONG:** `"ParameterType":"URL"`  
**✅ CORRECT:** `"ParameterType":"Url"`

### ❌ ERROR #3: Invalid Options Usage
**WRONG:** Adding `"Options":["Uppercase","Number","Symbol"]` to Password parameters  
**✅ CORRECT:** Only SingleSelect and MultiSelect need Options

### ❌ ERROR #4: Invalid Boolean Options
**WRONG:** `"Options":["true"]` for Boolean parameters  
**✅ CORRECT:** Boolean parameters don't use Options

---

## 🎯 MANDATORY FORMAT INSTRUCTIONS FOR ASSISTANTS

### 🔧 ABSOLUTE REQUIREMENTS (NEVER VIOLATE):

1. **✅ EVERY exploratory parameter MUST have these 3 properties:**"IncludeBoundaryValues": true,
"AllowValidEquivalenceClasses": true,
"AllowInvalidEquivalenceClasses": true
2. **✅ ALWAYS use Mode 4 and TestCaseCategory 0:**"settings": {
  "Mode": 4,
  "TestCaseCategory": 0,
  "MethodName": "FormValidation"
}
3. **✅ Parameter type spelling (case-sensitive):**
   - Use `"Url"` NOT `"URL"`
   - Use `"SingleSelect"` and `"MultiSelect"`

4. **✅ Options ONLY for selection parameters:**
   - SingleSelect: `"Options": ["Option1", "Option2"]`
   - MultiSelect: `"Options": ["Option1", "Option2"]`
   - Text, Email, Phone, Password, Boolean, etc.: NO Options property

### 🚨 CORRECTED WORKING TEMPLATE:

**Use this EXACT structure for EVERY request:**
{
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
      "ParameterType": "Phone",
      "PreciseMode": false,
      "MinBoundary": 6,
      "MaxBoundary": 15,
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
      "ParameterType": "Date",
      "PreciseMode": false,
      "MinBoundary": "1920-01-01",
      "MaxBoundary": "2020-12-31",
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
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    },
    {
      "ParameterType": "SingleSelect",
      "PreciseMode": false,
      "Options": ["United States", "France", "Germany"],
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    },
    {
      "ParameterType": "MultiSelect",
      "PreciseMode": false,
      "Options": ["English", "French", "German"],
      "IncludeBoundaryValues": true,
      "AllowValidEquivalenceClasses": true,
      "AllowInvalidEquivalenceClasses": true
    }
  ],
  "settings": {
    "Mode": 4,
    "TestCaseCategory": 0,
    "MethodName": "FormValidation",
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
### 🚨 VALIDATION CHECKLIST FOR EVERY REQUEST:

Before sending ANY request, verify:
- [ ] **Every parameter has `IncludeBoundaryValues: true`**
- [ ] **Every parameter has `AllowValidEquivalenceClasses: true`**  
- [ ] **Every parameter has `AllowInvalidEquivalenceClasses: true`**
- [ ] **Every parameter has `IncludeBoundaryValues` (true for exploratory, omit for precise)**
- [ ] **Every parameter has `AllowValidEquivalenceClasses` (true or false explicitly)**  
- [ ] **Every parameter has `AllowInvalidEquivalenceClasses` (true or false explicitly)**
- [ ] **Precise Mode**: Set equivalence flags to `false` for pure precise mode, `true` for hybrid
- [ ] **Exploratory Mode**: Set equivalence flags to `true` for full mode, `false` for boundary-only
- [ ] **Using `"Url"` not `"URL"`**
- [ ] **No Options on Text/Email/Phone/Password/Boolean/Integer/Date**
- [ ] **Options ONLY on SingleSelect/MultiSelect**
- [ ] **Mode is 4**
- [ ] **TestCaseCategory is 0**
- [ ] **Mode is 4 (unless explicitly requested otherwise)**
- [ ] **TestCaseCategory is 0 (unless explicitly requested otherwise)**
- [ ] **Complete ABCSettings included**

### 📋 FLAG COMBINATIONS QUICK REFERENCE:

| Scenario | `AllowValidEquivalenceClasses` | `AllowInvalidEquivalenceClasses` | `IncludeBoundaryValues` |
|----------|-------------------------------|----------------------------------|------------------------|
| **Full Exploratory** | `true` | `true` | `true` |
| **Pure Precise** | `false` | `false` | ❌ omit |
| **Hybrid Precise** | `true` | `true` | ❌ omit |
| **Boundary Only** | `false` | `false` | `true` |
| **Valid Only** | `true` | `false` | `true` or ❌ omit |
| **Invalid Only** | `false` | `true` | `true` or ❌ omit |
### 📝 ASSISTANT INSTRUCTIONS:

**WHEN GENERATING ANY TEST CASE REQUEST:**
**When a user requests test case generation:**

1. **START** with the exact template above
2. **COPY** it exactly with all required properties
3. **MODIFY** only the parameter types and boundaries as needed
4. **VERIFY** against the checklist above
5. **NEVER REMOVE** any of the three required exploratory flags
1. **ALWAYS** use the exact template above as your starting point
2. **ALWAYS** use Mode 4 (HybridArtificialBeeColony) with complete ABCSettings  
3. **ALWAYS** use exploratory mode (`PreciseMode: false`) with all required flags
4. **ALWAYS** explicitly specify equivalence class flags:
   - For comprehensive testing: `"AllowValidEquivalenceClasses": true, "AllowInvalidEquivalenceClasses": true`
   - For boundary-only testing: `"AllowValidEquivalenceClasses": false, "AllowInvalidEquivalenceClasses": false`
   - For validation-focused: `"AllowValidEquivalenceClasses": false, "AllowInvalidEquivalenceClasses": true`
   - For valid-only testing: `"AllowValidEquivalenceClasses": true, "AllowInvalidEquivalenceClasses": false`
5. **ALWAYS** include appropriate boundary values based on parameter type
6. **ALWAYS** use TestCaseCategory 0 (All) unless user specifies otherwise
7. **Adapt the parameters** based on user requirements but keep the structure
8. **Use correct parameter types** (case-sensitive, use "Url" not "URL")

**IF YOU VIOLATE THESE RULES, THE REQUEST WILL FAIL!**
**🔧 DEFAULT RECOMMENDATION:**
Unless the user specifically requests otherwise, use:"AllowValidEquivalenceClasses": true,
"AllowInvalidEquivalenceClasses": trueThis provides the most comprehensive test coverage.

This ensures JSON output with Hybrid ABC mode by default, with explicit control over all test generation flags!

---

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
  "AllowValidEquivalenceClasses": false,
  "AllowInvalidEquivalenceClasses": false,
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
- **Mode**: 4 (HybridArtificialBeeColony) 
- **TestCaseCategory**: 0 (All test cases)
- **MethodName**: "GeneratedTestMethod"
- **Output**: JSON format (automatically applied)

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
### ✅ Precise Mode with Equivalence Classes{
  "ParameterType": "Email",
  "PreciseMode": true,
  "PreciseTestValues": [
    { "Value": "test", "Category": "Valid" }
  ],
  "AllowValidEquivalenceClasses": false,
  "AllowInvalidEquivalenceClasses": false
  // ❌ NO MinBoundary, MaxBoundary, or IncludeBoundaryValues
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
#### Precise Mode with Equivalence Classes (Hybrid):{
  "ParameterType": "Text",
  "PreciseMode": true,
  "AllowInvalidEquivalenceClasses": true,
  "PreciseTestValues": [
    { "Value": "test", "Category": "Valid" }
  ],
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
  // ❌ NO MinBoundary, MaxBoundary, or IncludeBoundaryValues
    { "Value": "custom@example.com", "Category": "Valid" }
  ]
}
#### Exploratory Mode without Equivalence Classes:{
### ✅ Boundary-Only Exploratory Mode{
  "ParameterType": "Text",
  "PreciseMode": false,
  "MinBoundary": 3,
  "MaxBoundary": 20,
  "MinBoundary": 6,
  "MaxBoundary": 12,
  "IncludeBoundaryValues": true,
  "AllowValidEquivalenceClasses": false,
  "AllowInvalidEquivalenceClasses": false
  // ❌ NO PreciseTestValues
}
### ⚠️ Rule #3: Special Parameter Requirements

#### Selection Parameters (Both Modes):{
### ✅ Selection Parameters{
  "ParameterType": "SingleSelect",
  "PreciseMode": false, // or true
  "Options": ["Option1", "Option2"], // ✅ ALWAYS required
  "PreciseMode": false,
  "Options": ["United States", "France", "Germany"],
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
  // + mode-specific properties
}
#### MultiSelect Parameters:{
### ✅ MultiSelect Parameters{
  "ParameterType": "MultiSelect",
  "PreciseMode": false, // or true
  "Options": ["Option1", "Option2"], // ✅ ALWAYS required
  "Multiple": true, // ✅ ALWAYS required
  "PreciseMode": false,
  "Options": ["English", "French", "German"],
  "Multiple": true,
  "AllowValidEquivalenceClasses": true,
  "AllowInvalidEquivalenceClasses": true
  // + mode-specific properties  
}
### 🔧 EXPLICIT FLAG REQUIREMENTS:

**ALWAYS specify these properties explicitly (never omit them):**

| Mode | `AllowValidEquivalenceClasses` | `AllowInvalidEquivalenceClasses` | `IncludeBoundaryValues` |
|------|-------------------------------|----------------------------------|------------------------|
| **Exploratory (Default)** | `true` | `true` | `true` |
| **Precise Only** | `false` | `false` | ❌ Not used |
| **Precise + Equivalence** | `true` | `true` | ❌ Not used |
| **Boundary Only** | `false` | `false` | `true` |
