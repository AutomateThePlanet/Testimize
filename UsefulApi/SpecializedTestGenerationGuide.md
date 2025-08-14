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
# 1. Configure optimal settings for your project
configure_testimize_settings({
  "MethodName": "MyAppTests",
  "ABCSettings": { "Seed": 42, "TotalPopulationGenerations": 75 }
})

# 2. Quick smoke test with pairwise
generate_pairwise_test_cases({
  "parameters": [...],
  "methodName": "SmokeTest"
})

# 3. Comprehensive testing with hybrid ABC
generate_hybrid_test_cases({
  "parameters": [...],
  "methodName": "ComprehensiveTest"
})
```

This approach gives you both **speed** and **thoroughness** when you need them! ??