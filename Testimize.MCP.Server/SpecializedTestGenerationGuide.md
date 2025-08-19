# ?? Specialized Test Generation Tools Guide

## ?? Overview

The Testimize API now provides **three specialized MCP tools** for different testing scenarios:

1. **?? `generate_hybrid_test_cases`** - Advanced optimization for maximum fault detection
2. **? `generate_pairwise_test_cases`** - Fast, efficient testing for quick coverage
3. **?? `configure_testimize_settings`** - Customize default behavior for both tools

---

## ?? Tool #1: `generate_hybrid_test_cases`

### ?? **When to Use**
- **Maximum fault detection** needed
- **Form validation** testing
- **API endpoint** comprehensive testing
- **Critical system** components
- When you have **time for thorough testing**

### ? **Automatic Settings Applied**
- **Mode**: HybridArtificialBeeColony (most effective algorithm)
- **TestCaseCategory**: All (comprehensive testing)
- **ABCSettings**: Optimized defaults (customizable via `configure_testimize_settings`)

### ?? **Usage Format**
```json
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
      "ParameterType": "SingleSelect",
      "PreciseMode": true,
      "AllowValidEquivalenceClasses": false,
      "AllowInvalidEquivalenceClasses": false,
      "PreciseTestValues": [
        { "Value": "United States", "Category": "Valid" },
        { "Value": "France", "Category": "Valid" },
        { "Value": "Germany", "Category": "Valid" }
      ]
    }
  ],
  "methodName": "FormValidationTest"  // Optional
}
```

---

## ? Tool #2: `generate_pairwise_test_cases`

### ?? **When to Use**
- **Fast exploration** of input combinations
- **CI/CD pipelines** (quick feedback)
- **Smoke testing**
- **Initial coverage** before deeper testing
- **Performance-sensitive** environments

### ? **Automatic Settings Applied**
- **Mode**: Pairwise (fast and efficient)
- **TestCaseCategory**: All (comprehensive testing)
- **Minimal test suite** covering all parameter interactions

### ?? **Usage Format**
```json
{
  "parameters": [
    // Same parameter format as hybrid tool
  ],
  "methodName": "SmokeTest"  // Optional
}
```

---

## ?? Tool #3: `configure_testimize_settings`

### ?? **Purpose**
- **Customize default behavior** for both generation tools
- **Fine-tune ABC algorithm** parameters
- **Set default test categories** and method names
- **Persist settings** until server restart

### ?? **Configuration Options**

#### ??? **Plain Text Instructions (NEW!)**
You can now use natural language to describe what you want to change:

```json
{
  "instruction": "Set mutation rate to 0.5"
}
```

**Supported Plain Text Patterns:**
- `"Set mutation rate to 0.4"` ? Updates MutationRate
- `"Change method name to UserRegistrationTest"` ? Updates MethodName  
- `"Set total generations to 100"` ? Updates TotalPopulationGenerations
- `"Enable scout phase"` / `"Disable scout phase"` ? Updates EnableScoutPhase
- `"Set seed to 12345"` ? Updates Seed for reproducible results
- `"Set category to all"` / `"Set category to valid only"` ? Updates TestCaseCategory

#### ?? **Individual Property Updates**
Update just one setting without affecting others:

```json
{
  "TotalPopulationGenerations": 100
}
```

#### ?? **Complete Configuration**
```json
{
  "TestCaseCategory": 0,  // 0=All, 1=Valid only, 2=Validation only
  "MethodName": "MyDefaultTest",
  "ABCSettings": {
    "TotalPopulationGenerations": 100,  // More generations = better optimization
    "MutationRate": 0.4,                // Higher = more exploration
    "FinalPopulationSelectionRatio": 0.3, // Lower = more selective
    "EliteSelectionRatio": 0.2,
    "OnlookerSelectionRatio": 0.1,
    "ScoutSelectionRatio": 0.3,
    "EnableOnlookerSelection": true,
    "EnableScoutPhase": true,
    "EnforceMutationUniqueness": true,
    "StagnationThresholdPercentage": 0.75,
    "CoolingRate": 0.95,
    "AllowMultipleInvalidInputs": false,
    "Seed": 12345  // For reproducible results
  }
}
```

---

## ?? Tool #4: `get_testimize_settings` (NEW!)

### ?? **Purpose**
- **View current configuration** state
- **Understand what each setting does**
- **Check values before making changes**

### ?? **Usage**
```json
{}  // No parameters needed
```

**Returns:**
- Current TestCaseCategory setting with explanation
- Current default MethodName
- Complete ABCSettings configuration with descriptions
- Detailed explanations of what each setting controls

