using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using UsefulApi.Mcp;

var builder = WebApplication.CreateBuilder(args);

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Basic API info
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Useful API",
        Version = "v1",
        Description = "Tiny utility API with MCP stdio mode."
    });

    // Include XML comments (if enabled in .csproj)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

// MCP mode (unchanged)
if (args.Contains("--mcp"))
{
    await McpServer.RunAsync();
    return;
}

// Swagger middleware (enable UI in all envs; feel free to gate by IsDevelopment)
app.UseSwagger();
app.UseSwaggerUI();

// --- HTTP API ---

app.MapGet("/", () => Results.Redirect("/health"))
   .ExcludeFromDescription();

app.MapGet("/health", () =>
{
    return Results.Json(new
    {
        status = "ok",
        service = "useful-api",
        version = "1.1.0",
        timeUtc = DateTimeOffset.UtcNow
    });
})
.WithName("GetHealth")
.WithSummary("Health check")
.WithDescription("Returns service health details and current UTC time.")
.WithOpenApi();

app.MapGet("/time", () =>
{
    var now = DateTimeOffset.UtcNow;
    return Results.Json(new
    {
        utc = now,
        unixSeconds = now.ToUnixTimeSeconds(),
        iso = now.ToString("o")
    });
})
.WithName("GetTime")
.WithSummary("Current UTC time")
.WithDescription("Returns the current time in multiple formats (ISO 8601, Unix seconds).")
.WithOpenApi();

app.MapGet("/guid", () =>
{
    return Results.Json(new
    {
        value = Guid.NewGuid(),
        kind = "random-v4"
    });
})
.WithName("NewGuid")
.WithSummary("Generate GUID")
.WithDescription("Generates a new random version 4 GUID.")
.WithOpenApi();

app.MapPost("/echo", (JsonElement body) => Results.Json(new { received = body }))
.WithName("Echo")
.WithSummary("Echo")
.WithDescription("Echos the posted JSON payload.")
.WithOpenApi();

app.Run();
