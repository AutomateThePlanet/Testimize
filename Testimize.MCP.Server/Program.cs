using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nerdbank.Streams;
using Testimize.Parameters.Core;
using Testimize.Parameters;
using Testimize.Contracts;
using Testimize.Usage;

var builder = WebApplication.CreateBuilder(args);

// Register services following Dependency Inversion Principle
builder.Services.AddSingleton<IUtilityService, UtilityService>();
builder.Services.AddSingleton<IMcpProtocolHandler, McpProtocolHandler>();
//builder.Services.AddSingleton<Testimize.MCP.ServerService>(); // Facade for backwards compatibility

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

// --- HTTP API using business logic service directly ---

app.MapGet("/", () => Results.Redirect("/health"))
   .ExcludeFromDescription();

app.MapGet("/health", (IUtilityService service) => Results.Json(service.GetHealth()))
.WithName("GetHealth")
.WithSummary("Health check")
.WithDescription("Returns service health details and current UTC time.")
.WithOpenApi();

app.MapGet("/time", (IUtilityService service) => Results.Json(service.GetTime()))
.WithName("GetTime")
.WithSummary("Current UTC time")
.WithDescription("Returns the current time in multiple formats (ISO 8601, Unix seconds).")
.WithOpenApi();

app.MapGet("/guid", (IUtilityService service) => Results.Json(service.GenerateGuid()))
.WithName("NewGuid")
.WithSummary("Generate GUID")
.WithDescription("Generates a new random version 4 GUID.")
.WithOpenApi();

app.MapPost("/echo", (JsonElement body) => Results.Json(new { received = body }))
.WithName("Echo")
.WithSummary("Echo")
.WithDescription("Echos the posted JSON payload.")
.WithOpenApi();

app.MapPost("/generate-test-cases", (JsonDocument body, IUtilityService service) =>
{
    try
    {
        var root = body.RootElement;

        if (!root.TryGetProperty("parameters", out var parametersElement) || 
            !root.TryGetProperty("settings", out var settingsElement))
        {
            return Results.BadRequest(new { error = "Request must contain 'parameters' and 'settings' properties" });
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        var parameters = new List<IInputParameter>();
        for (int i = 0; i < parametersElement.GetArrayLength(); i++)
        {
            try
            {
                var parameterElement = parametersElement[i];
                var rawJson = parameterElement.GetRawText();

                Console.WriteLine($"DEBUG: Processing parameter {i}: {rawJson}");

                var universalParameter = JsonSerializer.Deserialize<UniversalDataParameter>(rawJson, jsonOptions);
                if (universalParameter != null)
                {
                    Console.WriteLine($"DEBUG: ParameterType = '{universalParameter.ParameterType ?? "NULL"}'");

                    if (string.IsNullOrWhiteSpace(universalParameter.ParameterType))
                    {
                        return Results.BadRequest(new { error = $"Parameter {i} has empty or null ParameterType" });
                    }

                    // Handle SingleSelect and MultiSelect options
                    if (universalParameter.ParameterType == "SingleSelect" || universalParameter.ParameterType == "MultiSelect")
                    {
                        if (universalParameter.Options == null || universalParameter.Options.Length == 0)
                        {
                            return Results.BadRequest(new { error = $"Parameter {i} of type {universalParameter.ParameterType} must have non-empty Options" });
                        }

                        universalParameter.PreciseTestValues = universalParameter.Options.Select(option => new TestValue
                        {
                            Value = option,
                            Category = TestValueCategory.Valid
                        }).ToArray();
                    }

                    // Ensure defaults for exploratory mode
                    if (!universalParameter.PreciseMode)
                    {
                        universalParameter.IncludeBoundaryValues ??= true;
                        universalParameter.AllowValidEquivalenceClasses ??= true;
                        universalParameter.AllowInvalidEquivalenceClasses ??= true;
                    }

                    var parameter = DataParameterFactory.CreateFromUniversal(universalParameter);
                    parameters.Add(parameter);

                    Console.WriteLine($"DEBUG: Successfully created parameter of type {parameter.GetType().Name}");
                }
                else
                {
                    return Results.BadRequest(new { error = $"Parameter {i} could not be deserialized" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error processing parameter {i}: {ex.Message}");
                return Results.BadRequest(new { error = $"Failed to process parameter {i}: {ex.Message}" });
            }
        }

        var settings = JsonSerializer.Deserialize<PreciseTestEngineSettings>(settingsElement.GetRawText(), jsonOptions);

        if (settings == null)
        {
            return Results.BadRequest(new { error = "Failed to deserialize settings" });
        }

        // Only override specific critical settings - let library defaults handle the rest
        if (settings.Mode == (TestGenerationMode)0) // Only override if default Pairwise was used
        {
            settings.Mode = (TestGenerationMode)4; // HybridArtificialBeeColony
        }
        
        if (string.IsNullOrWhiteSpace(settings.MethodName) || settings.MethodName == "TestMethodName")
        {
            settings.MethodName = "FormValidation";
        }

        // Let ABCSettings use library defaults - no hardcoding
        if (settings.ABCSettings == null)
        {
            settings.ABCSettings = new Testimize.ABCGenerationSettings();
        }

        var testCases = service.Generate(parameters, settings);
        return Results.Json(new { testCases });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DEBUG: Outer exception: {ex.Message}");
        Console.WriteLine($"DEBUG: Stack trace: {ex.StackTrace}");
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GenerateTestCases")
.WithSummary("Generate Test Cases")
.WithDescription("Generates test cases based on input parameters and settings.")
.WithOpenApi();

app.Run();
