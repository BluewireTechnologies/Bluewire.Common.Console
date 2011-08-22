using System;

namespace Bluewire.Common.Console
{
    public class InvalidArgumentsException : ErrorWithReturnCodeException
    {
        public InvalidArgumentsException(Exception ex) : this("{0}", ex.Message)
        {
        }

        public InvalidArgumentsException(string format, params object[] args)
            : base(1, format, args)
        {
            ShowUsage = true;
        }
    }
}