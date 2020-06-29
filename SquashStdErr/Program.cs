using System;
using System.Linq;
using Bluewire.Common.Console.Client.Shell;

namespace SquashStdErr
{
    class Program
    {
        static int Main(string[] args)
        {
            var cmd = new CommandLine(args.First());
            cmd.AddList(args.Skip(1));
            var process = cmd.Run();
            process.StdOut.Subscribe(Console.WriteLine);
            process.StdErr.Subscribe(Console.WriteLine);
            return process.Completed.Result;
        }
    }
}
