using System;
using System.IO;

namespace Bluewire.Common.Console.Environment
{
    public class RedirectConsoleScope : IDisposable
    {
        public RedirectConsoleScope(TextWriter stdout, TextWriter stderr)
        {
            if (stdout == null) throw new ArgumentNullException(nameof(stdout));
            if (stderr == null) throw new ArgumentNullException(nameof(stderr));

            System.Console.SetOut(stdout);
            System.Console.SetError(stderr);
        }

        public void Dispose()
        {
            System.Console.SetOut(new StreamWriter(System.Console.OpenStandardOutput()));
            System.Console.SetError(new StreamWriter(System.Console.OpenStandardError()));
        }
    }
}