---

## ?? **Auto-Correction Features**

All tools automatically fix common mistakes:

### ? **Common Errors Detected & Fixed**
1. **Wrong PreciseMode**: 
   - Text/Email/etc. with `PreciseMode: true` ? Auto-corrected to `false`
   - SingleSelect/MultiSelect with `PreciseMode: false` ? Auto-corrected to `true`

2. **Missing Required Flags**:
   - Auto-adds `IncludeBoundaryValues: true`
   - Auto-adds `AllowValidEquivalenceClasses: true`  
   - Auto-adds `AllowInvalidEquivalenceClasses: true`

3. **Options Format**:
   - Auto-converts `Options: ["A", "B"]` to `PreciseTestValues: [{"Value": "A", "Category": "Valid"}]`

---

## ?? **Performance Comparison**

| Aspect | **Pairwise** | **Hybrid ABC** |
|--------|--------------|----------------|
| **Speed** | ? Very Fast | ?? Slower |
| **Test Count** | ?? Minimal | ?? Optimized |
| **Fault Detection** | ? Good | ?? Excellent |
| **Best For** | Quick coverage | Thorough testing |

---

## ?? **Recommendation Matrix**

| Scenario | Recommended Tool | Reason |
|----------|------------------|--------|
| **CI/CD Pipeline** | `generate_pairwise_test_cases` | Fast feedback |
| **Release Testing** | `generate_hybrid_test_cases` | Maximum coverage |
| **Form Validation** | `generate_hybrid_test_cases` | Complex interactions |
| **API Smoke Tests** | `generate_pairwise_test_cases` | Quick validation |
| **Security Testing** | `generate_hybrid_test_cases` | Edge case discovery |
| **Performance Testing** | `generate_pairwise_test_cases` | Minimal overhead |

---

## ?? **Pro Tips**

1. **Start with Pairwise** for initial exploration, then use **Hybrid ABC** for critical paths
2. **Configure settings once** at the beginning of your testing session
3. **Use meaningful method names** to organize your generated tests
4. **Set a fixed seed** in ABCSettings for reproducible test generation
5. **Adjust `FinalPopulationSelectionRatio`** to control test suite size vs. coverage trade-off

---

## ?? **Workflow Example**

```bash
# 1. Check current settings
get_testimize_settings()

# 2. Configure optimal settings for your project (plain text!)
configure_testimize_settings({
  "instruction": "Set method name to MyAppTests and mutation rate to 0.5"
})

# 3. Or configure individual properties
configure_testimize_settings({
  "TotalPopulationGenerations": 75,
  "Seed": 42
})

# 4. Quick smoke test with pairwise
generate_pairwise_test_cases({
  "parameters": [...],
  "methodName": "SmokeTest"
})

# 5. Comprehensive testing with hybrid ABC
generate_hybrid_test_cases({
  "parameters": [...],
  "methodName": "ComprehensiveTest"
})
```

---

## ?? **Conversational Example with Assistant**

Here's how you can naturally interact with the assistant:

### **?? User:** 
*"I want to generate test cases for a user registration form. It has name (3-50 chars), email (6-100 chars), password (8-20 chars), age (18-120), and country selection from US, UK, Canada, France."*

### **?? Assistant:**
*"I'll generate comprehensive test cases for your registration form using the hybrid algorithm for maximum coverage. Let me set that up:"*

### **?? User:**
*"Actually, first show me the current settings."*

### **?? Assistant:**
*"I'll check the current Testimize configuration for you."*
`get_testimize_settings()`

### **?? User:**
*"Can you set the method name to UserRegistrationTest and increase the optimization to 100 generations?"*

### **?? Assistant:**
*"I'll update those settings using plain text instructions:"*
`configure_testimize_settings({ "instruction": "Set method name to UserRegistrationTest and total generations to 100" })`

### **?? User:**
*"Perfect! Now generate the test cases."*

### **?? Assistant:**
*"I'll generate comprehensive test cases for your registration form using the Hybrid ABC algorithm:"*
`generate_hybrid_test_cases({...})`

### **?? User:**
*"Great! Can you also make a quick pairwise version for smoke testing?"*

### **?? Assistant:**
*"I'll create a fast pairwise version for smoke testing:"*
`generate_pairwise_test_cases({...})`

### **?? User:**
*"One more thing - can you set the seed to 12345 so the tests are reproducible?"*

### **?? Assistant:**
*"I'll set a fixed seed for reproducible test generation:"*
`configure_testimize_settings({ "Seed": 12345 })`

This approach gives you both **speed** and **thoroughness** when you need them! ??