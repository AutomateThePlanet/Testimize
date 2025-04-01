// <copyright file="FormValidationTests.cs" company="Automate The Planet Ltd.">
// Copyright 2025 Automate The Planet Ltd.
// Licensed under the Apache License, Version 2.0 (the "License");
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Anton Angelov</author>
// <site>https://automatetheplanet.com/</site>

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Testimize.OutputGenerators;
using Testimize.Usage;

namespace Testimize.Tests.RealWorldExamples;

[TestFixture]
public class FormValidationTests
{
    private IWebDriver _driver;

    [OneTimeSetUp]
    public void ClassInit()
    {
        _driver = new ChromeDriver();
        var htmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "testimize-sample-form.html");
        _driver.Navigate().GoToUrl($"file:///{htmlFilePath.Replace("\\", "/")}");
    }

    [OneTimeTearDown]
    public void ClassCleanup()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }

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
                    OutputGenerator = new NUnitTestCaseAttributeOutputGenerator()
                };
            })
        .Generate();



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
    public void SubmitForm_WithValidation(
    string fullName, string email, string phone, string password, int age,
    string birthdate, string website, bool? terms, string country, string[] languages, string expectedError)
    {
        _driver.Navigate().Refresh();

        Console.WriteLine("=== Input Parameters ===");
        Console.WriteLine($"Full Name: {fullName}");
        Console.WriteLine($"Email: {email}");
        Console.WriteLine($"Phone: {phone}");
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Age: {age}");
        Console.WriteLine($"Birthdate: {birthdate}");
        Console.WriteLine($"Website: {website}");
        Console.WriteLine($"Terms Accepted: {terms}");
        Console.WriteLine($"Country: {country}");
        Console.WriteLine($"Languages: {string.Join(", ", languages ?? Array.Empty<string>())}");
        Console.WriteLine($"Expected Error: {expectedError}");

        _driver.FindElement(By.Id("fullName")).SendKeys(fullName);
        _driver.FindElement(By.Id("email")).SendKeys(email);
        _driver.FindElement(By.Id("phone")).SendKeys(phone);
        _driver.FindElement(By.Id("password")).SendKeys(password);
        _driver.FindElement(By.Id("age")).SendKeys(age.ToString());
        _driver.FindElement(By.Id("birthdate")).SendKeys(birthdate);
        _driver.FindElement(By.Id("website")).SendKeys(website);
        if (terms == true) _driver.FindElement(By.Id("terms")).Click();
        if (country != null) new SelectElement(_driver.FindElement(By.Id("country"))).SelectByText(country);

        if (languages != null)
        {
            foreach (var l in languages)
            {
                new SelectElement(_driver.FindElement(By.Id("languages"))).SelectByText(l);
            }
        }

        var submitButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
        new Actions(_driver).MoveToElement(submitButton).Click().Perform();

        if (!string.IsNullOrWhiteSpace(expectedError))
        {
            var globalError = _driver.FindElement(By.Id("globalError"));
            var invalidFeedbackDivs = _driver.FindElements(By.ClassName("invalid-feedback"))
                                             .Where(div => div.Displayed && !string.IsNullOrWhiteSpace(div.Text))
                                             .ToList();

            Console.WriteLine("=== Displayed Validation Messages ===");
            Debug.WriteLine("=== Displayed Validation Messages ===");
            foreach (var feedback in invalidFeedbackDivs)
            {
                Console.WriteLine($"- {feedback.Text}");
                Debug.WriteLine($"- {feedback.Text}");
            }

            Assert.Multiple(() =>
            {
                Assert.That(invalidFeedbackDivs.Any(), Is.True, "No validation messages were displayed.");
                Assert.That(
                    invalidFeedbackDivs.Any(x => x.Text.Contains(expectedError)),
                    Is.True,
                    $"Expected error message not found: '{expectedError}'"
                );
            });
        }
        else
        {
            var successMsg = _driver.FindElement(By.Id("successMsg"));
            Assert.That(successMsg.Displayed, Is.True, "Expected success message to be visible.");
        }
    }

    [TestCase("Ann", "valid.email+tagging@domain.fr", "+3598888", "Aa1@abcd", 18, "01-01-1990", "http://a.co", true, "United States", new[] { "German" }, "")]
    [TestCase("Ann", "valid.email+tagging@domain.fr", "+359888888888", "Aa1@xxxxxxxxxxxxxxxx", 100, "01-01-1920", "http://a.co", true, "Germany", new[] { "German" }, "")]
    [TestCase("Ann", "a@e.io", "+3598888", "Aa1@abcd", 18, "01-01-1920", "http://a.co", true, "France", new[] { "English", "French" }, "")]
    [TestCase("AAAAAAAAAAAAAAAAAAAA", "valid.email+tagging@domain.fr", "+3598888", "Aa1@abcd", 100, "01-01-1920", "https://example.com", true, "Germany", new[] { "English", "French", "German" }, "")]
    [TestCase("Ann", "a@e.io", "+3598888", "Secure1!", 100, "31-12-2020", "http://a.co", true, "United States", new[] { "German" }, "")]
    [TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 25, "01-01-1920", "https://very-long-url.com/with/path", true, "Germany", new[] { "English", "French", "German" }, "")]
    [TestCase("Ann", "a@e.io", "+359888888888", "Aa1@abcd", 18, "01-01-1920", "https://very-long-url.com/with/path", true, "Germany", new[] { "English", "French", "German" }, "")]
    [TestCase("Ann", "valid.email+tagging@domain.fr", "+359888888888", "Aa1@abcd", 18, "01-01-1920", "http://a.co", true, "France", new[] { "English", "French", "German" }, "")]
    [TestCase("Ann", "valid.email+tagging@domain.fr", "+3598888", "Aa1@abcd", 18, "01-01-1990", "http://a.co", true, "Germany", new[] { "German" }, "")]
    [TestCase("Ann", "valid.email+tagging@domain.fr", "+359888888888888", "Aa1@abcd", 25, "31-12-2020", "http://a.co", true, "United States", new[] { "German" }, "")]
    [TestCase("Ann", "a@e.io", "+3598888", "Aa1@abcd", 18, "31-12-2020", "https://very-long-url.com/with/path", true, "Germany", new[] { "English", "French" }, "")]
    [TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+359888888888", "Aa1@abcd", 18, "01-01-1920", "http://a.co", true, "United States", new[] { "German" }, "")]
    [TestCase("Ann", "a@e.io", "+3598888", "Secure1!", 100, "31-12-2020", "https://very-long-url.com/with/path", true, "United States", new[] { "German" }, "")]
    [TestCase("Anton Angelov", "a@e.io", "+359888888888888", "Aa1@xxxxxxxxxxxxxxxx", 100, "01-01-1920", "https://very-long-url.com/with/path", true, "France", new[] { "English", "French", "German" }, "")]
    [TestCase("Ann", "anton@example.com", "+359888888888888", "Aa1@xxxxxxxxxxxxxxxx", 18, "01-01-1990", "http://a.co", true, "France", new[] { "German" }, "")]
    [TestCase("Ann", "a@e.io", "+359888888888888", "Secure1!", 100, "01-01-1920", "https://example.com", true, "Germany", new[] { "English", "French", "German" }, "")]
    [TestCase("Ann", "valid.email+tagging@domain.fr", "+3598888", "Secure1!", 25, "01-01-1920", "https://very-long-url.com/with/path", true, "United States", new[] { "English", "French", "German" }, "")]
    [TestCase("AAAAAAAAAAAAAAAAAAAA", "anton@example.com", "+359888888888", "Aa1@xxxxxxxxxxxxxxxx", 100, "01-01-1920", "https://very-long-url.com/with/path", true, "France", new[] { "English", "French", "German" }, "")]
    [TestCase("AAAAAAAAAAAAAAAAAAAA", "anton@example.com", "+3598888", "Aa1@xxxxxxxxxxxxxxxx", 25, "31-12-2020", "https://very-long-url.com/with/path", true, "United States", new[] { "English", "French", "German" }, "")]
    [TestCase("Ann", "valid.email+tagging@domain.fr", "+359888888888888", "Aa1@abcd", 25, "31-12-2020", "https://example.com", true, "France", new[] { "German" }, "")]
    [TestCase("Anton Angelov", "a@e.io", "+359888888888888", "Aa1@abcd", 18, "01-01-1990", "http://a.co", true, "Germany", new[] { "English", "French", "German" }, "")]
    [TestCase("Ann", "anton@example.com", "+359888888888", "Aa1@abcd", 18, "01-01-1920", "https://very-long-url.com/with/path", true, "United States", new[] { "English", "French", "German" }, "")]
    [TestCase("Anton Angelov", "a@e.io", "+359888888888888", "Aa1@abcd", 100, "01-01-1990", "http://a.co", true, "Germany", new[] { "English", "French", "German" }, "")]
    [TestCase("Ann", "a@e.io", "+3598888", "Aa1@abcd", 25, "01-01-1990", "http://a.co", true, "Germany", new[] { "German" }, "")]
    [TestCase("Ann", "valid.email+tagging@domain.fr", "+359888888888", "Aa1@abcd", 100, "01-01-1920", "https://example.com", true, "United States", new[] { "German" }, "")]
    [TestCase("AAAAAAAAAAAAAAAAAAAA", "anton@example.com", "+3598888", "Aa1@abcd", 18, "31-12-2020", "http://a.co", true, "France", new[] { "English", "French" }, "")]
    [TestCase("AAAAAAAAAAAAAAAAAAAA", "anton@example.com", "+359888888888888", "Aa1@xxxxxxxxxxxxxxxx", 25, "01-01-1920", "http://a.co", true, "United States", new[] { "German" }, "")]
    [TestCase("Ann", "valid.email+tagging@domain.fr", "+359888888888", "Aa1@abcd", 100, "31-12-2020", "https://very-long-url.com/with/path", true, "Germany", new[] { "English", "French" }, "")]
    [TestCase("Ann", "a@e.io", "+3598888", "Aa1@abcd", 100, "31-12-2020", "https://example.com", true, "France", new[] { "English", "French" }, "")]
    [TestCase("Ann", "a@e.io", "+3598888", "Secure1!", 25, "01-01-1920", "https://very-long-url.com/with/path", true, "Germany", new[] { "German" }, "")]
    [TestCase("AAAAAAAAAAAAAAAAAAAA", "a@e.io", "+359888888888", "Aa1@abcd", 18, "01-01-1990", "https://very-long-url.com/with/path", true, "United States", new[] { "German" }, "")]
    [Ignore]
    public void SuccessMessageDisplayed_WhenSubmitFormWithValidParameters(
   string fullName, string email, string phone, string password, int age,
   string birthdate, string website, bool? terms, string country, string[] languages, string expectedError)
    {
        _driver.Navigate().Refresh();

        Console.WriteLine("=== Input Parameters ===");
        Console.WriteLine($"Full Name: {fullName}");
        Console.WriteLine($"Email: {email}");
        Console.WriteLine($"Phone: {phone}");
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Age: {age}");
        Console.WriteLine($"Birthdate: {birthdate}");
        Console.WriteLine($"Website: {website}");
        Console.WriteLine($"Terms Accepted: {terms}");
        Console.WriteLine($"Country: {country}");
        Console.WriteLine($"Languages: {string.Join(", ", languages ?? Array.Empty<string>())}");
        Console.WriteLine($"Expected Error: {expectedError}");

        _driver.FindElement(By.Id("fullName")).SendKeys(fullName);
        _driver.FindElement(By.Id("email")).SendKeys(email);
        _driver.FindElement(By.Id("phone")).SendKeys(phone);
        _driver.FindElement(By.Id("password")).SendKeys(password);
        _driver.FindElement(By.Id("age")).SendKeys(age.ToString());
        _driver.FindElement(By.Id("birthdate")).SendKeys(birthdate);
        _driver.FindElement(By.Id("website")).SendKeys(website);
        if (terms == true) _driver.FindElement(By.Id("terms")).Click();
        if (country != null) new SelectElement(_driver.FindElement(By.Id("country"))).SelectByText(country);

        if (languages != null)
        {
            foreach (var l in languages)
            {
                new SelectElement(_driver.FindElement(By.Id("languages"))).SelectByText(l);
            }
        }

        var submitButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
        new Actions(_driver).MoveToElement(submitButton).Click().Perform();


        var invalidFeedbackDivs = _driver.FindElements(By.ClassName("invalid-feedback"))
            .Where(div => div.Displayed && !string.IsNullOrWhiteSpace(div.Text))
            .ToList();
        if (invalidFeedbackDivs.Any())
        {
            Console.WriteLine("=== Displayed Validation Messages ===");
            Debug.WriteLine("=== Displayed Validation Messages ===");
            foreach (var feedback in invalidFeedbackDivs)
            {
                Console.WriteLine($"- {feedback.Text}");
                Debug.WriteLine($"- {feedback.Text}");
            }

            Assert.Fail("Expected success message to be visible but there were validation messages displayed.");
        }
        else
        {
            var successMsg = _driver.FindElement(By.Id("successMsg"));
            Assert.That(successMsg.Displayed, Is.True, "Expected success message to be visible.");
        }
    }

}
