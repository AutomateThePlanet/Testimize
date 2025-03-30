// // <copyright file="ReqresRegistrationTests.cs" company="Automate The Planet Ltd.">
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

using Testimize.Parameters;
using RestSharp;
using System.Collections.Generic;
using System;
using Testimize.Contracts;
using Testimize.OutputGenerators;

namespace Testimize.Tests.RealWorld;

[TestFixture]
public class ReqresRegistrationTests
{
    public static List<IInputParameter> ABCGeneratedTestParameters()
    {
        return new List<IInputParameter>
        {
            new EmailDataParameter(minBoundary: 10, maxBoundary: 20),
            new PasswordDataParameter(minBoundary: 8, maxBoundary: 16)
        };
    }

    [Test]
    [ABCTestCaseSource(nameof(ABCGeneratedTestParameters), TestCaseCategory.Validation)]
    public void RegisterUser_WithGeneratedEmailAndPassword(string email, string password)
    {
        var client = new RestClient("https://reqres.in");
        var request = new RestRequest("/api/register", Method.Post);
        request.AddJsonBody(new
        {
            email,
            password
        });

        var response = client.Execute(request);

        Console.WriteLine($"→ Attempted registration with Email: {email}, Password: {password}");
        Console.WriteLine($"← Response: {response.StatusCode}, Content: {response.Content}");

        // Expect 200 for known valid email or 400 otherwise (per Reqres.in behavior)
        Assert.That((int)response.StatusCode, Is.EqualTo(200).Or.EqualTo(400));
    }
}
