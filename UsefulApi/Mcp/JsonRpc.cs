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
    // Read JSON-RPC requests from stdin with MCP framing
    public static async IAsyncEnumerable<JsonRpcRequest> ReadRequestsAsync()
    {
        var stdin = Console.OpenStandardInput();
        using var reader = new StreamReader(stdin);

        while (true)
        {
            string? line;
            int contentLength = 0;

            // Read headers
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
            {
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int len))
                    {
                        contentLength = len;
                    }
                }
            }

            if (contentLength <= 0)
                yield break;

            // Read the actual JSON payload
            var buffer = new char[contentLength];
            var read = 0;
            while (read < contentLength)
            {
                var n = await reader.ReadAsync(buffer, read, contentLength - read);
                if (n == 0) yield break;
                read += n;
            }

            var json = new string(buffer, 0, read);
            var request = JsonSerializer.Deserialize<JsonRpcRequest>(json);
            if (request != null)
                yield return request;
        }
    }

    // Write a framed JSON-RPC response to stdout
    public static async Task WriteResponseAsync(object response)
    {
        var json = JsonSerializer.Serialize(response);
        var length = System.Text.Encoding.UTF8.GetByteCount(json);
        await Console.Out.WriteAsync($"Content-Length: {length}\r\n\r\n");
        await Console.Out.WriteAsync(json);
        await Console.Out.FlushAsync();
    }
}
