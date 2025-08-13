// File: Mcp/McpServer.cs
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UsefulApi.Mcp;

public static class McpServer
{
    public static async Task RunAsync()
    {
        await foreach (var req in JsonRpc.ReadRequestsAsync())
        {
            try
            {
                // Debugging log for incoming requests
                Console.Error.WriteLine($"[DEBUG] Received request: Method={req.Method}, Id={req.Id}, Params={req.Params}");

                switch (req.Method)
                {
                    case "initialize":
                        await JsonRpc.WriteResponseAsync(new
                        {
                            jsonrpc = "2.0",
                            id = req.Id,
                            result = new
                            {
                                protocolVersion = "2023-10-10",
                                serverInfo = new
                                {
                                    name = "useful-api",
                                    version = "1.0.0"
                                },
                                capabilities = new
                                {
                                    tools = new { list = new { }, call = new { } },
                                    resources = new { list = new { } },
                                    prompts = new { list = new { } }
                                }
                            }
                        });
                        break;

                    case "tools/list":
                        await JsonRpc.WriteResponseAsync(new
                        {
                            jsonrpc = "2.0",
                            id = req.Id,
                            result = new
                            {
                                tools = new[]
                                {
                                new {
                                    name = "health.check",
                                    description = "Get API health info.",
                                    inputSchema = new { type = "object", properties = new { } }
                                },
                                new {
                                    name = "time.now",
                                    description = "Get current UTC time.",
                                    inputSchema = new { type = "object", properties = new { } }
                                },
                                new {
                                    name = "guid.new",
                                    description = "Generate a random GUID.",
                                    inputSchema = new { type = "object", properties = new { } }
                                }
                            }
                            }
                        });
                        break;

                    case "tools/call":
                        var args = req.Params?.Deserialize<JsonObject>() ?? new JsonObject();
                        var name = args["name"]?.GetValue<string>() ?? "";
                        object result = name switch
                        {
                            "health.check" => new { status = "ok", service = "useful-api", version = "1.0.0", timeUtc = DateTimeOffset.UtcNow },
                            "time.now" => new { utc = DateTimeOffset.UtcNow, unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), iso = DateTimeOffset.UtcNow.ToString("o") },
                            "guid.new" => new { value = Guid.NewGuid(), kind = "random-v4" },
                            _ => new { error = $"Unknown tool '{name}'" }
                        };

                        await JsonRpc.WriteResponseAsync(new
                        {
                            jsonrpc = "2.0",
                            id = req.Id,
                            result = new
                            {
                                content = new[] {
                                new { type = "text", text = JsonSerializer.Serialize(result) }
                            },
                                isError = result is { } r && r.GetType().GetProperty("error") != null
                            }
                        });
                        break;

                    default:
                        await JsonRpc.WriteResponseAsync(new
                        {
                            jsonrpc = "2.0",
                            id = req.Id,
                            error = new { code = -32601, message = $"Unknown method: {req.Method}" }
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                await JsonRpc.WriteResponseAsync(new
                {
                    jsonrpc = "2.0",
                    id = req.Id,
                    error = new { code = -32000, message = ex.Message }
                });
            }
        }
    }

}
