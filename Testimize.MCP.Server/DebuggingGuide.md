# ?? Enhanced Debugging Guide for Testimize MCP Settings

## ?? **What's New**

I've significantly enhanced the debugging capabilities of the `configure_testimize_settings` tool to make troubleshooting much easier. Here's what you'll now see when settings updates fail or succeed.

---

## ?? **Enhanced Response Structure**

When you call `configure_testimize_settings`, you'll now get a comprehensive response:

```json
{
  "message": "? 2 setting(s) updated successfully, ? 1 error(s) occurred",
  "successful": [
    "Updated MutationRate from 0.300 to 0.600",
    "Updated TotalPopulationGenerations from 50 to 100"
  ],
  "errors": [
    "EliteSelectionRatio value 1.5 is out of range [0.1-1.0]. Value not updated."
  ],
  "warnings": [
    "Unknown property 'invalidProperty' ignored. Valid ABC properties: mutationRate, eliteSelectionRatio, ..."
  ],
  "currentSettings": { /* complete current settings */ },
  "debug": {
    "requestReceived": "{ /* your original JSON */ }",
    "totalPropertiesProcessed": 3,
    "processingTimestamp": "2025-01-20 15:30:45.123 UTC"
  }
}
```

---

## ?? **Detailed Console Logging**

The console now shows extensive debug information:

### **Individual Property Processing**
```
?? DEBUG: Processing MutationRate - ValueKind: Number, RawValue: '0.6'
?? DEBUG: Parsed MutationRate value: 0.6
? SUCCESS: Updated MutationRate from 0.300 to 0.600

?? DEBUG: Processing EliteSelectionRatio - ValueKind: Number, RawValue: '1.5'
?? DEBUG: Parsed EliteSelectionRatio value: 1.5
? VALIDATION ERROR: EliteSelectionRatio value 1.5 is out of range [0.1-1.0]. Value not updated.
```

### **Request Analysis**
```
?? DEBUG: Processing individual ABC property updates. Properties in request:
  - MutationRate: 0.6 (ValueKind: Number)
  - EliteSelectionRatio: 1.5 (ValueKind: Number)
  - UnknownProperty: "test" (ValueKind: String)
```

### **Plain Text Instructions**
```
?? DEBUG: Processing plain text instruction: 'set mutation rate to 0.6 and elite selection to 0.3'
? PLAIN TEXT SUCCESS: Updated MutationRate from 0.300 to 0.600 (from instruction)
? PLAIN TEXT SUCCESS: Updated EliteSelectionRatio from 0.500 to 0.300 (from instruction)
```

---

## ?? **Error Types and Messages**

### **1. Parse Errors**
**When:** Value cannot be converted to expected type
```
? PARSE ERROR: Failed to parse MutationRate from 'invalid' - expected number
? PARSE ERROR: Failed to parse EnableScoutPhase from 'maybe' - expected boolean (true/false)
```

### **2. Validation Errors**
**When:** Value is parsed but out of valid range
```
? VALIDATION ERROR: MutationRate value 1.5 is out of range [0.1-1.0]. Value not updated.
? VALIDATION ERROR: TotalPopulationGenerations value 5 is out of range [10-1000]. Value not updated.
```

### **3. Plain Text Parse Errors**
**When:** Plain text instruction cannot be understood
```
? PLAIN TEXT PARSE ERROR: Could not parse mutation rate from instruction. Expected format: 'mutation rate to 0.5'
? PLAIN TEXT PARSE ERROR: Could not determine scout phase action from instruction. Expected 'enable scout phase' or 'disable scout phase'
```

---

## ?? **Valid Ranges Reference**

| Property | Type | Valid Range | Example |
|----------|------|-------------|---------|
| **TotalPopulationGenerations** | Integer | 10-1000 | `100` |
| **MutationRate** | Number | 0.1-1.0 | `0.6` |
| **EliteSelectionRatio** | Number | 0.1-1.0 | `0.3` |
| **OnlookerSelectionRatio** | Number | 0.05-1.0 | `0.1` |
| **ScoutSelectionRatio** | Number | 0.1-1.0 | `0.3` |
| **FinalPopulationSelectionRatio** | Number | 0.1-1.0 | `0.5` |
| **StagnationThresholdPercentage** | Number | 0.5-1.0 | `0.75` |
| **CoolingRate** | Number | 0.8-1.0 | `0.95` |
| **EnableScoutPhase** | Boolean | true/false | `true` |
| **EnableOnlookerSelection** | Boolean | true/false | `true` |
| **EnforceMutationUniqueness** | Boolean | true/false | `true` |
| **AllowMultipleInvalidInputs** | Boolean | true/false | `false` |
| **Seed** | Integer | Any integer | `42` |

---

## ??? **Common Troubleshooting Scenarios**

### **Scenario 1: "My mutation rate isn't updating"**

**What to look for:**
1. Check the console for parsing errors
2. Verify the value is between 0.1 and 1.0
3. Check property name casing (`mutationRate` not `MutationRate`)

**Example:**
```json
// ? Wrong (out of range)
{"mutationRate": 1.5}

// ? Correct
{"mutationRate": 0.6}
```

### **Scenario 2: "Boolean settings not working"**

**What to look for:**
1. Ensure you're using actual booleans, not strings
2. Check the console for ValueKind information

**Example:**
```json
// ? Wrong (string)
{"enableScoutPhase": "true"}

// ? Correct (boolean)
{"enableScoutPhase": true}
```

### **Scenario 3: "Plain text instructions not working"**

**What to look for:**
1. Check the exact format in the console logs
2. Verify the instruction contains expected keywords

**Example:**
```json
// ? Unclear
{"instruction": "make mutation better"}

// ? Clear
{"instruction": "set mutation rate to 0.6"}
```

---

## ?? **Debug Information Available**

### **In Response Object:**
- `successful`: Array of successful changes with old?new values
- `errors`: Array of all errors encountered
- `warnings`: Array of warnings (unknown properties, etc.)
- `debug.requestReceived`: Your original JSON request
- `debug.totalPropertiesProcessed`: Number of properties in request
- `debug.processingTimestamp`: When the request was processed

### **In Console Logs:**
- Raw JSON input received
- Each property processing step
- Parse attempts and results
- Validation checks and outcomes
- Old vs new values for successful changes
- Summary of total changes applied

---

## ?? **Tips for Effective Debugging**

1. **Always check the console logs** - They contain the most detailed information
2. **Look at the `errors` array** - It contains specific validation failures
3. **Verify property names** - Use lowercase camelCase (e.g., `mutationRate`)
4. **Check data types** - Numbers should be numbers, booleans should be booleans
5. **Validate ranges** - Each property has specific valid ranges
6. **Use plain text for complex updates** - Sometimes easier than JSON

---

## ?? **Rebuilding Docker Image**

After these enhancements, rebuild the Docker image:

```bash
docker build -f Testimize.MCP.Server/Dockerfile -t testimize-mcp-server:1.0 .
```

The enhanced debugging will now be available in your GitHub Copilot MCP integration!