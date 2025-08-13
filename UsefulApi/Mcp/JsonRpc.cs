using System.Text.Json;
using System.Text.Json.Serialization;

namespace UsefulApi.Mcp;

// Model for incoming JSON-RPC requests
public record JsonRpcRequest(
    [property: JsonPropertyName("jsonrpc")] string JsonRpc,
    [property: JsonPropertyName("id")] JsonElement? Id,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("params")] JsonElement? Params
);

public static class JsonRpc
{
    private static bool _useFramedOutput = true;

    // Read JSON-RPC requests from stdin with MCP framing
    public static async IAsyncEnumerable<JsonRpcRequest> ReadRequestsAsync()
    {
        var stdin = Console.OpenStandardInput();
        using var reader = new StreamReader(stdin);

        Console.Error.WriteLine("[DEBUG] Starting to read requests from stdin...");

        while (true)
        {
            string? line = await reader.ReadLineAsync();
            if (line == null)
            {
                Console.Error.WriteLine("[DEBUG] End of stream reached, exiting...");
                yield break;
            }

            Console.Error.WriteLine($"[DEBUG] Read line: '{line}'");

            // Check if this is a Content-Length header (MCP framing)
            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int contentLength))
                {
                    Console.Error.WriteLine($"[DEBUG] Found Content-Length: {contentLength}");
                    _useFramedOutput = true; // Client uses framed input, so use framed output
                    
                    // Read empty line after headers
                    await reader.ReadLineAsync();
                    
                    // Read the actual JSON payload
                    var buffer = new char[contentLength];
                    var read = 0;
                    while (read < contentLength)
                    {
                        var n = await reader.ReadAsync(buffer, read, contentLength - read);
                        if (n == 0) 
                        {
                            Console.Error.WriteLine("[DEBUG] End of stream reached while reading payload, exiting...");
                            yield break;
                        }
                        read += n;
                    }

                    var json = new string(buffer, 0, read);
                    Console.Error.WriteLine($"[DEBUG] Read JSON payload from MCP frame: {json}");
                    
                    var request = ParseJsonRequest(json);
                    if (request != null)
                    {
                        Console.Error.WriteLine($"[DEBUG] Successfully parsed framed request: {request.Method}");
                        yield return request;
                    }
                }
            }
            // Check if this line is direct JSON (no MCP framing)
            else if (line.TrimStart().StartsWith("{"))
            {
                Console.Error.WriteLine($"[DEBUG] Processing direct JSON line: {line}");
                _useFramedOutput = false; // Client uses unframed input, so use unframed output
                
                var request = ParseJsonRequest(line);
                if (request != null)
                {
                    Console.Error.WriteLine($"[DEBUG] Successfully parsed direct request: {request.Method}");
                    yield return request;
                }
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                Console.Error.WriteLine($"[DEBUG] Ignoring unrecognized line: {line}");
            }
        }
    }

    private static JsonRpcRequest? ParseJsonRequest(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<JsonRpcRequest>(json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[DEBUG] Failed to parse JSON: {ex.Message}");
            return null;
        }
    }

    // Write a JSON-RPC response to stdout (framed or unframed based on input format)
    public static async Task WriteResponseAsync(object response)
    {
        var json = JsonSerializer.Serialize(response);
        
        if (_useFramedOutput)
        {
            var length = System.Text.Encoding.UTF8.GetByteCount(json);
            Console.Error.WriteLine($"[DEBUG] Sending framed response: Content-Length: {length}");
            Console.Error.WriteLine($"[DEBUG] JSON: {json}");
            
            await Console.Out.WriteAsync($"Content-Length: {length}\n\n");
            await Console.Out.WriteAsync(json);
        }
        else
        {
            Console.Error.WriteLine($"[DEBUG] Sending unframed response: {json}");
            await Console.Out.WriteLineAsync(json);
        }
        
        await Console.Out.FlushAsync();
    }
}
