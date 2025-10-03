# test-useful-api-mcp.ps1 - Comprehensive MCP Server Test
# Tests the Useful API MCP server with VS Code-compatible JSON-RPC messages

Write-Host "🧪 Testing Useful API MCP Server" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# Function to test a JSON-RPC method
function Test-McpMethod {
    param(
        [string]$Method,
        [hashtable]$Params = @{},
        [int]$Id = 1,
        [string]$Description
    )
    
    Write-Host "`n📋 Testing: $Description" -ForegroundColor Yellow
    
    # Create JSON-RPC request (unframed, as VS Code sends)
    $request = @{
        jsonrpc = "2.0"
        id = $Id
        method = $Method
        params = $Params
    } | ConvertTo-Json -Compress -Depth 10
    
    Write-Host "   Request: $request" -ForegroundColor Cyan
    
    try {
        # Send request to MCP server and capture response
        $response = $request | dotnet run --project ../Testimize.MCP.Server.csproj -- --mcp 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Response: $response" -ForegroundColor Green
            return $true
        } else {
            Write-Host "   ❌ Error (Exit code: $LASTEXITCODE): $response" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "   ❌ Exception: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Test cases
$tests = @(
    @{
        Method = "initialize"
        Params = @{
            protocolVersion = "2025-06-18"
            capabilities = @{
                roots = @{ listChanged = $true }
                sampling = @{
                    enabled = $true
                    interval = 5000
                }
                elicitation = @{
                    enabled = $true
                    params = @{
                        maxDepth = 3
                    }
                }
            }
            clientInfo = @{
                name = "Test Client"
                version = "1.0.0"
            }
        }
        Description = "MCP Initialize"
    },
    @{
        Method = "tools/list"
        Params = @{
            includeUnavailable = $true
        }
        Description = "List All Tools (including unavailable ones)"
    },
    @{
        Method = "tools/call"
        Params = @{
            name = "health_check"
            arguments = @{
                verbose = $true
            }
        }
        Description = "Call Health Check Tool (verbose)"
    },
    @{
        Method = "tools/call"
        Params = @{
            name = "get_time"
            arguments = @{
                utc = $true
            }
        }
        Description = "Call Get Time Tool (UTC)"
    },
    @{
        Method = "tools/call"
        Params = @{
            name = "generate_guid"
            arguments = @{
                prefix = "test-"
            }
        }
        Description = "Call Generate GUID Tool with prefix"
    },
    @{
        Method = "tools/call"
        Params = @{
            name = "generate_test_cases"
            arguments = @{
                parameters = @(
                    @{
                        ParameterType = "Text"
                        PreciseMode = $true
                        PreciseTestValues = @(
                            @{
                                Value = "Test Value"
                                Category = "Valid"
                            },
                            @{
                                Value = ""
                                Category = "Invalid"
                                ExpectedInvalidMessage = "Required field"
                            }
                        )
                    }
                )
                settings = @{
                    Mode = 1
                    TestCaseCategory = 0
                    MethodName = "TestMethod"
                }
            }
        }
        Description = "Call Generate Test Cases Tool (Precise Mode)"
    },
    @{
        Method = "tools/call"
        Params = @{
            name = "generate_test_cases"
            arguments = @{
                parameters = @(
                    @{
                        ParameterType = "Integer"
                        PreciseMode = $false
                        MinBoundary = 1
                        MaxBoundary = 100
                        IncludeBoundaryValues = $true
                        AllowValidEquivalenceClasses = $true
                        AllowInvalidEquivalenceClasses = $true
                    }
                )
                settings = @{
                    Mode = 1
                    TestCaseCategory = 0
                    MethodName = "ExploratoryTest"
                }
            }
        }
        Description = "Call Generate Test Cases Tool (Exploratory Mode)"
    },
    @{
        Method = "tools/call"
        Params = @{
            name = "generate_test_cases"
            arguments = @{
                parameters = @(
                    @{
                        ParameterType = "Email"
                        PreciseMode = $false
                        MinBoundary = 6
                        MaxBoundary = 50
                        IncludeBoundaryValues = $true
                        AllowValidEquivalenceClasses = $true
                        AllowInvalidEquivalenceClasses = $true
                    },
                    @{
                        ParameterType = "Password"
                        PreciseMode = $false
                        MinBoundary = 8
                        MaxBoundary = 20
                        IncludeBoundaryValues = $true
                        AllowValidEquivalenceClasses = $true
                        AllowInvalidEquivalenceClasses = $true
                    }
                )
                settings = @{
                    Mode = 4
                    TestCaseCategory = 2
                    MethodName = "RegisterUser"
                    ABCSettings = @{
                        TotalPopulationGenerations = 50
                        MutationRate = 0.4
                        AllowMultipleInvalidInputs = $false
                        Seed = 42
                    }
                }
            }
        }
        Description = "Call Generate Test Cases Tool (ABC Mode)"
    },
    @{
        Method = "tools/call"
        Params = @{
            name = "generate_test_cases"
            arguments = @{
                parameters = @(
                    @{
                        ParameterType = "MultiSelect"
                        PreciseMode = $true
                        Multiple = $true
                        Options = @("English", "French", "German", "Spanish")
                        PreciseTestValues = @(
                            @{
                                Value = @("English")
                                Category = "Valid"
                            },
                            @{
                                Value = @("English", "French")
                                Category = "Valid"
                            },
                            @{
                                Value = @()
                                Category = "Invalid"
                                ExpectedInvalidMessage = "At least one selection required"
                            }
                        )
                    }
                )
                settings = @{
                    Mode = 1
                    TestCaseCategory = 0
                    MethodName = "LanguageSelection"
                }
            }
        }
        Description = "Call Generate Test Cases Tool (MultiSelect)"
    },
    @{
        Method = "tools/call"
        Params = @{
            name = "unknown_tool"
            arguments = @{
            }
        }
        Description = "Call Unknown Tool (should fail)"
    },
    @{
        Method = "unknown/method"
        Params = @{
            someParam = "value"
        }
        Description = "Unknown Method with param (should fail)"
    }
)

# Execute tests
$passed = 0
$total = $tests.Count

foreach ($test in $tests) {
    $result = Test-McpMethod -Method $test.Method -Params $test.Params -Description $test.Description
    if ($result) { $passed++ }
    Start-Sleep -Milliseconds 500  # Brief pause between tests
}

# Summary
Write-Host "`n📊 Test Summary" -ForegroundColor Green
Write-Host "===============" -ForegroundColor Green
Write-Host "Passed: $passed/$total" -ForegroundColor $(if ($passed -eq $total) { "Green" } else { "Yellow" })

if ($passed -eq $total) {
    Write-Host "🎉 All tests passed! MCP server is working correctly." -ForegroundColor Green
    exit 0
} else {
    Write-Host "⚠️  Some tests failed. Check the output above for details." -ForegroundColor Yellow
    exit 1
}
