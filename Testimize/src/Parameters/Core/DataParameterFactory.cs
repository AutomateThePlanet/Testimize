// <copyright file="DataParameterFactory.cs" company="Automate The Planet Ltd.">
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

using Testimize.Contracts;
using Testimize.Parameters.Core;
using Testimize.TestValueProviders.Strategies;

namespace Testimize.Parameters.Core;

public static class DataParameterFactory
{
    // Dictionary for case-insensitive and flexible parameter type mapping
    private static readonly Dictionary<string, Func<UniversalDataParameter, IInputParameter>> ParameterFactories 
        = new(StringComparer.OrdinalIgnoreCase)
        {
            // Full type names
            ["Testimize.Parameters.TextDataParameter"] = CreateTextParameter,
            ["Testimize.Parameters.IntegerDataParameter"] = CreateIntegerParameter,
            ["Testimize.Parameters.DateTimeDataParameter"] = CreateDateTimeParameter,
            ["Testimize.Parameters.DateDataParameter"] = CreateDateParameter,
            ["Testimize.Parameters.TimeDataParameter"] = CreateTimeParameter,
            ["Testimize.Parameters.BooleanDataParameter"] = CreateBooleanParameter,
            ["Testimize.Parameters.CurrencyDataParameter"] = CreateCurrencyParameter,
            ["Testimize.Parameters.UsernameDataParameter"] = CreateUsernameParameter,
            ["Testimize.Parameters.EmailDataParameter"] = CreateEmailParameter,
            ["Testimize.Parameters.AddressDataParameter"] = CreateAddressParameter,
            ["Testimize.Parameters.ColorDataParameter"] = CreateColorParameter,
            ["Testimize.Parameters.MonthDataParameter"] = CreateMonthParameter,
            ["Testimize.Parameters.MultiSelectDataParameter"] = CreateMultiSelectParameter,
            ["Testimize.Parameters.PasswordDataParameter"] = CreatePasswordParameter,
            ["Testimize.Parameters.PercentageDataParameter"] = CreatePercentageParameter,
            ["Testimize.Parameters.PhoneDataParameter"] = CreatePhoneParameter,
            ["Testimize.Parameters.SingleSelectDataParameter"] = CreateSingleSelectParameter,
            ["Testimize.Parameters.UrlDataParameter"] = CreateUrlParameter,
            ["Testimize.Parameters.WeekDataParameter"] = CreateWeekParameter,
            ["Testimize.Parameters.GeoCoordinateDataParameter"] = CreateGeoCoordinateParameter,
            
            // Short names (class names only)
            ["TextDataParameter"] = CreateTextParameter,
            ["IntegerDataParameter"] = CreateIntegerParameter,
            ["DateTimeDataParameter"] = CreateDateTimeParameter,
            ["DateDataParameter"] = CreateDateParameter,
            ["TimeDataParameter"] = CreateTimeParameter,
            ["BooleanDataParameter"] = CreateBooleanParameter,
            ["CurrencyDataParameter"] = CreateCurrencyParameter,
            ["UsernameDataParameter"] = CreateUsernameParameter,
            ["EmailDataParameter"] = CreateEmailParameter,
            ["AddressDataParameter"] = CreateAddressParameter,
            ["ColorDataParameter"] = CreateColorParameter,
            ["MonthDataParameter"] = CreateMonthParameter,
            ["MultiSelectDataParameter"] = CreateMultiSelectParameter,
            ["PasswordDataParameter"] = CreatePasswordParameter,
            ["PercentageDataParameter"] = CreatePercentageParameter,
            ["PhoneDataParameter"] = CreatePhoneParameter,
            ["SingleSelectDataParameter"] = CreateSingleSelectParameter,
            ["UrlDataParameter"] = CreateUrlParameter,
            ["WeekDataParameter"] = CreateWeekParameter,
            ["GeoCoordinateDataParameter"] = CreateGeoCoordinateParameter,
            
            // Even shorter aliases
            ["Text"] = CreateTextParameter,
            ["Integer"] = CreateIntegerParameter,
            ["Int"] = CreateIntegerParameter,
            ["DateTime"] = CreateDateTimeParameter,
            ["Date"] = CreateDateParameter,
            ["Time"] = CreateTimeParameter,
            ["Boolean"] = CreateBooleanParameter,
            ["Bool"] = CreateBooleanParameter,
            ["Currency"] = CreateCurrencyParameter,
            ["Username"] = CreateUsernameParameter,
            ["Email"] = CreateEmailParameter,
            ["Address"] = CreateAddressParameter,
            ["Color"] = CreateColorParameter,
            ["Month"] = CreateMonthParameter,
            ["MultiSelect"] = CreateMultiSelectParameter,
            ["Password"] = CreatePasswordParameter,
            ["Percentage"] = CreatePercentageParameter,
            ["Phone"] = CreatePhoneParameter,
            ["SingleSelect"] = CreateSingleSelectParameter,
            ["Url"] = CreateUrlParameter,
            ["Week"] = CreateWeekParameter,
            ["GeoCoordinate"] = CreateGeoCoordinateParameter,
        };

