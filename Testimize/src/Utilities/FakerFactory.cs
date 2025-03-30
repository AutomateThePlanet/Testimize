// // <copyright file="FakerFactory.cs" company="Automate The Planet Ltd.">
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

using Bogus;

namespace Testimize.Utilities;

public static class FakerFactory
{
    private static readonly object _lock = new();
    private static Faker _sharedFaker;
    private static int? _seed;
    private static string _locale;

    /// <summary>
    /// Initializes the FakerFactory with a global seed (optional).
    /// Should be called once during test initialization.
    /// </summary>
    public static void Initialize(string locale = null, int? seed = null)
    {
        lock (_lock)
        {
            _locale = locale ?? Settings.GetSection<TestimizeSettings>().Locale;
            _seed = seed ?? Settings.GetSection<TestimizeSettings>().Seed;

            if (_seed.HasValue)
            {
                Randomizer.Seed = new Random(_seed.Value);
            }

            _sharedFaker = new Faker(locale: _locale);
        }
    }

    /// <summary>
    /// Gets the globally shared Faker instance.
    /// </summary>
    public static Faker GetFaker()
    {
        if (_sharedFaker == null)
        {
            // Default to unseeded, but allow lazy fallback
            Initialize();
        }

        return _sharedFaker;
    }

    /// <summary>
    /// Creates a new independent Faker (optionally with seed).
    /// </summary>
    public static Faker CreateNew(string locale = null, int? seed = null)
    {
       
        if (_seed.HasValue)
        {
            _locale = locale ?? Settings.GetSection<TestimizeSettings>().Locale;
            _seed = seed ?? Settings.GetSection<TestimizeSettings>().Seed;
            Randomizer.Seed = new Random(_seed.Value);
        }

        var newFaker = new Faker(locale: _locale);

        return newFaker;
    }
}