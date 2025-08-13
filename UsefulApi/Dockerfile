# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Support both HTTP and MCP modes
# HTTP mode: docker run -p 8088:8088 useful-api:1.0
# MCP mode: docker run --rm -i useful-api:1.0 --mcp
ENV ASPNETCORE_URLS=http://0.0.0.0:8088
EXPOSE 8088
ENTRYPOINT ["dotnet", "UsefulApi.dll"]
