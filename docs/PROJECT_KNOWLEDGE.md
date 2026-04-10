# Testimize — Project Knowledge

A shareable reference for teams integrating **Testimize** (the .NET test-data generation library) and the **Testimize MCP Server** (the Model Context Protocol front-end) into their own repositories and workflows.

---

## 1. What Testimize Is

Testimize is a .NET 8 library that generates *optimized* test cases from a declarative description of input parameters. It replaces handwritten `[TestCase]` lists and ad-hoc data builders with a deterministic, coverage-aware pipeline.

It supports three generation strategies:

| Strategy | When to use |
|---|---|
| **Combinatorial** | Small parameter spaces where full coverage is feasible. |
| **Pairwise** | Medium spaces — covers all pairs of parameter values with a fraction of the tests. |
| **Hybrid Artificial Bee Colony (ABC)** | Large or heavily-validated spaces — heuristic optimization biased toward boundary and invalid-class coverage. |

Two operating modes:

- **Exploratory Mode** — you describe boundaries/ranges, Testimize generates realistic values via Bogus.
- **Precise Mode** — you specify explicit valid/invalid values and expected error messages.

Outputs plug into **NUnit**, **xUnit**, **MSTest**, **JSON**, and **CSV**.

---

## 2. Consuming Testimize in Another Project

### 2.1 Install the core library

```bash
dotnet add package Testimize
```

Target framework: `net8.0`.

### 2.2 Minimal usage

```csharp
var cases = TestimizeEngine.Configure(
    p => p
        .AddText(t => t.Valid("hello").Invalid("").WithoutMessage())
        .AddEmail(e => e.Valid("a@b.co").Invalid("not-an-email").WithoutMessage()),
    s =>
    {
        s.Mode = TestGenerationMode.Pairwise;
        s.TestCaseCategory = TestCaseCategory.Validation;
    }).Generate();
```

### 2.3 Wiring into a test framework

```csharp
public static List<TestCase> Provider() => /* TestimizeEngine.Configure(...).Generate() */;

[Test]
[TestimizeGeneratedTestCases(nameof(Provider))]
public void Submit_Form(string name, string email) { /* ... */ }
```

Equivalent attributes exist for MSTest and xUnit.

### 2.4 Configuration file

Drop a `testimizeSettings.json` next to your test assembly to override ABC tuning, locale, default boundaries, and category weights without recompiling. A sample ships in the NuGet package under `contentFiles/cs/any`.

### 2.5 Key extension points

- `IInputParameter` / `DataParameter<T>` — add a new input type.
- `IDataProviderStrategy` — plug in a custom value generator.
- `ITestCaseOutputGenerator` — emit a new output format.

---

## 3. Architecture Cheat Sheet

```
TestimizeEngine.Configure(...)
        │
        ▼
TestSuiteBuilder ──► ITestCaseGenerator  (Combinatorial | Pairwise | HybridABC)
        │                   │
        │                   ▼
        │          IDataProviderStrategy  (Boundary | EquivalenceOnly)
        ▼
ITestCaseOutputGenerator  (NUnit | xUnit | MSTest | JSON | CSV)
```

Design patterns in use: Strategy (data providers), Builder (inputs/values), Factory (parameter + faker creation), Fluent Interface (public API), DI (MCP server).

---

## 4. The MCP Server

`Testimize.MCP.Server` is a dual-mode ASP.NET Core 8 app:

- **HTTP mode** — REST + Swagger at `http://localhost:5000/swagger`.
- **MCP stdio mode** — JSON-RPC over stdin/stdout so AI assistants (Claude Code, GitHub Copilot, Cursor, etc.) can call Testimize as *tools*.

### 4.1 Exposed MCP tools

| Tool | Purpose |
|---|---|
| `health_check` | Liveness / version probe. |
| `get_time` | Current UTC time. |
| `generate_guid` | Random GUID helper. |
| `generate_hybrid_test_cases` | Run the ABC generator from a parameter spec. |
| `generate_pairwise_test_cases` | Run the pairwise generator from a parameter spec. |
| `configure_testimize_settings` | Push an ABC settings object into the running server. |
| `get_testimize_settings` | Read back the active settings. |

### 4.2 Running locally from source

```bash
# HTTP + Swagger
dotnet run --project Testimize.MCP.Server

# MCP stdio (for an AI client)
dotnet run --project Testimize.MCP.Server -- --mcp
```

### 4.3 Running as a packaged tool (recommended for consumers)

Once published, the MCP server ships as a **.NET global tool** on NuGet:

```bash
dotnet tool install --global Testimize.MCP.Server
testimize-mcp --mcp          # stdio mode
testimize-mcp                # HTTP mode
```

### 4.4 Client configuration

**Claude Code** (`.mcp.json` or user settings):

```json
{
  "mcpServers": {
    "testimize": {
      "command": "testimize-mcp",
      "args": ["--mcp"]
    }
  }
}
```

**VS Code / GitHub Copilot** (`settings.json`):

```json
{
  "mcp.servers": {
    "testimize": {
      "type": "stdio",
      "command": "testimize-mcp",
      "args": ["--mcp"]
    }
  }
}
```

**From source (no install):**

```json
{
  "testimize": {
    "type": "stdio",
    "command": "dotnet",
    "args": ["run", "--project", "path/to/Testimize.MCP.Server", "--", "--mcp"]
  }
}
```

**Docker:**

```json
{
  "testimize": {
    "type": "stdio",
    "command": "docker",
    "args": ["run", "--rm", "-i", "testimize-mcp-server:latest", "--mcp"]
  }
}
```

### 4.5 Typical AI-assistant workflows

- *"Generate 30 pairwise test cases for a user-registration form with these fields…"* → `generate_pairwise_test_cases`.
- *"Use ABC to find boundary-violating inputs for this endpoint spec."* → `generate_hybrid_test_cases`.
- *"Raise the ABC colony size to 60 and re-run."* → `configure_testimize_settings` then re-call a generator.

---

## 5. Versioning & Release

- Core library and MCP server ship as **independent NuGet packages** with independent versions.
- Releases are cut by pushing a Git tag:
  - `v*.*.*` → publishes `Testimize`.
  - `mcp-v*.*.*` → publishes `Testimize.MCP.Server`.
- Both workflows live under `.github/workflows/` and require the `NUGETKEY` repository secret.

---

## 6. Useful Links

- Repository: https://github.com/AutomateThePlanet/Testimize
- NuGet (core): https://www.nuget.org/packages/Testimize
- NuGet (MCP): https://www.nuget.org/packages/Testimize.MCP.Server
- License: Apache-2.0 (see `LICENSE`)
