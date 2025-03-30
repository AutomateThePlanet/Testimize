// // <copyright file="JsonPlaceholderPostTests.cs" company="Automate The Planet Ltd.">
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
using Testimize.Parameters.Core;

namespace Testimize.Tests.RealWorld;

[TestFixture]
public class JsonPlaceholderPostTests
{
    //public static List<IInputParameter> ABCGeneratedTestParameters()
    //{
    //    return new List<IInputParameter>
    //    {
    //        new TextDataParameter(minBoundary: 5, maxBoundary: 100),            // title
    //        new TextDataParameter(minBoundary: 20, maxBoundary: 500),           // body
    //        new IntegerDataParameter(minBoundary: 1, maxBoundary: 100),         // userId
    //        new UsernameDataParameter(minBoundary: 6, maxBoundary: 15),         // authorUsername
    //        new EmailDataParameter(minBoundary: 10, maxBoundary: 30),           // authorEmail
    //        new BooleanDataParameter(),                                         // isPublished
    //        new DateTimeDataParameter(minBoundary: DateTime.UtcNow.AddDays(-30), maxBoundary: DateTime.UtcNow) // publishDate
    //    };
    //}
    public static List<IInputParameter> ABCGeneratedTestParameters() =>
    TestimizeInputBuilder
        .Start()
        .AddText(5, 100)                        // title
        .AddText(20, 500)                       // body
        .AddInteger(1, 100)                     // userId
        .AddUsername(6, 15)                     // authorUsername
        .AddEmail(10, 30)                       // authorEmail
        .AddBoolean()                           // isPublished
        .AddDateTime(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow)                    // publishDate
        .Build();

    [Test]
    [ABCTestCaseSource(nameof(ABCGeneratedTestParameters), TestCaseCategory.Validation)]
    public void CreateFullPost_WithGeneratedMetadata_ShouldSucceed(
        string title,
        string body,
        string userId,
        string authorUsername,
        string authorEmail,
        string isPublished,
        string publishDate)
    {
        var client = new RestClient("https://jsonplaceholder.typicode.com");
        var request = new RestRequest("/posts", Method.Post);
        request.AddJsonBody(new
        {
            title,
            body,
            userId = int.Parse(userId),
            metadata = new
            {
                username = authorUsername,
                email = authorEmail,
                isPublished = bool.Parse(isPublished),
                publishDate = DateTime.Parse(publishDate)
            }
        });

        var response = client.Execute(request);

        Console.WriteLine($"→ Posting data: {title} by {authorUsername} on {publishDate}");
        Console.WriteLine($"← Response: {response.StatusCode}, Body: {response.Content}");

        Assert.That((int)response.StatusCode, Is.EqualTo(201));
    }

}
