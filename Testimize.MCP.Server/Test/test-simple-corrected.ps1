# test-simple-corrected.ps1 - Simple test with corrected format using full parameter names
Write-Host "?? Testing Corrected Format with Full Parameter Names" -ForegroundColor Green

$request = @{
    jsonrpc = "2.0"
    id = 1
    method = "tools/call"
    params = @{
        name = "generate_test_cases"
        arguments = @{
            parameters = @(
                @{
                    ParameterType = "TextDataParameter"
                    PreciseMode = $false
                    MinBoundary = 3
                    MaxBoundary = 20
                    IncludeBoundaryValues = $true
                    AllowValidEquivalenceClasses = $true
                    AllowInvalidEquivalenceClasses = $true
                },
                @{
                    ParameterType = "EmailDataParameter"
                    PreciseMode = $false
                    MinBoundary = 6
                    MaxBoundary = 50
                    IncludeBoundaryValues = $true
                    AllowValidEquivalenceClasses = $true
                    AllowInvalidEquivalenceClasses = $true
                },
                @{
                    ParameterType = "UrlDataParameter"
                    PreciseMode = $false
                    IncludeBoundaryValues = $true
                    AllowValidEquivalenceClasses = $true
                    AllowInvalidEquivalenceClasses = $true
                },
                @{
                    ParameterType = "SingleSelectDataParameter"
                    PreciseMode = $false
                    Options = @("United States", "France", "Germany")
                    IncludeBoundaryValues = $true
                    AllowValidEquivalenceClasses = $true
                    AllowInvalidEquivalenceClasses = $true
                }
            )
            settings = @{
                Mode = 4
                TestCaseCategory = 0
                MethodName = "CorrectedFormatTest"
                ABCSettings = @{
                    TotalPopulationGenerations = 50
                    MutationRate = 0.4
                    FinalPopulationSelectionRatio = 0.5
                    EliteSelectionRatio = 0.3
                    OnlookerSelectionRatio = 0.1
                    ScoutSelectionRatio = 0.3
                    EnableOnlookerSelection = $true
                    EnableScoutPhase = $true
                    EnforceMutationUniqueness = $true
                    StagnationThresholdPercentage = 0.75
                    CoolingRate = 0.95
                    AllowMultipleInvalidInputs = $false
                    Seed = 42
                }
            }
        }
    }
} | ConvertTo-Json -Compress -Depth 10

Write-Host "Testing with corrected format using full parameter names..."
Write-Host $request

try {
    $response = $request | dotnet run --project ../Testimize.MCP.Server.csproj -- --mcp 2>&1
    Write-Host "? Response: $response" -ForegroundColor Green
} catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
}