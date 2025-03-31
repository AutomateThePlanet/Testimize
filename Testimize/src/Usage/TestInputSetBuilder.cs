// <copyright file="TestInputSetBuilder.cs" company="Automate The Planet Ltd.">
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
using Testimize.Parameters;
using Testimize.Parameters.Core;

namespace Testimize.Usage;

public class TestInputSetBuilder
{
    private readonly TestimizeInputBuilder _composer = TestimizeInputBuilder.Start();

    public TestInputSetBuilder AddText(Func<ParameterBuilder<TextDataParameter>, ParameterBuilder<TextDataParameter>> configure)
    {
        _composer.AddText(configure);
        return this;
    }

    public TestInputSetBuilder AddEmail(Func<ParameterBuilder<EmailDataParameter>, ParameterBuilder<EmailDataParameter>> configure)
    {
        _composer.AddEmail(configure);
        return this;
    }

    public TestInputSetBuilder AddPhone(Func<ParameterBuilder<PhoneDataParameter>, ParameterBuilder<PhoneDataParameter>> configure)
    {
        _composer.AddPhone(configure);
        return this;
    }

    public TestInputSetBuilder AddBoolean()
    {
        _composer.AddBoolean();
        return this;
    }

    public TestInputSetBuilder AddBoolean(Func<ParameterBuilder<BooleanDataParameter>, ParameterBuilder<BooleanDataParameter>> configure)
    {
        var builder = new ParameterBuilder<BooleanDataParameter>();
        _composer.AddBoolean(configure);
        return this;
    }

    public TestInputSetBuilder AddUsername(Func<ParameterBuilder<UsernameDataParameter>, ParameterBuilder<UsernameDataParameter>> configure)
    {
        _composer.AddUsername(configure);
        return this;
    }

    public TestInputSetBuilder AddPassword(Func<ParameterBuilder<PasswordDataParameter>, ParameterBuilder<PasswordDataParameter>> configure)
    {
        _composer.AddPassword(configure);
        return this;
    }

    public TestInputSetBuilder AddUrl(Func<ParameterBuilder<UrlDataParameter>, ParameterBuilder<UrlDataParameter>> configure)
    {
        _composer.AddUrl(configure);
        return this;
    }

    public TestInputSetBuilder AddColor(Func<ParameterBuilder<ColorDataParameter>, ParameterBuilder<ColorDataParameter>> configure)
    {
        _composer.AddColor(configure);
        return this;
    }

    public TestInputSetBuilder AddCurrency(Func<ParameterBuilder<CurrencyDataParameter>, ParameterBuilder<CurrencyDataParameter>> configure)
    {
        _composer.AddCurrency(configure);
        return this;
    }

    public TestInputSetBuilder AddDate(Func<ParameterBuilder<DateDataParameter>, ParameterBuilder<DateDataParameter>> configure)
    {
        _composer.AddDate(configure);
        return this;
    }

    public TestInputSetBuilder AddInteger(Func<ParameterBuilder<IntegerDataParameter>, ParameterBuilder<IntegerDataParameter>> configure)
    {
        _composer.AddInteger(configure);
        return this;
    }

    public TestInputSetBuilder AddGeoCoordinate(Func<ParameterBuilder<GeoCoordinateDataParameter>, ParameterBuilder<GeoCoordinateDataParameter>> configure)
    {
        _composer.AddGeoCoordinate(configure);
        return this;
    }

    public TestInputSetBuilder AddAddress(Func<ParameterBuilder<AddressDataParameter>, ParameterBuilder<AddressDataParameter>> configure)
    {
        _composer.AddAddress(configure);
        return this;
    }

    public TestInputSetBuilder AddSingleSelect(Func<ParameterBuilder<SingleSelectDataParameter>, ParameterBuilder<SingleSelectDataParameter>> configure)
    {
        _composer.AddSingleSelect(configure);
        return this;
    }

    public TestInputSetBuilder AddMultiSelect(Func<ParameterBuilder<MultiSelectDataParameter>, ParameterBuilder<MultiSelectDataParameter>> configure)
    {
        var builder = new ParameterBuilder<MultiSelectDataParameter>();
        _composer.AddMultiSelect(configure);
        return this;
    }

    public List<IInputParameter> Build() => _composer.Build();
}
