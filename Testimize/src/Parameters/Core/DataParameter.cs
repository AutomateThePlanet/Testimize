// <copyright file="DataParameter.cs" company="Automate The Planet Ltd.">
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

namespace Testimize.Parameters.Core;

public abstract class DataParameter<TDataStrategy> : IInputParameter
    where TDataStrategy : class, IDataProviderStrategy
{
    private TDataStrategy? _dataProviderStrategy;
    private bool _preciseMode;
    private bool? _includeBoundaryValues;
    private bool? _allowValidEquivalenceClasses;
    private bool? _allowInvalidEquivalenceClasses;
    private List<TestValue> _testValues = new List<TestValue>();
    private TestValue[] _preciseTestValues = Array.Empty<TestValue>();

    public DataParameter(
        TDataStrategy? dataProviderStrategy = null,
        bool preciseMode = false,
        bool? includeBoundaryValues = null,
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
    {
        _dataProviderStrategy = dataProviderStrategy;
        _preciseMode = preciseMode;
        _includeBoundaryValues = includeBoundaryValues;
        _allowValidEquivalenceClasses = allowValidEquivalenceClasses;
        _allowInvalidEquivalenceClasses = allowInvalidEquivalenceClasses;
        _preciseTestValues = preciseTestValues ?? Array.Empty<TestValue>();

        if (dataProviderStrategy != null)
        {
            InitializeTestValues();
        }
    }

    // Parameterless constructor for deserialization
    protected DataParameter()
    {
    }

    public TDataStrategy? DataProviderStrategy 
    { 
        get => _dataProviderStrategy;
        protected set => _dataProviderStrategy = value;
    }
    
    public bool PreciseMode 
    { 
        get => _preciseMode;
        protected set => _preciseMode = value;
    }
    
    public bool? IncludeBoundaryValues 
    { 
        get => _includeBoundaryValues;
        protected set => _includeBoundaryValues = value;
    }
    
    public bool? AllowValidEquivalenceClasses 
    { 
        get => _allowValidEquivalenceClasses;
        protected set => _allowValidEquivalenceClasses = value;
    }
    
    public bool? AllowInvalidEquivalenceClasses 
    { 
        get => _allowInvalidEquivalenceClasses;
        protected set => _allowInvalidEquivalenceClasses = value;
    }

    public List<TestValue> TestValues 
    { 
        get => _testValues;
        set => _testValues = value ?? new List<TestValue>();
    }

    public TestValue[] PreciseTestValues 
    { 
        get => _preciseTestValues;
        set => _preciseTestValues = value ?? Array.Empty<TestValue>();
    }

    public abstract string ParameteryType { get; }

    public virtual void InitializeTestValues()
    {
        if (_dataProviderStrategy == null) return;

        _testValues = _dataProviderStrategy.GenerateTestValues(
            allowBoundaryValues: _preciseMode ? false : _includeBoundaryValues,
            allowValidEquivalenceClasses: _preciseMode ? false : _allowValidEquivalenceClasses,
            allowInvalidEquivalenceClasses: _preciseMode ? false : _allowInvalidEquivalenceClasses,
            _preciseTestValues);
    }

    public virtual void Initialize(
        TDataStrategy dataProviderStrategy,
        bool preciseMode = false,
        bool? includeBoundaryValues = null,
        bool? allowValidEquivalenceClasses = null,
        bool? allowInvalidEquivalenceClasses = null,
        params TestValue[] preciseTestValues)
    {
        _dataProviderStrategy = dataProviderStrategy;
        _preciseMode = preciseMode;
        _includeBoundaryValues = includeBoundaryValues;
        _allowValidEquivalenceClasses = allowValidEquivalenceClasses;
        _allowInvalidEquivalenceClasses = allowInvalidEquivalenceClasses;
        _preciseTestValues = preciseTestValues ?? Array.Empty<TestValue>();
        
        InitializeTestValues();
    }
}
