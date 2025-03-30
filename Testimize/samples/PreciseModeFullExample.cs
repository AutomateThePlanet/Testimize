// // <copyright file="PreciseModeFullExample.cs" company="Automate The Planet Ltd.">
// // Copyright 2025 Automate The Planet Ltd.
// // Licensed under the Apache License, Version 2.0 (the "License");
// // You may not use this file except in compliance with the License.
// // You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// // Unless required by applicable law or agreed to in writing,
// // software distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// // </copyright>
// // <author>Anton Angelov</author>
// // <site>https://automatetheplanet.com/</site>

using NUnit.Framework;
using Testimize.OutputGenerators;
using Testimize.Usage;

namespace Testimize;

[TestFixture]
public class PreciseModeFullExample
{
    [Test]
    [Ignore("This is used as a template, it is not an actual test")]
    public void StartHereToConfigureYourPreciseMode_With_AllRules_FromTestimizeSettings()
    {
        TestimizeEngine
            .Configure(
                parameters => parameters
                    .AddSelect(s => s
                        .Valid("user@example.com")
                        .Valid("contact@domain.net")
                        .Invalid("invalid-email").WithExpectedMessage("Invalid Email value: 'invalid-email'")
                        .Invalid("plainaddress").WithExpectedMessage("Invalid Email value: 'plainaddress'")
                        .Invalid("@missingusername.com").WithExpectedMessage("Invalid Email value: '@missingusername.com'")
                        .Invalid("missingdomain@").WithExpectedMessage("Invalid Email value: 'missingdomain@'")
                        .Invalid("user@.com").WithExpectedMessage("Invalid Email value: 'user@.com'")
                        .Invalid("user@domain..com").WithExpectedMessage("Invalid Email value: 'user@domain..com'"))

                    .AddSelect(s => s
                        .Valid("+11234567890")
                        .Valid("+442071838750")
                        .Invalid("12345").WithExpectedMessage("Invalid Phone value: '12345'")
                        .Invalid("0000000000").WithExpectedMessage("Invalid Phone value: '0000000000'")
                        .Invalid("abcdefg").WithExpectedMessage("Invalid Phone value: 'abcdefg'")
                        .Invalid("+123").WithExpectedMessage("Invalid Phone value: '+123'")
                        .Invalid("+359 888").WithExpectedMessage("Invalid Phone value: '+359 888'")
                        .Invalid("+359888BADNUM").WithExpectedMessage("Invalid Phone value: '+359888BADNUM'")
                        .Invalid("(123) 456-7890-ext").WithExpectedMessage("Invalid Phone value: '(123) 456-7890-ext'"))

                    .AddSelect(s => s
                        .Valid("Hello World")
                        .Valid("Sample Input")
                        .Invalid("").WithExpectedMessage("Invalid Text value: ''")
                        .Invalid(" ").WithExpectedMessage("Invalid Text value: ' '")
                        .Invalid("\n").WithExpectedMessage("Invalid Text value: '\n'")
                        .Invalid("\t").WithExpectedMessage("Invalid Text value: '\t'")
                        .Invalid("!@#$%^&*()").WithExpectedMessage("Invalid Text value: '!@#$%^&*()'")
                        .Invalid("超长文本超长文本超长文本").WithExpectedMessage("Invalid Text value: '超长文本超长文本超长文本'")
                        .Invalid("<script>alert('XSS')</script>").WithExpectedMessage("Invalid Text value: '<script>alert('XSS')</script>'")
                        .Invalid("' OR 1=1 --").WithExpectedMessage("Invalid Text value: '' OR 1=1 --'"))

                    .AddSelect(s => s
                        .Valid("StrongP@ssw0rd1")
                        .Valid("Another1#Valid")
                        .Invalid("12345").WithExpectedMessage("Invalid Password value: '12345'")
                        .Invalid("password").WithExpectedMessage("Invalid Password value: 'password'")
                        .Invalid("abc").WithExpectedMessage("Invalid Password value: 'abc'")
                        .Invalid(" ").WithExpectedMessage("Invalid Password value: ' '"))

                    .AddSelect(s => s
                        .Valid("true")
                        .Valid("false"))
                    // Optional: Add commented invalids
                    //.Invalid("yes")
                    //.Invalid("no")
                    //.Invalid("1")
                    //.Invalid("maybe")

                    .AddSelect(s => s
                        .Valid("0")
                        .Valid("42")
                        .Valid("-100")
                        .Valid("100000")
                        .Invalid("abc").WithExpectedMessage("Invalid Integer value: 'abc'")
                        .Invalid("").WithExpectedMessage("Invalid Integer value: ''")
                        .Invalid("999999999999999999999").WithExpectedMessage("Invalid Integer value: '999999999999999999999'")
                        .Invalid("-999999999999999999999").WithExpectedMessage("Invalid Integer value: '-999999999999999999999'"))

                    .AddSelect(s => s
                        .Valid("10.5")
                        .Valid("-100.75")
                        .Valid("0.00")
                        .Valid("9999.99")
                        .Invalid("NaN").WithExpectedMessage("Invalid Decimal value: 'NaN'")
                        .Invalid("infinity").WithExpectedMessage("Invalid Decimal value: 'infinity'")
                        .Invalid("text").WithExpectedMessage("Invalid Decimal value: 'text'")
                        .Invalid("").WithExpectedMessage("Invalid Decimal value: ''")
                        .Invalid("null").WithExpectedMessage("Invalid Decimal value: 'null'"))

                    .AddSelect(s => s
                        .Valid("0.0")
                        .Valid("50.5")
                        .Valid("99.99")
                        .Valid("100.0")
                        .Invalid("-1").WithExpectedMessage("Invalid Percentage value: '-1'")
                        .Invalid("101").WithExpectedMessage("Invalid Percentage value: '101'")
                        .Invalid("text").WithExpectedMessage("Invalid Percentage value: 'text'")
                        .Invalid("").WithExpectedMessage("Invalid Percentage value: ''"))

                    .AddSelect(s => s
                        .Valid("0.0")
                        .Valid("19.99")
                        .Valid("100.00")
                        .Valid("99999.99")
                        .Invalid("-5").WithExpectedMessage("Invalid Currency value: '-5'")
                        .Invalid("free").WithExpectedMessage("Invalid Currency value: 'free'")
                        .Invalid("text").WithExpectedMessage("Invalid Currency value: 'text'")
                        .Invalid("").WithExpectedMessage("Invalid Currency value: ''"))

                    .AddSelect(s => s
                        .Valid("2024-01-01")
                        .Valid("1990-12-31")
                        .Valid("2025-03-26")
                        .Invalid("not-a-date").WithExpectedMessage("Invalid Date value: 'not-a-date'")
                        .Invalid("13/32/2020").WithExpectedMessage("Invalid Date value: '13/32/2020'")
                        .Invalid("").WithExpectedMessage("Invalid Date value: ''"))

                    .AddSelect(s => s
                        .Valid("00:00")
                        .Valid("12:30")
                        .Valid("23:59")
                        .Invalid("24:00").WithExpectedMessage("Invalid Time value: '24:00'")
                        .Invalid("99:99").WithExpectedMessage("Invalid Time value: '99:99'")
                        .Invalid("noon").WithExpectedMessage("Invalid Time value: 'noon'")
                        .Invalid("").WithExpectedMessage("Invalid Time value: ''"))

                    .AddSelect(s => s
                        .Valid("2024-10-01T10:30:00")
                        .Valid("1999-12-31T23:59:59")
                        .Valid("2025-03-26T00:00:00")
                        .Invalid("tomorrow").WithExpectedMessage("Invalid DateTime value: 'tomorrow'")
                        .Invalid("32/01/2022 25:00").WithExpectedMessage("Invalid DateTime value: '32/01/2022 25:00'")
                        .Invalid("not-a-datetime").WithExpectedMessage("Invalid DateTime value: 'not-a-datetime'")
                        .Invalid("").WithExpectedMessage("Invalid DateTime value: ''")
                        .Invalid("null").WithExpectedMessage("Invalid DateTime value: 'null'"))

                    .AddSelect(s => s
                        .Valid("2025-W01")
                        .Valid("2024-W52")
                        .Valid("2023-W12")
                        .Invalid("2025-W60").WithExpectedMessage("Invalid Week value: '2025-W60'")
                        .Invalid("2024-W00").WithExpectedMessage("Invalid Week value: '2024-W00'")
                        .Invalid("not-a-week").WithExpectedMessage("Invalid Week value: 'not-a-week'")
                        .Invalid("").WithExpectedMessage("Invalid Week value: ''"))

                    .AddSelect(s => s
                        .Valid("2025-01")
                        .Valid("2024-12")
                        .Valid("1999-07")
                        .Invalid("2025-13").WithExpectedMessage("Invalid Month value: '2025-13'")
                        .Invalid("1999-00").WithExpectedMessage("Invalid Month value: '1999-00'")
                        .Invalid("March 2025").WithExpectedMessage("Invalid Month value: 'March 2025'")
                        .Invalid("").WithExpectedMessage("Invalid Month value: ''"))

                    .AddSelect(s => s
                        .Valid("123 Main St, Springfield, IL 62704")
                        .Valid("456 Elm St, Apt 5B, New York, NY 10001")
                        .Invalid("").WithExpectedMessage("Invalid Address value: ''")
                        .Invalid("No Address").WithExpectedMessage("Invalid Address value: 'No Address'")
                        .Invalid("ZZZ").WithExpectedMessage("Invalid Address value: 'ZZZ'"))

                    .AddSelect(s => s
                        .Valid("42.6975,23.3242")
                        .Valid("48.8566,2.3522")
                        .Valid("-33.8688,151.2093")
                        .Invalid("NaN,NaN").WithExpectedMessage("Invalid GeoCoordinate value: 'NaN,NaN'")
                        .Invalid("999,999").WithExpectedMessage("Invalid GeoCoordinate value: '999,999'")
                        .Invalid("text").WithExpectedMessage("Invalid GeoCoordinate value: 'text'")
                        .Invalid("42.6975").WithExpectedMessage("Invalid GeoCoordinate value: '42.6975'")
                        .Invalid("42.6975,").WithExpectedMessage("Invalid GeoCoordinate value: '42.6975,'")
                        .Invalid("").WithExpectedMessage("Invalid GeoCoordinate value: ''"))

                    .AddSelect(s => s
                        .Valid("john_doe")
                        .Valid("user123")
                        .Valid("qa_tester")
                        .Valid("dev_user1")
                        .Invalid("admin!").WithExpectedMessage("Invalid Username value: 'admin!'")
                        .Invalid("user name").WithExpectedMessage("Invalid Username value: 'user name'")
                        .Invalid("root$").WithExpectedMessage("Invalid Username value: 'root$'")
                        .Invalid("").WithExpectedMessage("Invalid Username value: ''"))

                    .AddSelect(s => s
                        .Valid("https://www.google.com")
                        .Valid("http://example.org")
                        .Valid("https://sub.domain.co.uk")
                        .Invalid("www.google.com").WithExpectedMessage("Invalid URL value: 'www.google.com'")
                        .Invalid("http:/invalid.com").WithExpectedMessage("Invalid URL value: 'http:/invalid.com'")
                        .Invalid("ftp://wrong.protocol").WithExpectedMessage("Invalid URL value: 'ftp://wrong.protocol'")
                        .Invalid("://missing.scheme.com").WithExpectedMessage("Invalid URL value: '://missing.scheme.com'")
                        .Invalid("").WithExpectedMessage("Invalid URL value: ''"))

                    .AddSelect(s => s
                        .Valid("#FF0000")
                        .Valid("#00FF00")
                        .Valid("#0000FF")
                        .Valid("#123ABC")
                        .Valid("#000000")
                        .Valid("#FFFFFF")
                        .Invalid("FF0000").WithExpectedMessage("Invalid Color value: 'FF0000'")
                        .Invalid("#GGGGGG").WithExpectedMessage("Invalid Color value: '#GGGGGG'")
                        .Invalid("#12345").WithExpectedMessage("Invalid Color value: '#12345'")
                        .Invalid("#1234567").WithExpectedMessage("Invalid Color value: '#1234567'")
                        .Invalid("red").WithExpectedMessage("Invalid Color value: 'red'")
                        .Invalid("").WithExpectedMessage("Invalid Color value: ''")),
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
    }
}