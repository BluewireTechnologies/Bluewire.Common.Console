﻿using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace Bluewire.Common.Console.Client.Shell
{
    public class SimpleConsoleInvocationLogger : IConsoleInvocationLogger
    {
        private readonly TextWriter log;

        public SimpleConsoleInvocationLogger(TextWriter log)
        {
            this.log = log;
        }

        private void WriteLine(string line)
        {
            lock (this) log.WriteLine(line);
        }

        public IConsoleInvocationLogScope LogInvocation(IConsoleProcess process)
        {
            WriteLine($"[Shell]  {process.CommandLine}");

            var stdout = process.StdOut.Select(l => $"  [Out]  {l}");
            var stderr = process.StdErr.Select(l => $"  [Err]  {l}");

            var logger = Observer.Create<string>(WriteLine);

            return new ConsoleInvocationLogScope(stdout.Merge(stderr).Subscribe(logger));
        }

        public IConsoleInvocationLogScope LogMinorInvocation(IConsoleProcess process)
        {
            return LogInvocation(process);
        }
    }
}
