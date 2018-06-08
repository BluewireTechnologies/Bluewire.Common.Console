using System;

namespace Bluewire.Common.Console
{
    public class ExcessArgumentsException : ErrorWithReturnCodeException
    {
        public ExcessArgumentsException(string[] excessArguments)
            : base(1, "Excess arguments: {0}", String.Join(" ", excessArguments))
        {
            ShowUsage = true;
        }
    }
}
