// // <copyright file="TestimizeInputBuilder.cs" company="Automate The Planet Ltd.">
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

using Testimize.Contracts;

namespace Testimize.Parameters.Core;

public class TestimizeInputBuilder
{
    private readonly List<IInputParameter> _parameters = new();

    public static TestimizeInputBuilder Start() => new();

    public List<IInputParameter> Build() => _parameters;

    // --- Address ---
    public TestimizeInputBuilder AddAddress(Func<ParameterBuilder<AddressDataParameter>, ParameterBuilder<AddressDataParameter>> configure)
    {
        var builder = new ParameterBuilder<AddressDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    // --- Boolean ---
    public TestimizeInputBuilder AddBoolean()
    {
        _parameters.Add(new BooleanDataParameter());
        return this;
    }

    // --- Color ---
    public TestimizeInputBuilder AddColor(Func<ParameterBuilder<ColorDataParameter>, ParameterBuilder<ColorDataParameter>> configure)
    {
        var builder = new ParameterBuilder<ColorDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    // --- Currency ---
    public TestimizeInputBuilder AddCurrency(Func<ParameterBuilder<CurrencyDataParameter>, ParameterBuilder<CurrencyDataParameter>> configure)
    {
        var builder = new ParameterBuilder<CurrencyDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    // --- Date ---
    public TestimizeInputBuilder AddDate(Func<ParameterBuilder<DateDataParameter>, ParameterBuilder<DateDataParameter>> configure)
    {
        var builder = new ParameterBuilder<DateDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    public TestimizeInputBuilder AddDate(DateTime min, DateTime max)
    {
        _parameters.Add(new DateDataParameter(minBoundary: min, maxBoundary: max));
        return this;
    }

    // --- DateTime ---
    public TestimizeInputBuilder AddDateTime(Func<ParameterBuilder<DateTimeDataParameter>, ParameterBuilder<DateTimeDataParameter>> configure)
    {
        var builder = new ParameterBuilder<DateTimeDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    public TestimizeInputBuilder AddDateTime(DateTime min, DateTime max)
    {
        _parameters.Add(new DateTimeDataParameter(minBoundary: min, maxBoundary: max));
        return this;
    }

    // --- Email ---
    public TestimizeInputBuilder AddEmail(Func<ParameterBuilder<EmailDataParameter>, ParameterBuilder<EmailDataParameter>> configure)
    {
        var builder = new ParameterBuilder<EmailDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    public TestimizeInputBuilder AddEmail(int min, int max)
    {
        _parameters.Add(new EmailDataParameter(minBoundary: min, maxBoundary: max));
        return this;
    }

    // --- GeoCoordinate ---
    public TestimizeInputBuilder AddGeoCoordinate(Func<ParameterBuilder<GeoCoordinateDataParameter>, ParameterBuilder<GeoCoordinateDataParameter>> configure)
    {
        var builder = new ParameterBuilder<GeoCoordinateDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    // --- Integer ---
    public TestimizeInputBuilder AddInteger(Func<ParameterBuilder<IntegerDataParameter>, ParameterBuilder<IntegerDataParameter>> configure)
    {
        var builder = new ParameterBuilder<IntegerDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    public TestimizeInputBuilder AddInteger(int min, int max)
    {
        _parameters.Add(new IntegerDataParameter(minBoundary: min, maxBoundary: max));
        return this;
    }

    // --- Month ---
    public TestimizeInputBuilder AddMonth(Func<ParameterBuilder<MonthDataParameter>, ParameterBuilder<MonthDataParameter>> configure)
    {
        var builder = new ParameterBuilder<MonthDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    // --- MultiSelect ---
    public TestimizeInputBuilder AddMultiSelect(Func<ParameterBuilder<MultiSelectDataParameter>, ParameterBuilder<MultiSelectDataParameter>> configure)
    {
        var builder = new ParameterBuilder<MultiSelectDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    // --- Password ---
    public TestimizeInputBuilder AddPassword(Func<ParameterBuilder<PasswordDataParameter>, ParameterBuilder<PasswordDataParameter>> configure)
    {
        var builder = new ParameterBuilder<PasswordDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    public TestimizeInputBuilder AddPassword(int min, int max)
    {
        _parameters.Add(new PasswordDataParameter(minBoundary: min, maxBoundary: max));
        return this;
    }

    // --- Percentage ---
    public TestimizeInputBuilder AddPercentage(Func<ParameterBuilder<PercentageDataParameter>, ParameterBuilder<PercentageDataParameter>> configure)
    {
        var builder = new ParameterBuilder<PercentageDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    public TestimizeInputBuilder AddPercentage(int min, int max)
    {
        _parameters.Add(new PercentageDataParameter(minBoundary: min, maxBoundary: max));
        return this;
    }

    // --- Phone ---
    public TestimizeInputBuilder AddPhone(Func<ParameterBuilder<PhoneDataParameter>, ParameterBuilder<PhoneDataParameter>> configure)
    {
        var builder = new ParameterBuilder<PhoneDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    public TestimizeInputBuilder AddPhone(int min, int max)
    {
        _parameters.Add(new PhoneDataParameter(minBoundary: min, maxBoundary: max));
        return this;
    }

    // --- SingleSelect ---
    public TestimizeInputBuilder AddSingleSelect(Func<ParameterBuilder<SingleSelectDataParameter>, ParameterBuilder<SingleSelectDataParameter>> configure)
    {
        var builder = new ParameterBuilder<SingleSelectDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    // --- Text ---
    public TestimizeInputBuilder AddText(Func<ParameterBuilder<TextDataParameter>, ParameterBuilder<TextDataParameter>> configure)
    {
        var builder = new ParameterBuilder<TextDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    public TestimizeInputBuilder AddText(int min, int max)
    {
        _parameters.Add(new TextDataParameter(minBoundary: min, maxBoundary: max));
        return this;
    }

    // --- Time ---
    public TestimizeInputBuilder AddTime(Func<ParameterBuilder<TimeDataParameter>, ParameterBuilder<TimeDataParameter>> configure)
    {
        var builder = new ParameterBuilder<TimeDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    public TestimizeInputBuilder AddTime(TimeSpan min, TimeSpan max)
    {
        _parameters.Add(new TimeDataParameter(minBoundary: min, maxBoundary: max));
        return this;
    }

    // --- Url ---
    public TestimizeInputBuilder AddUrl(Func<ParameterBuilder<UrlDataParameter>, ParameterBuilder<UrlDataParameter>> configure)
    {
        var builder = new ParameterBuilder<UrlDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    public TestimizeInputBuilder AddUrl(int min, int max)
    {
        _parameters.Add(new UrlDataParameter(minBoundary: min, maxBoundary: max));
        return this;
    }

    // --- Username ---
    public TestimizeInputBuilder AddUsername(Func<ParameterBuilder<UsernameDataParameter>, ParameterBuilder<UsernameDataParameter>> configure)
    {
        var builder = new ParameterBuilder<UsernameDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }

    public TestimizeInputBuilder AddUsername(int min, int max)
    {
        _parameters.Add(new UsernameDataParameter(minBoundary: min, maxBoundary: max));
        return this;
    }

    // --- Week ---
    public TestimizeInputBuilder AddWeek(Func<ParameterBuilder<WeekDataParameter>, ParameterBuilder<WeekDataParameter>> configure)
    {
        var builder = new ParameterBuilder<WeekDataParameter>();
        var configured = configure(builder);
        _parameters.Add(configured.Build());
        return this;
    }
}