    public static IInputParameter CreateFromUniversal(UniversalDataParameter universal)
    {
        if (string.IsNullOrWhiteSpace(universal.ParameterType))
        {
            throw new ArgumentException("ParameterType cannot be null or empty", nameof(universal));
        }

        // Try to find factory function using case-insensitive lookup
        if (ParameterFactories.TryGetValue(universal.ParameterType, out var factory))
        {
            return factory(universal);
        }

        // If not found, provide helpful error message with suggestions
        var availableTypes = string.Join(", ", ParameterFactories.Keys.Take(10));
        throw new ArgumentException($"Unknown parameter type: '{universal.ParameterType}'. " +
                                  $"Available types include: {availableTypes}... " +
                                  $"(case-insensitive, supports full names, class names, and short aliases)");
    }

    private static TextDataParameter CreateTextParameter(UniversalDataParameter universal)
    {
        var parameter = new TextDataParameter();
        var strategy = new TextDataProviderStrategy(
            universal.GetMinBoundary<int>(),
            universal.GetMaxBoundary<int>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static IntegerDataParameter CreateIntegerParameter(UniversalDataParameter universal)
    {
        var parameter = new IntegerDataParameter();
        var strategy = new IntegerDataProviderStrategy(
            universal.GetMinBoundary<int>(),
            universal.GetMaxBoundary<int>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static DateTimeDataParameter CreateDateTimeParameter(UniversalDataParameter universal)
    {
        var parameter = new DateTimeDataParameter();
        var strategy = new DateTimeDataProviderStrategy(
            universal.GetMinBoundary<DateTime>(),
            universal.GetMaxBoundary<DateTime>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static DateDataParameter CreateDateParameter(UniversalDataParameter universal)
    {
        var parameter = new DateDataParameter();
        var strategy = new DateDataProviderStrategy(
            universal.GetMinBoundary<DateTime>(),
            universal.GetMaxBoundary<DateTime>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static TimeDataParameter CreateTimeParameter(UniversalDataParameter universal)
    {
        var parameter = new TimeDataParameter();
        var strategy = new TimeDataProviderStrategy(
            universal.GetMinBoundary<TimeSpan>(),
            universal.GetMaxBoundary<TimeSpan>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static BooleanDataParameter CreateBooleanParameter(UniversalDataParameter universal)
    {
        var parameter = new BooleanDataParameter();
        var strategy = new BooleanDataProviderStrategy();
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static CurrencyDataParameter CreateCurrencyParameter(UniversalDataParameter universal)
    {
        var parameter = new CurrencyDataParameter();
        var strategy = new CurrencyDataProviderStrategy(
            universal.GetMinBoundary<decimal>(),
            universal.GetMaxBoundary<decimal>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static UsernameDataParameter CreateUsernameParameter(UniversalDataParameter universal)
    {
        var parameter = new UsernameDataParameter();
        var strategy = new UsernameDataProviderStrategy(
            universal.GetMinBoundary<int>(),
            universal.GetMaxBoundary<int>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static EmailDataParameter CreateEmailParameter(UniversalDataParameter universal)
    {
        var parameter = new EmailDataParameter();
        var strategy = new EmailDataProviderStrategy(
            universal.GetMinBoundary<int>(),
            universal.GetMaxBoundary<int>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static AddressDataParameter CreateAddressParameter(UniversalDataParameter universal)
    {
        var parameter = new AddressDataParameter();
        var strategy = new AddressDataProviderStrategy(
            universal.GetMinBoundary<int>(),
            universal.GetMaxBoundary<int>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static ColorDataParameter CreateColorParameter(UniversalDataParameter universal)
    {
        var parameter = new ColorDataParameter();
        var strategy = new ColorDataProviderStrategy();
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static MonthDataParameter CreateMonthParameter(UniversalDataParameter universal)
    {
        var parameter = new MonthDataParameter();
        var strategy = new MonthDataProviderStrategy(
            universal.GetMinBoundary<DateTime>(),
            universal.GetMaxBoundary<DateTime>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static MultiSelectDataParameter CreateMultiSelectParameter(UniversalDataParameter universal)
    {
        var parameter = new MultiSelectDataParameter();
        var strategy = new MultiSelectDataProviderStrategy();
        
        // Set options if provided
        if (universal.Options != null && universal.Options.Length > 0)
        {
            // Need to set options on the strategy or parameter
            // This might require modifying the strategy to accept options
        }
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static PasswordDataParameter CreatePasswordParameter(UniversalDataParameter universal)
    {
        var parameter = new PasswordDataParameter();
        var strategy = new PasswordDataProviderStrategy(
            universal.GetMinBoundary<int>(),
            universal.GetMaxBoundary<int>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static PercentageDataParameter CreatePercentageParameter(UniversalDataParameter universal)
    {
        var parameter = new PercentageDataParameter();
        var strategy = new PercentageDataProviderStrategy(
            universal.GetMinBoundary<decimal>(),
            universal.GetMaxBoundary<decimal>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static PhoneDataParameter CreatePhoneParameter(UniversalDataParameter universal)
    {
        var parameter = new PhoneDataParameter();
        var strategy = new PhoneDataProviderStrategy(
            universal.GetMinBoundary<int>(),
            universal.GetMaxBoundary<int>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static SingleSelectDataParameter CreateSingleSelectParameter(UniversalDataParameter universal)
    {
        var parameter = new SingleSelectDataParameter();
        var strategy = new SingleSelectDataProviderStrategy();
        
        // Set options if provided
        if (universal.Options != null && universal.Options.Length > 0)
        {
            // Need to set options on the strategy or parameter
            // This might require modifying the strategy to accept options
        }
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static UrlDataParameter CreateUrlParameter(UniversalDataParameter universal)
    {
        var parameter = new UrlDataParameter();
        var strategy = new UrlDataProviderStrategy(
            universal.GetMinBoundary<int>(),
            universal.GetMaxBoundary<int>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static WeekDataParameter CreateWeekParameter(UniversalDataParameter universal)
    {
        var parameter = new WeekDataParameter();
        var strategy = new WeekDataProviderStrategy(
            universal.GetMinBoundary<DateTime>(),
            universal.GetMaxBoundary<DateTime>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }

    private static GeoCoordinateDataParameter CreateGeoCoordinateParameter(UniversalDataParameter universal)
    {
        var parameter = new GeoCoordinateDataParameter();
        var strategy = new GeoCoordinateDataProviderStrategy(
            universal.GetMinBoundary<double>(),
            universal.GetMaxBoundary<double>());
        
        parameter.Initialize(
            strategy,
            universal.PreciseMode,
            universal.IncludeBoundaryValues,
            universal.AllowValidEquivalenceClasses,
            universal.AllowInvalidEquivalenceClasses,
            universal.PreciseTestValues);
        
        return parameter;
    }
}