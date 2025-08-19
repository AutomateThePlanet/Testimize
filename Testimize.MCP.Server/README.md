# Testimize MCP Server

A .NET 8 Web API with HTTP endpoints and MCP stdio server for GitHub Copilot integration.

## Features
- **HTTP API**: REST endpoints for direct API access
- **MCP Server**: Model Context Protocol server for VS Code GitHub Copilot
- **Docker Support**: Both modes available in containers
- **SOLID Architecture**: Clean separation of concerns

## HTTP API Endpoints
- `GET /health` → API health info
- `GET /time` → Current UTC time
- `GET /guid` → Generate random GUID
- `POST /echo` → Echo JSON payload

## MCP Tools (for VS Code GitHub Copilot)
- `health_check` → Get API health info
- `get_time` → Get current UTC time
- `generate_guid` → Generate a random GUID

## Development Usage

### HTTP Mode (Development)
```bash
dotnet run --project Testimize.MCP.Server
# API available at: http://localhost:5000
# Swagger UI at: http://localhost:5000/swagger
```

### MCP Mode (Development)
```bash
dotnet run --project Testimize.MCP.Server -- --mcp
# Starts MCP server on stdio for VS Code integration
```

## Docker Usage

### Build Docker Image
```bash
docker build -t testimize-mcp-server:1.0 .
```

### HTTP Mode (Docker)
```bash
# Run HTTP API in container
docker run --rm -p 8088:8088 testimize-mcp-server:1.0

# Test HTTP API
curl http://localhost:8088/health
```

### MCP Mode (Docker)
```bash
# Run MCP server in container (for VS Code)
docker run --rm -i testimize-mcp-server:1.0 --mcp
```

## VS Code GitHub Copilot Configuration

Add this to your VS Code `settings.json` or MCP configuration:

### Option 1: Direct .NET execution (Recommended for development)
```json
{
  "testimize-mcp-server": {
    "type": "stdio",
    "command": "dotnet",
    "args": ["run", "--project", "path/to/Testimize.MCP.Server", "--", "--mcp"]
  }
}
```

### Option 2: Docker execution
```json
{
  "testimize-mcp-server": {
    "type": "stdio", 
    "command": "docker",
    "args": ["run", "--rm", "-i", "testimize-mcp-server:1.0", "--mcp"]
  }
}
```

### Option 3: Local binary (after publish)
```json
{
  "testimize-mcp-server": {
    "type": "stdio",
    "command": "path/to/Testimize.MCP.Server.exe", 
    "args": ["--mcp"]
  }
}
```

## Architecture
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   HTTP Client   │    │   VS Code MCP    │    │  Business Logic │
│                 │    │                  │    │                 │
│  REST API calls │───▶│  JSON-RPC stdio  │───▶│ IUtilityService │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                │                        │
                                ▼                        │
                       ┌──────────────────┐             │
                       │ IMcpProtocolHandler │◀──────────┘
                       │                  │
                       │ • Initialize     │
                       │ • ToolsList      │
                       │ • ToolsCall      │
                       └──────────────────┘
## Extending the API

To add new functionality that works in both HTTP and MCP modes:

1. **Add business logic** to `IUtilityService` and `UtilityService`
2. **Add HTTP endpoint** in `Program.cs`
3. **Add MCP tool** definition in `McpProtocolHandler.ToolsList()`
4. **Add tool implementation** in `McpProtocolHandler.ToolsCall()`

Example:// 1. Add to IUtilityService
public interface IUtilityService
{
    object GetWeather(string city);
}

// 2. Add HTTP endpoint
app.MapGet("/weather/{city}", (string city, IUtilityService service) => 
    Results.Json(service.GetWeather(city)));

// 3. Add to ToolsList()
new {
    name = "get_weather",
    description = "Get weather for a city",
    inputSchema = new {
        type = "object",
        properties = new { city = new { type = "string" } },
        required = new[] { "city" }
    }
}

// 4. Add to ToolsCall()
"get_weather" => _utilityService.GetWeather(
    paramsDict?["city"]?.ToString() ?? "Unknown")
## Troubleshooting

### MCP Connection Issues
- Ensure Docker image is built: `docker build -t testimize-mcp-server:1.0 .`
- Check VS Code MCP configuration syntax
- Verify paths in MCP configuration are correct
- Check VS Code developer console for MCP errors

### HTTP API Issues  
- Verify port 8088 is available
- Check Docker container logs: `docker logs <container-id>`
- Test with curl: `curl http://localhost:8088/health`

### Development Issues
- Run `dotnet build` to verify compilation
- Check for missing dependencies: `dotnet restore`
- Verify .NET 8 SDK is installed
