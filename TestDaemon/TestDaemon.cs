using System;
using System.Configuration;
using Bluewire.Common.Console;
using Bluewire.Common.Console.ThirdParty;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TestDaemon
{
    /// <summary>
    /// Daemon for testing hosting of EXE assemblies.
    /// If the --key argument is provided, asserts that the appSettings key of the same name has a value
    /// matching that specified by the --value argument. Throws an exception on failure.
    /// </summary>
    public class TestDaemon : IDaemonisable<TestDaemonArguments>
    {
        public string Name
        {
            get { return "TestDaemon"; }
        }

        public SessionArguments<TestDaemonArguments> Configure()
        {
            var arguments = new TestDaemonArguments();
            return new SessionArguments<TestDaemonArguments>(arguments, new OptionSet
            {
                { "key=", s => arguments.ExpectedConfigKey = s },
                { "value=", s => arguments.ExpectedConfigValue = s },
                { "environment-exit=", (int exitCode) => arguments.EnvironmentExitCode = exitCode }
            });
        }

        public Task<IDaemon> Start(TestDaemonArguments args, CancellationToken token)
        {
            if (args.ExpectedConfigKey != null)
            {
                var configValue = ConfigurationManager.AppSettings[args.ExpectedConfigKey] ?? "";
                if (configValue != args.ExpectedConfigValue) throw new Exception("Test");
            }
            if (args.EnvironmentExitCode.HasValue)
            {
                Environment.Exit(args.EnvironmentExitCode.Value);
            }
            return Task.FromResult<IDaemon>(new Implementation());
        }

        public string[] GetDependencies()
        {
            return new string[0];
        }

        class Implementation : IDaemon
        {
            public void Dispose()
            {
            }
        }

    }

    public class TestDaemonArguments
    {
        public string ExpectedConfigKey { get; set; }
        public string ExpectedConfigValue { get; set; }
        public int? EnvironmentExitCode {get; set;}
    }
}
