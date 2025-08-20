# Testimize MCP Server API Endpoints

## Overview
All endpoint methods have been moved from `Program.cs` to dedicated controllers for better organization and maintainability.

## ?? Test Case Generation Endpoints
**Controller**: `TestCaseGenerationController`
**Base Route**: `/api/test-cases`

### 1. Hybrid ABC Test Generation
- **Endpoint**: `POST /api/test-cases/hybrid`
- **Description**: Generate test cases using Hybrid Artificial Bee Colony algorithm with optimized defaults
- **Features**:
  - Maximum fault detection
  - Advanced optimization
  - Comprehensive testing
- **Request Body**:
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
    }
  ],
  "methodName": "OptionalMethodName"
}
```

### 2. Pairwise Test Generation
- **Endpoint**: `POST /api/test-cases/pairwise`
- **Description**: Generate test cases using Pairwise algorithm for fast, efficient testing
- **Features**:
  - Fast execution
  - Minimal test suite
  - Good coverage
- **Request Body**: Same format as hybrid

### 3. Legacy Full Control Test Generation
- **Endpoint**: `POST /api/test-cases/generate`
- **Description**: Generate test cases with full control over parameters and settings
- **Request Body**:
```json
{
  "parameters": [...],
  "settings": {
    "Mode": 4,
    "TestCaseCategory": 0,
    "MethodName": "TestMethod",
    "ABCSettings": {...}
  }
}
```

## ?? Settings Management Endpoints
**Controller**: `ABCSettingsController`  
**Base Route**: `/api/settings`

### 1. Configure Settings
- **Endpoint**: `POST /api/settings/configure`
- **Description**: Comprehensive settings configuration with plain text support

### 2. Get Current Settings
- **Endpoint**: `GET /api/settings/current`
- **Description**: Retrieve current settings with explanations

### 3. Plain Text Configuration
- **Endpoint**: `POST /api/settings/configure/instruction`
- **Description**: Update settings using plain text instructions

### 4. ABC Settings Update
- **Endpoint**: `POST /api/settings/abc/update`
- **Description**: Update ABC settings with a complete settings object

### 5. Legacy Update (Backward Compatibility)
- **Endpoint**: `POST /api/settings/update`
- **Description**: Legacy endpoint for updating ABC settings

## ?? Utility Endpoints
**Controller**: `UtilityController`
**Base Route**: `/api`

### 1. Health Check
- **Endpoint**: `GET /api/health`
- **Description**: Returns service health details and current UTC time

### 2. Current Time
- **Endpoint**: `GET /api/time`
- **Description**: Returns current time in multiple formats (ISO 8601, Unix seconds)

### 3. Generate GUID
- **Endpoint**: `GET /api/guid`
- **Description**: Generates a new random version 4 GUID

### 4. Echo
- **Endpoint**: `POST /api/echo`
- **Description**: Echoes the posted JSON payload

### 5. Root Redirect
- **Endpoint**: `GET /`
- **Description**: Redirects to health endpoint

## ?? Benefits of the New Structure

### **Separation of Concerns**
- Each controller handles a specific domain
- Clear organization by functionality
- Easier to maintain and extend

### **Better API Documentation**
- Controllers automatically appear in Swagger
- Proper endpoint categorization
- Enhanced API discoverability

### **Improved Testing**
- Controllers can be unit tested independently
- Mock dependencies easily
- Better integration testing

### **Cleaner Program.cs**
- Focused only on application setup
- No endpoint definitions
- Better separation of infrastructure and business logic

## ?? Usage Examples

### Test Hybrid ABC Generation
```bash
curl -X POST "http://localhost:8088/api/test-cases/hybrid" \
     -H "Content-Type: application/json" \
     -d '{
       "parameters": [
         {
           "ParameterType": "Text",
           "PreciseMode": false,
           "MinBoundary": 3,
           "MaxBoundary": 20,
           "IncludeBoundaryValues": true,
           "AllowValidEquivalenceClasses": true,
           "AllowInvalidEquivalenceClasses": true
         }
       ],
       "methodName": "MyTest"
     }'
```

### Configure Settings with Plain Text
```bash
curl -X POST "http://localhost:8088/api/settings/configure/instruction" \
     -H "Content-Type: application/json" \
     -d '"Set mutation rate to 0.6 and total generations to 100"'
```

### Get Current Settings
```bash
curl -X GET "http://localhost:8088/api/settings/current"
```

## ?? Complete API Map

| Endpoint | Method | Controller | Purpose |
|----------|--------|------------|---------|
| `/` | GET | UtilityController | Root redirect |
| `/api/health` | GET | UtilityController | Health check |
| `/api/time` | GET | UtilityController | Current time |
| `/api/guid` | GET | UtilityController | Generate GUID |
| `/api/echo` | POST | UtilityController | Echo payload |
| `/api/test-cases/hybrid` | POST | TestCaseGenerationController | Hybrid ABC generation |
| `/api/test-cases/pairwise` | POST | TestCaseGenerationController | Pairwise generation |
| `/api/test-cases/generate` | POST | TestCaseGenerationController | Legacy full control |
| `/api/settings/configure` | POST | ABCSettingsController | Configure settings |
| `/api/settings/current` | GET | ABCSettingsController | Get current settings |
| `/api/settings/configure/instruction` | POST | ABCSettingsController | Plain text config |
| `/api/settings/abc/update` | POST | ABCSettingsController | ABC settings update |
| `/api/settings/update` | POST | ABCSettingsController | Legacy update |

All endpoints are now properly organized in controllers and accessible via both HTTP API and MCP protocol! ??