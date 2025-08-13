# test-useful-api-initialize.ps1

# Construct a proper JSON-RPC 2.0 initialize request
$json = @{
    jsonrpc = "2.0"
    id = 1
    method = "initialize"
    params = @{}
} | ConvertTo-Json -Compress

# Calculate the correct Content-Length (in bytes)
$bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
$contentLength = $bytes.Length

# Create the full MCP-framed request
$request = "Content-Length: $contentLength`r`n`r`n$json"

# Show the request for debug
Write-Host "`n🔁 Sending 'initialize' request:"
Write-Host $request
Write-Host ""

# Pipe it into the Docker container running your MCP server
$request | docker run --rm -i useful-api:1.0 --mcp
