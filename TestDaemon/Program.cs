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
        static void Main(string[] args)
        {
            DaemonRunner.Run(args, new TestDaemon());
        }
    }
}
