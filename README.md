# Testimize

> 🚀 Smart and scalable test data generation engine for .NET automated testing.

[![NuGet](https://img.shields.io/nuget/v/Testimize.svg)](https://www.nuget.org/packages/Testimize)
[![License](https://img.shields.io/github/license/testimize/testimize)](LICENSE)

---

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
var testCases = TestimizeEngine
    .Configure(
        parameters => parameters
            .AddEmail(p => p
                .Valid("test@mail.com")
                .Invalid("bad-email").WithExpectedMessage("Invalid email")),
        config => {
            config.Mode = TestGenerationMode.Precise;
            config.OutputGenerator = new NUnitTestCaseAttributeOutputGenerator();
        })
    .Generate("EmailValidationTests");
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
config.Mode = TestGenerationMode.Exploratory; // Alias for ABC
config.AbcGenerationConfig = new ABCGenerationConfig {
    TotalPopulationGenerations = 20,
    MutationRate = 0.3,
    OutputGenerator = new NUnitTestCaseSourceOutputGenerator()
};
```

✅ **Best for:** finding edge cases, fuzzing, unknown test spaces  
✅ **Not ideal for:** stable CI/CD (unless deterministic seed used)

---

## 🔧 Configuration via `testimizeSettings.json`

```json
{
  "testValueGenerationSettings": {
    "seed": 12345,
    "locale": "en",
    "includeBoundaryValues": true,
    "allowValidEquivalenceClasses": true,
    "allowInvalidEquivalenceClasses": true,
    "abcGenerationConfig": {
      "totalPopulationGenerations": 20,
      "mutationRate": 0.3,
      "allowMultipleInvalidInputs": false,
      "outputGenerator": "NUnit"
    }
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
