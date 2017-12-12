using Bluewire.Common.Console;

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
