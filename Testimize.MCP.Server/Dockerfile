# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the entire solution
COPY . .

# Restore dependencies for the specific project
RUN dotnet restore Testimize.MCP.Server/Testimize.MCP.Server.csproj

# Build and publish the Testimize.MCP.Server project with package generation disabled
RUN dotnet publish Testimize.MCP.Server/Testimize.MCP.Server.csproj -c Release -o /app --no-restore -p:GeneratePackageOnBuild=false

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Support both HTTP and MCP modes
# HTTP mode: docker run -p 8088:8088 testimize-mcp-server:1.0
# MCP mode: docker run --rm -i testimize-mcp-server:1.0 --mcp
ENV ASPNETCORE_URLS=http://0.0.0.0:8088
EXPOSE 8088
ENTRYPOINT ["dotnet", "Testimize.MCP.Server.dll"]
