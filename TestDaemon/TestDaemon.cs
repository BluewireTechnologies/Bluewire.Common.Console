using System;
using System.Configuration;
using Bluewire.Common.Console;
using Bluewire.Common.Console.ThirdParty;
using System.Threading;
using System.Threading.Tasks;

namespace TestDaemon
{
    /// <summary>
    /// Daemon for testing hosting of EXE assemblies.
    /// If the --key argument is provided, asserts that the appSettings key of the same name has a value
    /// matching that specified by the --value argument. Throws an exception on failure.
    /// </summary>
    public class TestDaemon : IDaemonisable, IReceiveOptions
    {
        public string Name => "TestDaemon";

        public string ExpectedConfigKey { get; set; }
        public string ExpectedConfigValue { get; set; }
        public int? EnvironmentExitCode { get; set; }

        void IReceiveOptions.ReceiveFrom(OptionSet options)
        {
            options.Add("key=", s => ExpectedConfigKey = s);
            options.Add("value=", s => ExpectedConfigValue = s);
            options.Add("environment-exit=", (int exitCode) => EnvironmentExitCode = exitCode);
        }

        public Task<IDaemon> Start(CancellationToken token)
        {
            if (ExpectedConfigKey != null)
            {
                var configValue = ConfigurationManager.AppSettings[ExpectedConfigKey] ?? "";
                if (configValue != ExpectedConfigValue) throw new Exception("Test");
            }
            if (EnvironmentExitCode.HasValue)
            {
                Environment.Exit(EnvironmentExitCode.Value);
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
}
