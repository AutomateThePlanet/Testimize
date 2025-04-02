# Testimize 🧪

**Testimize** is a powerful .NET library for automated test case generation driven by advanced strategies like **Hybrid Artificial Bee Colony (ABC)**, **Pairwise**, and **Combinatorial** algorithms. It helps testers and developers **systematically test boundary conditions, equivalence classes**, and validation rules by generating effective test suites with minimal redundancy.

[![NuGet](https://img.shields.io/nuget/v/Testimize.svg)](https://www.nuget.org/packages/Testimize)
[![GitHub license](https://img.shields.io/github/license/AutomateThePlanet/Testimize?style=flat)](https://github.com/AutomateThePlanet/Testimize/blob/main/LICENSE)
[![CI](https://github.com/AutomateThePlanet/Testimize/actions/workflows/ci.yml/badge.svg)](https://github.com/AutomateThePlanet/Testimize/actions/workflows/ci.yml)

---
<p align="center">
  <img src="https://github.com/AutomateThePlanet/Testimize/blob/main/testimize_banner_beige_bg.png?raw=true" width="100%" alt="Testimize Banner" />
</p>

## ✨ Why Testimize?

Testimize helps you design **high-quality, optimized test cases** for automated testing with minimal effort.

It supports:

- ✅ Boundary Value Analysis (BVA)
- ✅ Pairwise Test Case Generation
- ✅ Heuristic Optimization (Artificial Bee Colony Algorithm)
- ✅ Supports **Exploratory Mode** and **Precise Mode**  
- ✅ Works for **unit tests**, **web UI**, **REST APIs**, **GraphQL**, and **mobile** testing 
- ✅ Targets **validation scenarios**, **form coverage**, **data-driven testing**, and more 
- ✅ Rich, extensible DSL for defining valid/invalid inputs and expected error messages
- ✅ Configuration via JSON files and localization support

---

## 🛠️ Installation

```bash
dotnet add package Testimize
```

---

## 📐 Modes of Operation

### 1. 🧠 **Exploratory Mode** (Default)
- Automatically generates realistic and varied test data.
- Users only specify **boundary values** and basic parameter configuration.
- Powered by built-in **Bogus-based** strategies and `TestimizeSettings` JSON config.
- Great for **initial coverage**, **TDD**, **form testing**, and discovering **missing validations**.

```csharp
public static List<TestCase> ConfigureEngine() =>
    TestimizeEngine.Configure(
        parameters => parameters
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
                .Invalid("").WithoutMessage()),
        settings =>
        {
            settings.Mode = TestGenerationMode.HybridArtificialBeeColony;
            settings.TestCaseCategory = TestCaseCategory.Validation;
        }).Generate();

[Test]
[TestimizeGeneratedTestCases(nameof(ConfigureEngine))]
public void QueryCountry_WithLanguageAndContinentFilters_ShouldReturn200(
    string countryCode, string languageCode, string continentCode)
{
    // your test logic
}
```

### 2. 🔄 **Precise Mode**
- Used when exact control over test values and expected messages is required.
- You define **valid**, **boundary-valid**, **boundary-invalid**, and **invalid** values explicitly.
- Ensures deterministic and meaningful regression coverage.

```csharp
[Test]
public void GenerateTests() =>
TestimizeEngine
    .Configure(
        parameters => parameters

            .AddText(t => t
                .Valid("Anton Angelov")
                .BoundaryValid("Ann")
                .BoundaryValid(new string('A', 20))
                .BoundaryInvalid("An")
                    .WithExpectedMessage("Full Name must be between 3 and 20 characters.")
                .BoundaryInvalid(new string('A', 21))
                    .WithExpectedMessage("Full Name must be between 3 and 20 characters.")
                .Invalid("")
                    .WithExpectedMessage("Full Name is required."))

            .AddEmail(e => e
                .Valid("anton@example.com")
                .BoundaryValid("a@e.io")
                .BoundaryValid("valid.email+tagging@domain.fr")
                .BoundaryInvalid("a@e.c")
                    .WithExpectedMessage("Email must be at least 6 characters long.")
                .BoundaryInvalid("user@domain.toolo.com")  // 21 symbols
                    .WithExpectedMessage("Email must not exceed 20 characters.")

                        // Pattern-related
                .Invalid("").WithExpectedMessage("Email is required.")
                .Invalid("notanemail").WithExpectedMessage("Enter a valid email address.")
                .Invalid("missing@tld").WithExpectedMessage("Enter a valid email address.")
                .Invalid("user@domain.c").WithExpectedMessage("Enter a valid email address.")
                .Invalid("invalid-email").WithExpectedMessage("Enter a valid email address.")
                .Invalid("plainaddress").WithExpectedMessage("Enter a valid email address.")
                .Invalid("@missingusername.com").WithExpectedMessage("Enter a valid email address.")
                .Invalid("missingdomain@").WithExpectedMessage("Enter a valid email address.")
                .Invalid("user@.com").WithExpectedMessage("Enter a valid email address.")
                .Invalid("user@domain..com").WithExpectedMessage("Enter a valid email address.")
            )

            .AddPhone(p => p
                .Valid("+359888888888")
                .BoundaryValid("+3598888")
                .BoundaryValid("+359888888888888")
                .BoundaryInvalid("+3598")
                    .WithExpectedMessage("Phone number must be at least 6 digits.")
                .BoundaryInvalid("+3598888888888888")
                    .WithExpectedMessage("Phone number must not exceed 15 digits.")

                // Invalid format values
                .Invalid("12345").WithExpectedMessage("Enter a valid international phone number.")
                .Invalid("0000000000").WithExpectedMessage("Enter a valid international phone number.")
                .Invalid("abcdefg").WithExpectedMessage("Enter a valid international phone number.")
                .Invalid("+123").WithExpectedMessage("Phone number must be at least 6 digits.")
                .Invalid("+359 888").WithExpectedMessage("Phone number must not contain spaces.")
                .Invalid("+359888BADNUM").WithExpectedMessage("Phone number must contain only digits.")
                .Invalid("(123) 456-7890-ext").WithExpectedMessage("Phone number must not contain symbols or brackets.")
                .Invalid("").WithExpectedMessage("Phone number is required.")
            )

            .AddPassword(p => p
                .Valid("Secure1!")
                .BoundaryValid("Aa1@abcd")
                .BoundaryValid("Aa1@" + new string('x', 16))
                .BoundaryInvalid("Aa1@abc")
                    .WithExpectedMessage("Password must be 8–20 characters with uppercase, number, and symbol.")
                .BoundaryInvalid("Aa1@" + new string('x', 17))
                    .WithExpectedMessage("Password must be 8–20 characters with uppercase, number, and symbol.")
                .Invalid("")
                    .WithExpectedMessage("Password is required.")
            )

            .AddInteger(i => i
                .Valid(25)
                .BoundaryValid(18)
                .BoundaryValid(100)
                .BoundaryInvalid(17)
                    .WithExpectedMessage("Age must be between 18 and 100.")
                .BoundaryInvalid(101)
                    .WithExpectedMessage("Age must be between 18 and 100.")
            )

            .AddDate(d => d
                .Valid(DateTime.Parse("1990-01-01"))
                .BoundaryValid(DateTime.Parse("1920-01-01"))
                .BoundaryValid(DateTime.Parse("2020-12-31"))
                .BoundaryInvalid(DateTime.Parse("1919-12-31"))
                    .WithExpectedMessage("Birthdate must be between 1920 and 2020.")
                .BoundaryInvalid(DateTime.Parse("2021-01-01"))
                    .WithExpectedMessage("Birthdate must be between 1920 and 2020.")
            )

            .AddUrl(u => u
                .Valid("https://example.com")
                .BoundaryValid("http://a.co")
                .BoundaryValid("https://very-long-url.com/with/path")
                .BoundaryInvalid("invalid.url")
                    .WithExpectedMessage("Please enter a valid website URL.")
                .BoundaryInvalid("ftp://site.com")
                    .WithExpectedMessage("Please enter a valid website URL.")

                // Newly requested invalid cases
                .Invalid("www.google.com").WithExpectedMessage("Please enter a valid website URL.")
                .Invalid("http:/invalid.com").WithExpectedMessage("Please enter a valid website URL.")
                .Invalid("ftp://wrong.protocol").WithExpectedMessage("Please enter a valid website URL.")
                .Invalid("://missing.scheme.com").WithExpectedMessage("Please enter a valid website URL.")
                .Invalid("").WithExpectedMessage("Website is required.")
            )

            .AddBoolean(b => b
                .Valid(true)
                .Invalid(false).WithExpectedMessage("You must accept the terms to proceed.")
            )

            .AddSingleSelect(s => s
                .Valid("United States")
                .Valid("France")
                .Valid("Germany")
                .Invalid(null).WithExpectedMessage("Country is required.")
            )

            .AddMultiSelect(m => m
                .Valid(new[] { "English", "French" })
                .BoundaryValid(new[] { "German" })
                .BoundaryValid(new[] { "English", "French", "German" })
                .BoundaryInvalid(null).WithExpectedMessage("Select at least one language.")
            ),

        settings =>
        {
            settings.Mode = TestGenerationMode.HybridArtificialBeeColony;
            settings.TestCaseCategory = TestCaseCategory.Valid;
            settings.ABCSettings = new ABCGenerationSettings
            {
                TotalPopulationGenerations = 100,
                MutationRate = 0.6,
                FinalPopulationSelectionRatio = 0.5,
                EliteSelectionRatio = 0.3,
                OnlookerSelectionRatio = 0.1,
                ScoutSelectionRatio = 0.3,
                EnableOnlookerSelection = true,
                EnableScoutPhase = true,
                EnforceMutationUniqueness = true,
                StagnationThresholdPercentage = 0.75,
                CoolingRate = 0.95,
                AllowMultipleInvalidInputs = false,
                OutputGenerator = new NUnitTestCaseSourceOutputGenerator()
            };
        })
    .Generate();
```
The following test will output to console and copy to clipboard the following tests used via the NUnit TestCaseAttribute:
```csharp
[TestCase("AAAAAAAAAAAAAAAAAAAA", "valid.email+tagging@domain.fr", "+359888888888888", "Aa1@abcd", 100, "01-01-1920", "http://a.co", false, "France", new[] { "English", "French", "German" }, "You must accept the terms to proceed.")]
[TestCase("Ann", "a@e.c", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 18, "01-01-1920", "http://a.co", true, "France", new[] { "English", "French", "German" }, "Email must be at least 6 characters long.")]
[TestCase("Ann", "user@domain.toolo.com", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 100, "31-12-2020", "https://very-long-url.com/with/path", true, "France", new[] { "German" }, "Email must not exceed 50 characters.")]
[TestCase("Ann", "valid.email+tagging@domain.fr", "+359888888888888", "Aa1@abc", 18, "31-12-2020", "http://a.co", true, "United States", new[] { "English", "French", "German" }, "Password must be 8–20 characters with uppercase, number, and symbol.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+3598", "Aa1@abcd", 100, "01-01-1920", "https://very-long-url.com/with/path", true, "Germany", new[] { "English", "French", "German" }, "Phone number must be at least 6 digits.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+3598888888888888", "Aa1@xxxxxxxxxxxxxxxx", 100, "31-12-2020", "https://very-long-url.com/with/path", true, "Germany", new[] { "German" }, "Phone number must not exceed 15 digits.")]
[TestCase("An", "valid.email+tagging@domain.fr", "+359888888888888", "Aa1@xxxxxxxxxxxxxxxx", 100, "01-01-1920", "https://very-long-url.com/with/path", true, "Germany", new[] { "English", "French", "German" }, "Full Name must be between 3 and 20 characters.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "user@domain.toolo.com", "+359888888888888", "Aa1@xxxxxxxxxxxxxxxx", 18, "01-01-1920", "http://a.co", true, "United States", new[] { "English", "French", "German" }, "Email must not exceed 50 characters.")]
[TestCase("Ann", "valid.email+tagging@domain.fr", "+3598888888888888", "Aa1@xxxxxxxxxxxxxxxx", 18, "31-12-2020", "https://very-long-url.com/with/path", true, "France", new[] { "German" }, "Phone number must not exceed 15 digits.")]
[TestCase("Ann", "valid.email+tagging@domain.fr", "+3598", "Aa1@abcd", 100, "31-12-2020", "http://a.co", true, "France", new[] { "German" }, "Phone number must be at least 6 digits.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+3598888", "Aa1@abcd", 18, "31-12-2020", "invalid.url", true, "United States", new[] { "German" }, "Please enter a valid website URL.")]
[TestCase("Ann", "valid.email+tagging@domain.fr", "(123) 456-7890-ext", "Aa1@abcd", 18, "01-01-1920", "https://very-long-url.com/with/path", true, "Germany", new[] { "English", "French", "German" }, "Phone number must not contain symbols or brackets.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+359888888888888", "Secure1!", 100, "31-12-2020", "https://very-long-url.com/with/path", false, "United States", new[] { "German" }, "You must accept the terms to proceed.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "user@domain.c", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 18, "31-12-2020", "http://a.co", true, "France", new[] { "German" }, "Enter a valid email address.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "valid.email+tagging@domain.fr", "+359888888888888", "Aa1@abcd", 18, "31-12-2020", "https://example.com", false, "United States", new[] { "German" }, "You must accept the terms to proceed.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+359888888888", "Aa1@abcd", 18, "01-01-1920", "http://a.co", true, null, new[] { "German" }, "Country is required.")]
[TestCase("Ann", "a@e.io", "+3598888", "Aa1@abcd", 18, "31-12-2020", "http:/invalid.com", true, "United States", new[] { "German" }, "Please enter a valid website URL.")]
[TestCase("", "valid.email+tagging@domain.fr", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 18, "31-12-2020", "https://very-long-url.com/with/path", true, "United States", new[] { "English", "French", "German" }, "Full Name is required.")]
[TestCase("Ann", "a@e.io", "+359888888888888", "Aa1@abcd", 18, "31-12-2020", "www.google.com", true, "France", new[] { "German" }, "Please enter a valid website URL.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "", "+359888888888888", "Aa1@xxxxxxxxxxxxxxxx", 18, "01-01-1920", "http://a.co", true, "Germany", new[] { "English", "French", "German" }, "Email is required.")]
[TestCase("Ann", "a@e.io", "+3598888", "Aa1@abcd", 18, "01-01-1920", "www.google.com", true, "United States", new[] { "German" }, "Please enter a valid website URL.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "notanemail", "+359888888888888", "Aa1@abcd", 100, "01-01-1920", "http://a.co", true, "France", new[] { "English", "French", "German" }, "Enter a valid email address.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "anton@example.com", "+359888888888888", "Aa1@xxxxxxxxxxxxxxxx", 100, "01-01-1920", "http://a.co", false, "France", new[] { "English", "French", "German" }, "You must accept the terms to proceed.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+359888888888888", "Aa1@xxxxxxxxxxxxxxxx", 25, "31-12-2020", "https://very-long-url.com/with/path", false, "United States", new[] { "German" }, "You must accept the terms to proceed.")]
[TestCase("Ann", "valid.email+tagging@domain.fr", "+359 888", "Aa1@xxxxxxxxxxxxxxxx", 100, "01-01-1920", "https://very-long-url.com/with/path", true, "United States", new[] { "English", "French", "German" }, "Phone number must not contain spaces.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "plainaddress", "+359888888888888", "Aa1@abcd", 18, "31-12-2020", "https://very-long-url.com/with/path", true, "United States", new[] { "English", "French", "German" }, "Enter a valid email address.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "", "Aa1@xxxxxxxxxxxxxxxx", 18, "31-12-2020", "http://a.co", true, "France", new[] { "German" }, "Phone number is required.")]
[TestCase("Ann", "@missingusername.com", "+359888888888888", "Aa1@xxxxxxxxxxxxxxxx", 18, "31-12-2020", "http://a.co", true, "United States", new[] { "English", "French", "German" }, "Enter a valid email address.")]
[TestCase("Anton Angelov", "a@e.c", "+3598888", "Aa1@abcd", 100, "31-12-2020", "https://very-long-url.com/with/path", true, "Germany", new[] { "German" }, "Email must be at least 6 characters long.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "valid.email+tagging@domain.fr", "+359888888888", "Aa1@xxxxxxxxxxxxxxxx", 17, "01-01-1920", "https://very-long-url.com/with/path", true, "France", new[] { "German" }, "Age must be between 18 and 100.")]
[TestCase("Ann", "a@e.io", "+359888888888", "Aa1@abcd", 100, "01-01-1920", "ftp://site.com", true, "France", new[] { "German" }, "Please enter a valid website URL.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.c", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 18, "01-01-1920", "https://very-long-url.com/with/path", true, "France", new[] { "English", "French" }, "Email must be at least 6 characters long.")]
[TestCase("Ann", "anton@example.com", "+3598", "Aa1@xxxxxxxxxxxxxxxx", 18, "01-01-1920", "http://a.co", true, "United States", new[] { "German" }, "Phone number must be at least 6 digits.")]
[TestCase("Ann", "anton@example.com", "+359888888888888", "Aa1@xxxxxxxxxxxxxxxx", 18, "01-01-2021", "http://a.co", true, "United States", new[] { "German" }, "Birthdate must be between 1920 and 2020.")]
[TestCase("AAAAAAAAAAAAAAAAAAAAA", "valid.email+tagging@domain.fr", "+359888888888888", "Aa1@abcd", 100, "01-01-1990", "http://a.co", true, "France", new[] { "German" }, "Full Name must be between 3 and 20 characters.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.c", "+3598888", "Secure1!", 100, "31-12-2020", "http://a.co", true, "Germany", new[] { "German" }, "Email must be at least 6 characters long.")]
[TestCase("AAAAAAAAAAAAAAAAAAAAA", "valid.email+tagging@domain.fr", "+359888888888888", "Aa1@abcd", 18, "01-01-1920", "https://example.com", true, "United States", new[] { "German" }, "Full Name must be between 3 and 20 characters.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "user@domain.toolo.com", "+359888888888", "Aa1@abcd", 100, "01-01-1920", "https://very-long-url.com/with/path", true, "France", new[] { "German" }, "Email must not exceed 50 characters.")]
[TestCase("Ann", "valid.email+tagging@domain.fr", "+3598", "Aa1@abcd", 18, "01-01-1990", "http://a.co", true, "France", new[] { "German" }, "Phone number must be at least 6 digits.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+359888888888", "Aa1@xxxxxxxxxxxxxxxx", 18, "01-01-1920", "ftp://site.com", true, "United States", new[] { "German" }, "Please enter a valid website URL.")]
[TestCase("Ann", "anton@example.com", "+3598", "Aa1@abcd", 100, "01-01-1920", "https://very-long-url.com/with/path", true, "Germany", new[] { "English", "French", "German" }, "Phone number must be at least 6 digits.")]
[TestCase("Ann", "valid.email+tagging@domain.fr", "+3598888", "Secure1!", 100, "31-12-2020", "http:/invalid.com", true, "Germany", new[] { "German" }, "Please enter a valid website URL.")]
[TestCase("Ann", "a@e.io", "+123", "Aa1@abcd", 18, "31-12-2020", "https://example.com", true, "France", new[] { "English", "French", "German" }, "Phone number must be at least 6 digits.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "valid.email+tagging@domain.fr", "", "Secure1!", 100, "01-01-1920", "http://a.co", true, "United States", new[] { "English", "French", "German" }, "Phone number is required.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "plainaddress", "+359888888888888", "Aa1@abcd", 18, "01-01-1920", "https://example.com", true, "Germany", new[] { "German" }, "Enter a valid email address.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "user@.com", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 18, "01-01-1920", "https://example.com", true, "United States", new[] { "German" }, "Enter a valid email address.")]
[TestCase("Ann", "a@e.io", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 18, "31-12-2020", "https://example.com", false, "Germany", new[] { "English", "French" }, "You must accept the terms to proceed.")]
[TestCase("", "a@e.io", "+3598888", "Secure1!", 18, "01-01-1920", "https://very-long-url.com/with/path", true, "United States", new[] { "German" }, "Full Name is required.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 18, "31-12-2020", "https://example.com", true, "United States", new[] { "German" }, "Email is required.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "anton@example.com", "abcdefg", "Aa1@xxxxxxxxxxxxxxxx", 18, "01-01-1920", "http://a.co", true, "France", new[] { "German" }, "Enter a valid international phone number.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+359888BADNUM", "Aa1@abcd", 25, "01-01-1920", "https://very-long-url.com/with/path", true, "United States", new[] { "German" }, "Phone number must contain only digits.")]
[TestCase("Anton Angelov", "a@e.io", "(123) 456-7890-ext", "Aa1@abcd", 100, "01-01-1920", "https://very-long-url.com/with/path", true, "France", new[] { "English", "French", "German" }, "Phone number must not contain symbols or brackets.")]
[TestCase("Ann", "valid.email+tagging@domain.fr", "+123", "Aa1@abcd", 18, "01-01-1920", "http://a.co", true, "United States", new[] { "English", "French" }, "Phone number must be at least 6 digits.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "valid.email+tagging@domain.fr", "(123) 456-7890-ext", "Aa1@abcd", 100, "01-01-1920", "https://example.com", true, "France", new[] { "English", "French", "German" }, "Phone number must not contain symbols or brackets.")]
[TestCase("Ann", "a@e.io", "+3598888", "Secure1!", 18, "31-12-2020", "https://example.com", true, null, new[] { "German" }, "Country is required.")]
[TestCase("Ann", "valid.email+tagging@domain.fr", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 18, "01-01-1920", "http:/invalid.com", true, "Germany", new[] { "English", "French" }, "Please enter a valid website URL.")]
[TestCase("Ann", "anton@example.com", "abcdefg", "Aa1@abcd", 100, "01-01-1920", "https://very-long-url.com/with/path", true, "France", new[] { "English", "French", "German" }, "Enter a valid international phone number.")]
[TestCase("Ann", "a@e.io", "+123", "Aa1@xxxxxxxxxxxxxxxx", 25, "01-01-1920", "http://a.co", true, "Germany", new[] { "German" }, "Phone number must be at least 6 digits.")]
[TestCase("Anton Angelov", "valid.email+tagging@domain.fr", "+359888BADNUM", "Aa1@abcd", 18, "01-01-1920", "https://very-long-url.com/with/path", true, "France", new[] { "English", "French", "German" }, "Phone number must contain only digits.")]
[TestCase("Anton Angelov", "user@domain.c", "+359888888888888", "Aa1@abcd", 18, "31-12-2020", "http://a.co", true, "United States", new[] { "German" }, "Enter a valid email address.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+3598888", "", 100, "31-12-2020", "https://example.com", true, "United States", new[] { "English", "French", "German" }, "Password is required.")]
[TestCase("Ann", "valid.email+tagging@domain.fr", "+3598888", "Secure1!", 100, "31-12-2020", "http:/invalid.com", true, "United States", new[] { "English", "French", "German" }, "Please enter a valid website URL.")]
[TestCase("Ann", "@missingusername.com", "+359888888888", "Aa1@xxxxxxxxxxxxxxxx", 100, "31-12-2020", "http://a.co", true, "Germany", new[] { "English", "French", "German" }, "Enter a valid email address.")]
[TestCase("Ann", "valid.email+tagging@domain.fr", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 100, "01-01-1920", "://missing.scheme.com", true, "Germany", new[] { "English", "French" }, "Please enter a valid website URL.")]
[TestCase("Ann", "missingdomain@", "+359888888888", "Aa1@abcd", 18, "01-01-1920", "https://very-long-url.com/with/path", true, "United States", new[] { "German" }, "Enter a valid email address.")]
[TestCase("Anton Angelov", "missingdomain@", "+359888888888888", "Aa1@abcd", 18, "01-01-1920", "http://a.co", true, "France", new[] { "German" }, "Enter a valid email address.")]
[TestCase("Ann", "user@domain..com", "+3598888", "Aa1@abcd", 25, "31-12-2020", "https://very-long-url.com/with/path", true, "France", new[] { "English", "French", "German" }, "Enter a valid email address.")]
[TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+359888888888", "Aa1@abcd", 18, "31-12-2020", "", true, "United States", new[] { "German" }, "Website is required.")]
 public void SubmitForm_WithValidation(string fullName, string email, string phone, string password, int age, string birthdate, string website, bool? terms, string country, string[] languages, string expectedError)
 {
    // your test's logic
 }
```
---

## 🧠 Generation Strategies

| Strategy | Description |
|---------|-------------|
| `Combinatorial` | All combinations of inputs (exhaustive, grows fast). |
| `Pairwise` | Only pairwise combinations for efficiency. |
| `PairwiseOptimized` | Uses ABC fitness function to keep only best-scoring pairwise tests. |
| `CombinatorialOptimized` | Similar optimization applied to full combinations. |
| `HybridArtificialBeeColony` (**ABC**) | Heuristic approach for selecting most valuable test cases. Highest coverage-to-size ratio. |

```json
{
    "testimizeSettings": {
    "seed": 12345,
    "locale": "en", 
    "allowBoundaryValues": true,
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
        "allowMultipleInvalidInputs": false,
        "seed": 8941 // 42
    },
    "inputTypeSettings": {
    //...
    }
}
```

---

## ✅ Usage in Tests

### ✅ Use `TestimizeGeneratedTestCases` NUnit Attributte

```csharp
[TestFixture]
public class SampleTests
{
    public static List<TestCase> ConfigureEngine() =>
        TestimizeEngine.Configure(
            parameters => parameters
                .AddText(6, 12)
                .AddEmail(5, 10)
                .AddPhone(6, 8)
                .AddText(4, 10)
            , settings =>
            {
                settings.Mode = TestGenerationMode.HybridArtificialBeeColony;
                settings.TestCaseCategory = TestCaseCategory.Validation;
            }
            ).Generate();

    [Test, TestimizeGeneratedTestCases(nameof(ConfigureEngine))]
    public void TestABCGeneration(string textValue, string email, string phone, string anotherText)
    {
        // your test logic here
    }
}

```

---

### ✅ Use `TestimizeGeneratedTestCases` MSTest Attributte

```csharp
[TestClass]
public class SampleTests
{
    public static List<TestCase> ConfigureEngine() =>
        TestimizeEngine.Configure(
            parameters => parameters
                .AddText(6, 12)
                .AddEmail(5, 10)
                .AddPhone(6, 8)
                .AddText(4, 10)
            , settings =>
            {
                settings.Mode = TestGenerationMode.HybridArtificialBeeColony;
                settings.TestCaseCategory = TestCaseCategory.Validation;
            }
            ).Generate();

    [DataTestMethod]
    [TestimizeGeneratedTestCases(nameof(ConfigureEngine))]
    public void TestABCGeneration(string textValue, string email, string phone, string anotherText)
    {
        // your test logic here
    }
}

```
---

### ✅ Use `TestimizeGeneratedTestCases` xUnit Attributte

```csharp
public class SampleXUnitTests
{
    public static List<TestCase> ConfigureEngine() =>
        TestimizeEngine.Configure(
            parameters => parameters
                .AddText(6, 12)
                .AddEmail(5, 10)
                .AddPhone(6, 8)
                .AddText(4, 10)
            , settings =>
            {
                settings.Mode = TestGenerationMode.HybridArtificialBeeColony;
                settings.TestCaseCategory = TestCaseCategory.Validation;
            }
            ).Generate();

    [Theory]
    [TestimizeGeneratedTestCases(nameof(ConfigureEngine))]
    public void TestABCGeneration(string textValue, string email, string phone, string anotherText)
    {
        // your test logic here
    }
}

```
---

## 🛠 Available Parameters

| Type | Parameter Class |
|------|------------------|
| Text | `TextDataParameter` |
| Email | `EmailDataParameter` |
| Password | `PasswordDataParameter` |
| Phone | `PhoneDataParameter` |
| Integer | `IntegerDataParameter` |
| Boolean | `BooleanDataParameter` |
| Date | `DateDataParameter`, `DateTimeDataParameter`, `WeekDataParameter`, `MonthDataParameter` |
| Select (Single/Multi) | `SingleSelectDataParameter`, `MultiSelectDataParameter` |
| URL | `UrlDataParameter` |
| Username | `UsernameDataParameter` |
| Address | `AddressDataParameter` |
| Currency | `CurrencyDataParameter` |
| Time | `TimeDataParameter` |
| Percentage | `PercentageDataParameter` |
| Color | `ColorDataParameter` |
| Geo Coordinates | `GeoCoordinateDataParameter` |

---

## ⚙ Configuration File (`testimizeSettings.json`)

Define custom equivalence classes and settings for each input type:

```json

"InputTypeSettings":{
    "Email":{
        "PrecisionStep":"1",
        "ValidEquivalenceClasses":["user@example.com", "contact@domain.net"],
        "InvalidEquivalenceClasses":[
        "invalid-email",
        "plainaddress",
        "@missingusername.com",
        "missingdomain@",
        "user@.com",
        "user@domain..com"
        ]
    },
    "Phone":{
        "PrecisionStep":"1",
        "ValidEquivalenceClasses":["+11234567890", "+442071838750"],
        "InvalidEquivalenceClasses":[
        "12345",
        "0000000000",
        "abcdefg",
        "+123",
        "+359 888",
        "+359888BADNUM",
        "(123) 456-7890-ext"
        ]
    },
    "Percentage":{
        "PrecisionStep":"0.01",
        "ValidEquivalenceClasses":[0.0, 50.5, 99.99, 100.0 ],
        "InvalidEquivalenceClasses":[-1, 101, "text", ""]
    },
    "Currency":{
        "PrecisionStep":"0.01",
        "FormatString":"C2",
        "ValidEquivalenceClasses":[0.0, 19.99, 100.00, 99999.99 ],
        "InvalidEquivalenceClasses":[-5, "free", "text", ""]
    },
    "Date":{
        "PrecisionStep":"1",
        "PrecisionStepUnit":"Days",
        "FormatString":"yyyy-MM-dd",
        "ValidEquivalenceClasses":["2024-01-01", "1990-12-31", "2025-03-26"],
        "InvalidEquivalenceClasses":["not-a-date", "13/32/2020",""]
    },
    "Time":{
        "PrecisionStep":"15",
        "PrecisionStepUnit":"Minutes",
        "FormatString":"hh\\:mm",
        "ValidEquivalenceClasses":["00:00", "12:30", "23:59"],
        "InvalidEquivalenceClasses":["24:00", "99:99", "noon",""]
    }
}

```
## 📦 Supported Output Generators

| Output Generator Class                        | Target Framework | Description                                                                 |
|----------------------------------------------|------------------|-----------------------------------------------------------------------------|
| `NUnitTestCaseAttributeOutputGenerator`       | NUnit            | Generates `[TestCase(...)]` attributes per test case.                       |
| `NUnitTestCaseSourceOutputGenerator`          | NUnit            | Outputs a method with `IEnumerable<object[]>` for use with `TestCaseSource`.|
| `XUnitInlineDataOutputGenerator`              | xUnit            | Generates `[InlineData(...)]` attributes for xUnit tests.                   |
| `MSTestTestMethodAttributeOutputGenerator`    | MSTest           | Generates `[DataTestMethod]` and `[DataRow(...)]` attributes.               |
| `CsvTestCaseOutputGenerator`                  | CSV              | Writes test cases as CSV rows (e.g., for import/export or tools).           |
| `JsonTestCaseOutputGenerator`                 | JSON             | Outputs test cases as a structured JSON array (used in pipelines/tools).    |
| `FactoryMethodTestCaseOutputGenerator`        | Any (.NET)       | Generates `List<Model>` factory-style methods for object-based test cases.  |


---

## 📊 Advanced Topics
---

## 💡 When to Use Which Mode?

| Mode | Use Case |
|------|----------|
| Exploratory | Early-stage testing, discover missing validations, generate bulk coverage |
| Precise | Regression suites, CI pipelines, validation of well-defined rules |

---
## 🧠 Theory and Metaheuristic Foundations

Testimize leverages classical testing techniques enhanced by powerful metaheuristics to produce compact, high-value test cases. Below we explain the core test design principles and the intelligent optimization behind Hybrid ABC.

---

### 🔍 Classical Test Design Techniques

#### ✅ Equivalence Partitioning (EP)
Equivalence Partitioning divides input data into groups where values are assumed to behave the same. Only one representative is tested from each class.

**Example:**  
For input 1–100, equivalence classes are:
- Valid: [1–100]
- Invalid: <1, >100

Testimize uses this in both **Exploratory** and **Precise** modes via predefined or custom equivalence classes.

---

#### ✅ Boundary Value Analysis (BVA)
BVA focuses on values at the edge of valid and invalid ranges where bugs frequently occur.

**Example:**  
For 1–100, test:
- 0 (invalid), 1 (min), 100 (max), 101 (invalid)

In Testimize, BVA is automatically added when using boundary-aware data parameters like:
```csharp
.AddInteger(1, 100)
```

---

#### ✅ Pairwise Testing
Pairwise ensures every pair of input parameters is tested in all combinations, significantly reducing test count while maintaining interaction coverage.

**Example:**  
3 fields with 3 values →  
- Full combo: 27 cases  
- Pairwise: ~9 cases

Use it via:
```csharp
settings.Mode = TestGenerationMode.Pairwise;
```

---

#### ✅ Combinatorial Testing
All combinations of all inputs. Use when you need **exhaustive** coverage (e.g., <4 fields).

```csharp
settings.Mode = TestGenerationMode.Combinatorial;
```

Combine with fitness filtering:
```csharp
settings.Mode = TestGenerationMode.CombinatorialOptimized;
```

---

## 🐝 Hybrid Artificial Bee Colony (ABC)

Hybrid ABC is a swarm-based optimization algorithm inspired by bee foraging behavior. It generates **small but effective test sets** by maximizing test case fitness.

---

### 🎯 How Hybrid ABC Works

Each test case is a **"food source"**. Bees explore and refine these:

- **Employed bees** mutate current test cases.
- **Onlooker bees** choose promising ones to exploit based on fitness.
- **Scout bees** randomly generate new ones after stagnation.

---

### ⚙️ Key Enhancements in Testimize

| Feature                      | Purpose |
|-----------------------------|---------|
| **Elite selection**         | Keeps top test cases across generations. |
| **Simulated annealing**     | Allows accepting worse solutions early to escape local optima. |
| **Cooling rate**            | Gradually lowers mutation intensity over time. |
| **RCL (restricted candidate list)** | Selects test cases probabilistically for balanced search. |
| **Mutation uniqueness**     | Accepts only improving mutations (hill climbing). |
| **Tabu-like behavior**      | Avoids regenerating previously seen test cases. |

---

### 🧮 Fitness Function Details

Each test case receives a fitness score based on:

| Value Category        | Score |
|-----------------------|-------|
| Boundary Valid        | +20   |
| Valid                 | +2    |
| Boundary Invalid      | -1    |
| Invalid               | -2    |
| **New unique value**  | +25   |

If `AllowMultipleInvalidInputs=false`, then for each extra invalid parameter:
- **Penalty = -50 × (invalidCount)**

**Formula:**
```
F(t) = Σ score(value) + Σ uniqueness bonuses - invalid penalties
```

This scoring:
- Promotes **diversity**
- Maximizes **coverage of categories**
- Penalizes test cases that are too negative

---
### ✅ When to Use What

| Use Case                         | Suggested Mode |
|----------------------------------|----------------|
| Fast exploration of inputs       | Exploratory + Pairwise |
| Maximum fault detection          | Precise + Hybrid ABC |
| Stable CI-friendly generation    | Precise + ABC + fixed Seed |
| Form/API validation coverage     | Exploratory + ABC |
| Performance-sensitive test sets  | ABC + FinalPopulationSelectionRatio < 0.5 |

---
## 🔧 ABCGenerationSettings Explained

The `ABCGenerationSettings` class contains all the configuration options for the HybridArtificialBeeColonyTestCaseGenerator. Here's a breakdown of each property:

| Setting                          | Description |
|----------------------------------|-------------|
| `Seed`                           | Seed for the random number generator to allow reproducibility. |
| `TotalPopulationGenerations`     | Number of generations to run the ABC optimization algorithm. |
| `MutationRate`                   | Probability of mutation during the exploration phase (between 0.0 and 1.0). |
| `FinalPopulationSelectionRatio`  | Proportion (0.0–1.0) of the best-performing test cases to retain after scoring. |
| `EliteSelectionRatio`            | Ratio of the best solutions passed to the next generation unmodified. |
| `OnlookerSelectionRatio`         | Ratio of the population considered in the onlooker phase. |
| `ScoutSelectionRatio`            | Ratio of solutions explored randomly when stagnation is detected. |
| `EnableOnlookerSelection`        | Enables the onlooker phase that exploits better regions of the solution space. |
| `EnableScoutPhase`               | Enables random exploration when the population stagnates. |
| `EnforceMutationUniqueness`     | Ensures each mutation in the population is unique (costly but good for diversity). |
| `StagnationThresholdPercentage`  | Percentage (0.0–1.0) determining when stagnation occurs (low diversity). |
| `CoolingRate`                    | Cooling factor to gradually reduce mutation aggressiveness. |
| `AllowMultipleInvalidInputs`     | If false, filters test cases that contain more than one invalid input. |

These values can be tuned based on the complexity of your test data and the desired balance between exploration and test suite compactness.

## 🔧 Global Testimize Settings

Testimize also supports a range of global configuration settings (defined in `testimizeSettings.json`) which influence how input values are generated in **Exploratory Mode**:

| Setting | Description | Default |
|--------|-------------|---------|
| `AllowBoundaryValues` | Whether to automatically generate boundary values for applicable parameters. | `true` |
| `AllowValidEquivalenceClasses` | Whether to generate valid equivalence class values from preconfigured lists. | `true` |
| `AllowInvalidEquivalenceClasses` | Whether to generate invalid values from preconfigured lists. | `true` |
| `PrecisionStep` | Determines the step between boundary values for numeric/text/date types. | `1` (int) or depends on type |
| `PrecisionStepUnit` | Unit of step (e.g., `days`, `chars`) for complex types like `Date`, `Text`. | Depends on provider |
| `FormatString` | Optional formatting string for specific input types (e.g., `"yyyy-MM-dd"` for dates). | Depends on provider |
| `InputTypeSettings` | Dictionary of rules per parameter type (e.g., for `"Text"`, `"Email"`, `"Integer"`) | Customizable |

These settings are especially useful when using boundary-capable strategies like `IntegerDataParameter`, `TextDataParameter`, or `DateTimeDataParameter`, which extend `BoundaryCapableDataProviderStrategy<T>`.

## 🧩 Extendability

You can create your own:

- `IInputParameter` types (e.g., for file uploads, geolocation, etc.)
- `ITestCaseGenerator` strategies (e.g., metaheuristics)
- `ITestCaseEvaluator` scoring functions
- `IOutputGenerator` for NUnit, XUnit, or custom dashboards
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
- ✅ System Web, Mobile, Desktop, API tests
- ✅ Data-driven tests
- ✅ Exploratory test generation

---

## 👥 Contributors

Made with ❤️ by [@angelovstanton] and the Testimize community.

---

## 📜 License

Licensed under the [Apache License, Version 2.0](https://www.apache.org/licenses/LICENSE-2.0).
