using Testimize.Contracts;
using Testimize.Parameters.Core;

public class MockInputParameter : IInputParameter
{
    public string ParameteryType => "MockInputParameter";

    public List<TestValue> TestValues { get; set; } = new List<TestValue>();
}