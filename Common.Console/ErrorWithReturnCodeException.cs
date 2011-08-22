using System;

namespace Bluewire.Common.Console
{
    public class ErrorWithReturnCodeException : ApplicationException
    {
        public int ExitCode { get; private set; }

        public bool ShowUsage { get; protected set; }

        public ErrorWithReturnCodeException(int code, string format, params object[] args) : base(String.Format(format, args))
        {
            ExitCode = code;
        }
    }
}