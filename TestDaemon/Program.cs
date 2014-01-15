using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bluewire.Common.Console;
using Bluewire.Common.Console.Daemons;

namespace TestDaemon
{
    class Program
    {
        static int Main(string[] args)
        {
            return DaemonRunner.Run(args, new TestDaemon());
        }
    }
}
