# Testimize

> Smart and scalable test data generation engine for automated testing in .NET.

[![NuGet](https://img.shields.io/nuget/v/Testimize.svg)](https://www.nuget.org/packages/Testimize)
[![License](https://img.shields.io/github/license/your-org/testimize)](LICENSE)

---

## ✨ Why Testimize?

Testimize helps you design **high-quality, optimized test cases** for automated testing with minimal effort. It supports:

- ✅ Boundary Value Analysis (BVA)
- ✅ Pairwise Combinatorial Testing
- ✅ Heuristic Optimization (ABC Algorithm)
- ✅ Fully controlled **Precise Mode** for stable CI/CD runs
- ✅ Fluent and expressive DSL for defining test inputs
- ✅ Support for expected validation messages, JSON configs, NUnit output
- ✅ Centralized config with support for locales, equivalence classes, and precision

---

## 🚀 Key Features

| Feature                        | Description                                                                 |
|-------------------------------|-----------------------------------------------------------------------------|
| 🧪 Precise Mode                | Full control of test inputs + validation messages                          |
| 📊 Exploratory Mode           | Metaheuristic test generation (ABC algorithm) for bug discovery            |
| ⚡ Pairwise Support            | Efficient test case coverage via pairwise combination                     |
| 🔧 Output Generators          | Supports NUnit `[TestCase]`, `[TestCaseSource]`, CSV, JSON, and more       |
| 🌍 Configurable Locales       | Faker-based localized values (en, fr, de, bg, etc.)                        |
| 🧠 Smart Equivalence Classes  | Valid and invalid sample inputs for every input type                       |
| ✅ Designed for TDD & CI/CD   | Precise control for TDD, stability for CI builds                           |

---

## 🛠️ Installation

```bash
dotnet add package Testimize
