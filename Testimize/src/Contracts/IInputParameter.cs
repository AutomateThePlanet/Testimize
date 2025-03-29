using Testimize.Parameters.Core;

namespace Testimize.Contracts;
public interface IInputParameter
{
    List<TestValue> TestValues { get; }
}