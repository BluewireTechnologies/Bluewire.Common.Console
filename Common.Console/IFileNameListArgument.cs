using System.Collections.Generic;

namespace Bluewire.Common.Console
{
    public interface IFileNameListArgument
    {
        IList<string> FileNames { get; }
    }
}