# Testimize

> 🚀 Smart and scalable test data generation engine for .NET automated testing.

[![NuGet](https://img.shields.io/nuget/v/Testimize.svg)](https://www.nuget.org/packages/Testimize)
[![License](https://img.shields.io/github/license/testimize/testimize)](LICENSE)

---
<p align="center">
  <img src="https://raw.githubusercontent.com/AutomateThePlanet/Testimize/85d45a39841b841edce0b51d9a998a92672fe719/testimize_logo_github.png" width="140" alt="Testimize logo" />
</p>

## ✨ Why Testimize?

Testimize helps you design **high-quality, optimized test cases** for automated testing with minimal effort.

It supports:

- ✅ Boundary Value Analysis (BVA)
- ✅ Pairwise Test Case Generation
- ✅ Heuristic Optimization (Artificial Bee Colony Algorithm)
- ✅ Fully-controlled **Precise Mode** for CI/CD and validation
- ✅ Rich, extensible DSL for defining valid/invalid inputs and expected error messages
- ✅ Configuration via JSON files and localization support

---

## 🛠️ Installation

```bash
dotnet add package Testimize
```

---

## 📐 Modes of Generation

Testimize offers **three powerful modes** of test case generation:

### ✅ 1. Precise Mode

For **CI/CD**, strict validations, and known input sets. Allows full control over:

- Specific valid/invalid values
- Expected validation messages
- Boundary values

```csharp
TestimizeEngine
.Configure(
    parameters => parameters
        .AddSelect(s => s
            .Valid("US")
            .Valid("BG")
            .Valid("FR")
            .Invalid("XX").WithExpectedMessage("Country code is invalid")
            .Invalid("U1").WithExpectedMessage("Country code must contain only letters")
            .Invalid("").WithExpectedMessage("Country code is required"))
        .AddSelect(s => s
            .Valid("en")
            .Valid("fr")
            .Valid("de")
            .Invalid("zz").WithExpectedMessage("Language code not supported")
            .Invalid("123").WithExpectedMessage("Language code must be alphabetic"))
        .AddSelect(s => s
            .Valid("EU")
            .Valid("AF")
            .Valid("AS")
            .Invalid("999").WithExpectedMessage("Continent code cannot be numeric")
            .Invalid("X").WithExpectedMessage("Continent code too short")
            .Invalid("").WithExpectedMessage("Continent code is required")),
    settings =>
    {
        settings.Mode = TestGenerationMode.HybridArtificialBeeColony;

        settings.ABCSettings = new ABCGenerationSettings
        {
            TotalPopulationGenerations = 20,
            MutationRate = 0.3,
            FinalPopulationSelectionRatio = 0.5,
            EliteSelectionRatio = 0.5,
            OnlookerSelectionRatio = 0.1,
            ScoutSelectionRatio = 0.3,
            EnableOnlookerSelection = true,
            EnableScoutPhase = false,
            EnforceMutationUniqueness = true,
            StagnationThresholdPercentage = 0.75,
            CoolingRate = 0.95,
            AllowMultipleInvalidInputs = false,
            OutputGenerator = new NUnitTestCaseAttributeOutputGenerator()
        };
    })
.Generate();
```

✅ **Best for:** validation rules, known test inputs, automation suites  
✅ **Supports:** `[TestCase]`, `[TestCaseSource]`, CSV, JSON

---

### 🔄 2. Pairwise Mode

Generates a minimal set of test cases covering **every pairwise combination** of parameters.

```csharp
config.Mode = TestGenerationMode.Pairwise;
```

✅ **Best for:** wide input coverage with low execution time  
✅ **Supports:** Output generators + category filtering  
✅ **Stateless:** No heuristics or randomness required

---

### 🧠 3. Exploratory Mode (ABC Algorithm)

Uses a **metaheuristic algorithm** to explore input combinations based on:

- Fitness functions
- Mutation rate
- Heuristic selection strategies

```csharp
public static List<IInputParameter> ABCGeneratedTestParameters() =>
TestimizeInputBuilder
    .Start()
    .AddSingleSelect(s => s
        .Valid("US")
        .Valid("BG")
        .Valid("FR")
        .Invalid("XX").WithoutMessage()
        .Invalid("U1").WithoutMessage()
        .Invalid("").WithoutMessage())
    .AddSingleSelect(s => s
        .Valid("en")
        .Valid("fr")
        .Valid("de")
        .Invalid("zz").WithoutMessage()
        .Invalid("123").WithoutMessage())
    .AddSingleSelect(s => s
        .Valid("EU")
        .Valid("AF")
        .Valid("AS")
        .Invalid("999").WithoutMessage()
        .Invalid("X").WithoutMessage()
        .Invalid("").WithoutMessage())
    .Build();

[Test]
[ABCTestCaseSource(nameof(ABCGeneratedTestParameters), TestCaseCategory.Validation)]
public void QueryCountry_WithLanguageAndContinentFilters_ShouldReturn200(
    string countryCode, string languageCode, string continentCode)
{
    // your test logic here
}
```

✅ **Best for:** finding edge cases, fuzzing, unknown test spaces  
✅ **Not ideal for:** stable CI/CD (unless deterministic seed used)

---

## 🔧 Configuration via `testimizeSettings.json`

```json
{
  "testimizeSettings": {
    "seed": 12345,
    "locale": "en",
    "includeBoundaryValues": true,
    "allowValidEquivalenceClasses": true,
    "allowInvalidEquivalenceClasses": true,
    "abcGenerationSettings": {
      "totalPopulationGenerations": 20,
      "mutationRate": 0.3,
      "finalPopulationSelectionRatio": 0.5,
      "eliteSelectionRatio": 0.5,
      "onlookerSelectionRatio": 0.1,
      "scoutSelectionRatio": 0.3,
      "enableOnlookerSelection": true,
      "enableScoutPhase": false,
      "enforceMutationUniqueness": true,
      "stagnationThresholdPercentage": 0.75,
      "coolingRate": 0.95,
      "allowMultipleInvalidInputs": false
    },
    "inputTypeSettings": {
    //...
    }
}
```

---

## 🧩 Supported Input Types

- `Text`, `Email`, `Phone`, `Password`, `Username`, `URL`, `Address`
- `Integer`, `Decimal`, `Percentage`, `Boolean`
- `Date`, `Time`, `DateTime`, `Week`, `Month`
- `Currency`, `GeoCoordinate`, `Color`
- `SingleSelect`, `MultiSelect`

---

## 📦 Output Generators

| Class Name                              | Description                        |
|----------------------------------------|------------------------------------|
| `NUnitTestCaseAttributeOutputGenerator` | `[TestCase(...)]` attributes       |
| `NUnitTestCaseSourceOutputGenerator`    | `IEnumerable<object[]>` method     |
| `CsvTestCaseOutputGenerator`            | CSV output                         |
| `JsonTestCaseOutputGenerator`           | JSON test data output              |

---

## 🧪 Integration & Test Frameworks

✅ Primary support for:

- **NUnit**: with `[TestCase]` and `[TestCaseSource]` generation

🟡 Planned support:

- **xUnit**
- **MSTest**

---

## 🔮 Roadmap

- [ ] xUnit and MSTest support
- [ ] Java support
- [ ] Security testing parameters
- [ ] GitHub Action to generate data as part of CI

---

## 📚 Samples

See `/samples` for examples of using Testimize in:

- ✅ Unit tests
- ✅ Data-driven tests
- ✅ Exploratory test generation

---

## 👥 Contributors

Made with ❤️ by [@angelovstanton] and the Testimize community.

---

## 📜 License

Licensed under the [Apache License, Version 2.0](https://www.apache.org/licenses/LICENSE-2.0).
