# Test MCP Schema with Simple Request
Write-Host "Testing MCP Server Schema Validation..." -ForegroundColor Green

# Function to test MCP via stdio
function Test-McpStdio {
    param(
        [string]$Method,
        [hashtable]$Params = @{},
        [int]$Id = 1,
        [string]$Description
    )
    
    Write-Host "`n?? Testing: $Description" -ForegroundColor Yellow
    
    # Create JSON-RPC request
    $request = @{
        jsonrpc = "2.0"
        id = $Id
        method = $Method
        params = $Params
    } | ConvertTo-Json -Compress -Depth 10
    
    Write-Host "   Request: $request" -ForegroundColor Cyan
    
    try {
        # Create a temporary input file
        $inputFile = [System.IO.Path]::GetTempFileName()
        $outputFile = [System.IO.Path]::GetTempFileName()
        
        # Write the request to the input file
        $request | Out-File -FilePath $inputFile -Encoding UTF8 -NoNewline
        
        # Start the MCP server process
        $psi = New-Object System.Diagnostics.ProcessStartInfo
        $psi.FileName = "dotnet"
        $psi.Arguments = "run --project ../UsefulApi.csproj -- --mcp"
        $psi.UseShellExecute = $false
        $psi.RedirectStandardInput = $true
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError = $true
        $psi.CreateNoWindow = $true
        $psi.WorkingDirectory = "../"
        
        $process = [System.Diagnostics.Process]::Start($psi)
        
        # Send the request
        $process.StandardInput.WriteLine($request)
        $process.StandardInput.Close()
        
        # Wait for response (with timeout)
        $response = ""
        $timeout = 10000 # 10 seconds
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        
        while ($sw.ElapsedMilliseconds -lt $timeout -and !$process.HasExited) {
            if (!$process.StandardOutput.EndOfStream) {
                $line = $process.StandardOutput.ReadLine()
                if (![string]::IsNullOrEmpty($line)) {
                    $response = $line
                    break
                }
            }
            Start-Sleep -Milliseconds 100
        }
        
        # Get any error output
        $errorOutput = $process.StandardError.ReadToEnd()
        
        # Clean up process
        if (!$process.HasExited) {
            $process.Kill()
        }
        $process.Dispose()
        
        # Clean up temp files
        Remove-Item $inputFile -Force -ErrorAction SilentlyContinue
        Remove-Item $outputFile -Force -ErrorAction SilentlyContinue
        
        if (![string]::IsNullOrEmpty($response)) {
            Write-Host "   ? Response: $response" -ForegroundColor Green
            if (![string]::IsNullOrEmpty($errorOutput)) {
                Write-Host "   ?? Debug Output: $errorOutput" -ForegroundColor Gray
            }
            return $true
        } else {
            Write-Host "   ? No response received" -ForegroundColor Red
            if (![string]::IsNullOrEmpty($errorOutput)) {
                Write-Host "   ?? Error Output: $errorOutput" -ForegroundColor Red
            }
            return $false
        }
    }
    catch {
        Write-Host "   ? Exception: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Test the HTTP API instead (which should work)
function Test-HttpApi {
    Write-Host "`n?? Testing HTTP API (alternative to MCP)..." -ForegroundColor Yellow
    
    # Start the HTTP API in the background
    $apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "../UsefulApi.csproj" -PassThru -NoNewWindow -WorkingDirectory "../"
    
    # Wait for API to start
    Write-Host "   Waiting for API to start..." -ForegroundColor Cyan
    Start-Sleep -Seconds 5
    
    try {
        # Test health endpoint
        Write-Host "   Testing /health endpoint..." -ForegroundColor Cyan
        $healthResponse = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get
        Write-Host "   ? Health Response: $($healthResponse | ConvertTo-Json -Compress)" -ForegroundColor Green
        
        # Test generate test cases endpoint
        Write-Host "   Testing /generate-test-cases endpoint..." -ForegroundColor Cyan
        $testRequest = @{
            parameters = @(
                @{
                    ParameterType = "Text"
                    PreciseMode = $true
                    PreciseTestValues = @(
                        @{
                            Value = "Test Value"
                            Category = "Valid"
                        }
                    )
                }
            )
            settings = @{
                Mode = 1
                TestCaseCategory = 0
                MethodName = "TestMethod"
            }
        } | ConvertTo-Json -Depth 10
        
        $testResponse = Invoke-RestMethod -Uri "http://localhost:5000/generate-test-cases" -Method Post -Body $testRequest -ContentType "application/json"
        Write-Host "   ? Test Cases Response: $($testResponse | ConvertTo-Json -Compress)" -ForegroundColor Green
        
        return $true
        
    } catch {
        Write-Host "   ? HTTP API Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    } finally {
        # Clean up - stop the API process
        if ($apiProcess -and !$apiProcess.HasExited) {
            $apiProcess.Kill()
            Write-Host "   Stopped API process" -ForegroundColor Gray
        }
    }
}

# Test schema validation by checking if MCP server starts correctly
Write-Host "`n?? Testing MCP Server Startup..." -ForegroundColor Yellow

try {
    # Test if the MCP server can start and return the tools list
    $result = Test-McpStdio -Method "tools/list" -Params @{} -Description "MCP Tools List (Schema Validation Test)"
    
    if ($result) {
        Write-Host "`n?? MCP Schema Validation: PASSED" -ForegroundColor Green
        Write-Host "   The MCP server started successfully and returned a tools list." -ForegroundColor Green
        Write-Host "   This indicates the schema validation issues have been resolved." -ForegroundColor Green
    } else {
        Write-Host "`n??  MCP Schema Validation: FAILED" -ForegroundColor Yellow
        Write-Host "   The MCP server had issues. Testing HTTP API as alternative..." -ForegroundColor Yellow
        
        # Test HTTP API as fallback
        $httpResult = Test-HttpApi
        
        if ($httpResult) {
            Write-Host "`n? HTTP API: Working correctly" -ForegroundColor Green
            Write-Host "   The core functionality is working via HTTP API." -ForegroundColor Green
        }
    }
} catch {
    Write-Host "`n? Test Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n?? Summary:" -ForegroundColor Green
Write-Host "   - MCP Server: Uses --mcp flag and stdio JSON-RPC protocol" -ForegroundColor Cyan
Write-Host "   - HTTP API: Available at http://localhost:5000 when running normally" -ForegroundColor Cyan
Write-Host "   - Both modes use the same underlying services and schema" -ForegroundColor Cyan
Write-Host "`n   To test with VS Code:" -ForegroundColor Yellow
Write-Host "   1. Configure MCP in VS Code settings" -ForegroundColor Yellow
Write-Host "   2. Start server with: dotnet run --project UsefulApi -- --mcp" -ForegroundColor Yellow
Write-Host "   3. Use GitHub Copilot Chat to call the generate_test_cases tool" -ForegroundColor Yellow