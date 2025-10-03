using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nerdbank.Streams;
using Testimize.Parameters.Core;
using Testimize.Parameters;
using Testimize.Contracts;
using Testimize.Usage;
using Testimize.MCP.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services following Dependency Inversion Principle
builder.Services.AddSingleton<IUtilityService, UtilityService>();
builder.Services.AddSingleton<IMcpProtocolHandler, McpProtocolHandler>();
//builder.Services.AddSingleton<Testimize.MCP.ServerService>(); // Facade for backwards compatibility

// Register ABCSettingsService
builder.Services.AddSingleton<ABCSettingsService>(sp => new ABCSettingsService(McpProtocolHandler.DefaultSettings));

// Add controllers support
builder.Services.AddControllers();

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Basic API info
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Testimize MCP Server API",
        Version = "v1",
        Description = "Test generation utility API with MCP stdio mode."
    });

    // Include XML comments (if enabled in .csproj)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

if (args.Contains("--mcp"))
{
    // Use dedicated MCP handler (Single Responsibility)
    var mcpHandler = app.Services.GetRequiredService<IMcpProtocolHandler>();
    
    // Create a simple line-based JSON-RPC handler for VS Code compatibility
    var input = Console.OpenStandardInput();
    var output = Console.OpenStandardOutput();
    
    // Use a simple approach that reads JSON lines and writes JSON responses
    using var reader = new StreamReader(input);
    using var writer = new StreamWriter(output) { AutoFlush = true };
    
    await Console.Error.WriteLineAsync("[DEBUG] MCP server started, waiting for requests...");
    
    while (true)
    {
        try
        {
            var line = await reader.ReadLineAsync();
            if (line == null) 
            {
                await Console.Error.WriteLineAsync("[DEBUG] End of input stream reached");
                break;
            }
            
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            await Console.Error.WriteLineAsync($"[DEBUG] Received: {line}");
            
            // Parse JSON-RPC request
            var request = JsonSerializer.Deserialize<JsonElement>(line);
            var method = request.GetProperty("method").GetString();
            var id = request.TryGetProperty("id", out var idProp) ? idProp : (JsonElement?)null;
            var paramsElement = request.TryGetProperty("params", out var paramsProp) ? paramsProp : (JsonElement?)null;
            
            await Console.Error.WriteLineAsync($"[DEBUG] Method: {method}, ID: {id}");
            
            object? result = null;
            object? error = null;
            
            try
            {
                // Handle MCP methods
                result = method switch
                {
                    "initialize" => mcpHandler.Initialize(paramsElement?.Deserialize<object>()),
                    "tools/list" => mcpHandler.ToolsList(),
                    "tools/call" => mcpHandler.ToolsCall(paramsElement ?? new JsonElement()),
                    "notifications/initialized" => new { }, // Handle initialization notification
                    "prompts/list" => new { prompts = new object[] { } }, // Empty prompts list
                    _ => throw new Exception($"Unknown method: {method}")
                };
                
                await Console.Error.WriteLineAsync($"[DEBUG] Method executed successfully");
            }
            catch (Exception ex)
            {
                error = new { code = -32000, message = ex.Message };
                await Console.Error.WriteLineAsync($"[DEBUG] Method execution failed: {ex.Message}");
            }
            
            // Send response
            var response = new
            {
                jsonrpc = "2.0",
                id = id?.Deserialize<object>(),
                result = error == null ? result : null,
                error = error
            };
            
            var responseJson = JsonSerializer.Serialize(response);
            await Console.Error.WriteLineAsync($"[DEBUG] Sending response: {responseJson}");
            await writer.WriteLineAsync(responseJson);
        }
        catch (Exception ex)
        {
            // Log error but continue listening
            await Console.Error.WriteLineAsync($"[ERROR] Error processing request: {ex.Message}");
        }
    }
    
    return;
}

// Swagger middleware (enable UI in all envs; feel free to gate by IsDevelopment)
app.UseSwagger();
app.UseSwaggerUI();

// Add routing for controllers
app.UseRouting();
app.MapControllers();

app.Run();
