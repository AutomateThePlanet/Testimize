# Useful API + MCP (stdio)

A tiny .NET 8 Web API with 3 endpoints and an MCP stdio server for GitHub Copilot.

## Endpoints
- GET /health → { status, service, version, timeUtc }
- GET /time   → { utc, unixSeconds, iso }
- GET /guid   → { value, kind }

## Run (HTTP)
dotnet run --project src/UsefulApi/UsefulApi.csproj
curl http://localhost:8080/health

## Run (MCP stdio)
dotnet run --project src/UsefulApi/UsefulApi.csproj -- --mcp

## Docker
docker build -t useful-api:1.0 .
docker run --rm -p 8080:8080 useful-api:1.0
