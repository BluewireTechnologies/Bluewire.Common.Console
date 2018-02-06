using Bluewire.Common.Console.Logging;

namespace Bluewire.Common.Console.Environment
{
    public interface IExecutionEnvironment
    {
        OutputDescriptorBase CreateOutputDescriptor();
    }
}
