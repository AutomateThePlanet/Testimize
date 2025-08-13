# setup-useful-api.ps1 - Complete setup script for Useful API

Write-Host "?? Setting up Useful API (HTTP + MCP)" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green

# Function to check if command exists
function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}

# Check prerequisites
Write-Host "`n?? Checking Prerequisites..." -ForegroundColor Yellow

$prerequisites = @(
    @{ Name = ".NET 8 SDK"; Command = "dotnet"; Check = { (dotnet --version) -like "8.*" } },
    @{ Name = "Docker"; Command = "docker"; Check = { docker --version } }
)

$missingPrereqs = @()
foreach ($prereq in $prerequisites) {
    if (Test-Command $prereq.Command) {
        try {
            $result = & $prereq.Check
            Write-Host "   ? $($prereq.Name): Found" -ForegroundColor Green
        }
        catch {
            Write-Host "   ? $($prereq.Name): Installed but not working properly" -ForegroundColor Red
            $missingPrereqs += $prereq.Name
        }
    }
    else {
        Write-Host "   ? $($prereq.Name): Not found" -ForegroundColor Red
        $missingPrereqs += $prereq.Name
    }
}

if ($missingPrereqs.Count -gt 0) {
    Write-Host "`n??  Missing prerequisites: $($missingPrereqs -join ', ')" -ForegroundColor Red
    Write-Host "Please install the missing prerequisites and run this script again." -ForegroundColor Red
    exit 1
}

# Build and test .NET project
Write-Host "`n?? Building .NET Project..." -ForegroundColor Yellow
try {
    dotnet build --configuration Release
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? .NET build successful" -ForegroundColor Green
    } else {
        throw "Build failed with exit code $LASTEXITCODE"
    }
}
catch {
    Write-Host "   ? .NET build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Build Docker image
Write-Host "`n?? Building Docker Image..." -ForegroundColor Yellow
try {
    docker build -t useful-api:1.0 .
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Docker build successful" -ForegroundColor Green
    } else {
        throw "Docker build failed with exit code $LASTEXITCODE"
    }
}
catch {
    Write-Host "   ? Docker build failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   ?? Make sure Docker Desktop is running" -ForegroundColor Yellow
}

# Test HTTP API (.NET)
Write-Host "`n?? Testing HTTP API (.NET mode)..." -ForegroundColor Yellow
$dotnetJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    dotnet run --no-build --configuration Release
}

Start-Sleep -Seconds 5  # Wait for startup

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/health" -TimeoutSec 10
    Write-Host "   ? HTTP API (.NET): $($response.status)" -ForegroundColor Green
}
catch {
    Write-Host "   ??  HTTP API (.NET): Could not test (may still work)" -ForegroundColor Yellow
}
finally {
    Stop-Job $dotnetJob -ErrorAction SilentlyContinue
    Remove-Job $dotnetJob -ErrorAction SilentlyContinue
}

# Test HTTP API (Docker)
if (docker images -q useful-api:1.0) {
    Write-Host "`n?? Testing HTTP API (Docker mode)..." -ForegroundColor Yellow
    $dockerJob = Start-Job -ScriptBlock {
        docker run --rm -p 8088:8088 useful-api:1.0
    }
    
    Start-Sleep -Seconds 5  # Wait for startup
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:8088/health" -TimeoutSec 10
        Write-Host "   ? HTTP API (Docker): $($response.status)" -ForegroundColor Green
    }
    catch {
        Write-Host "   ??  HTTP API (Docker): Could not test (may still work)" -ForegroundColor Yellow
    }
    finally {
        Stop-Job $dockerJob -ErrorAction SilentlyContinue
        Remove-Job $dockerJob -ErrorAction SilentlyContinue
        docker stop (docker ps -q --filter ancestor=useful-api:1.0) 2>$null
    }
}

# Display configuration information
Write-Host "`n?? Configuration Information" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green

Write-Host "`nFor VS Code GitHub Copilot MCP configuration:" -ForegroundColor Cyan
Write-Host "Option 1 (.NET Development - Recommended):" -ForegroundColor Yellow
Write-Host @"
{
  "useful-api": {
    "type": "stdio",
    "command": "dotnet",
    "args": ["run", "--project", "$((Get-Location).Path)", "--", "--mcp"]
  }
}
"@ -ForegroundColor White

Write-Host "`nOption 2 (Docker):" -ForegroundColor Yellow
Write-Host @"
{
  "useful-api": {
    "type": "stdio",
    "command": "docker",
    "args": ["run", "--rm", "-i", "useful-api:1.0", "--mcp"]
  }
}
"@ -ForegroundColor White

Write-Host "`n?? Usage Commands" -ForegroundColor Green
Write-Host "=================" -ForegroundColor Green
Write-Host "HTTP API (.NET):    dotnet run" -ForegroundColor White
Write-Host "HTTP API (Docker):  docker run --rm -p 8088:8088 useful-api:1.0" -ForegroundColor White
Write-Host "MCP Server (.NET):  dotnet run -- --mcp" -ForegroundColor White
Write-Host "MCP Server (Docker): docker run --rm -i useful-api:1.0 --mcp" -ForegroundColor White

Write-Host "`n?? Setup complete!" -ForegroundColor Green
Write-Host "You can now use the Useful API in both HTTP and MCP modes." -ForegroundColor Green