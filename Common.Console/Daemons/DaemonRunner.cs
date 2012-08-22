using System.Linq;
using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console.Daemons
{
    public class DaemonRunner<T>
    {
        private readonly IExecutionEnvironment executionEnvironment;
        private readonly IRunAsConsoleApplication runAsConsoleApplication;
        private readonly IRunAsService runAsService;
        private readonly IRunAsServiceInstaller runAsServiceInstaller;

        public DaemonRunner(
            IExecutionEnvironment executionEnvironment,
            IRunAsConsoleApplication runAsConsoleApplication,
            IRunAsService runAsService,
            IRunAsServiceInstaller runAsServiceInstaller)
        {
            this.executionEnvironment = executionEnvironment;
            this.runAsConsoleApplication = runAsConsoleApplication;
            this.runAsService = runAsService;
            this.runAsServiceInstaller = runAsServiceInstaller;
        }

        public int Run(IDaemonisable<T> daemon, params string[] args)
        {
            if (executionEnvironment.IsRunningAsService())
            {
                runAsService.Run(daemon, args);
                return 0;
            }

            var serviceInstallerArguments = new ServiceInstallerArguments<T>(daemon);

            var sessionArguments = AddInstallerOptions(daemon.Configure(), serviceInstallerArguments);
            var session = new ConsoleSession<T>(sessionArguments);
            return session.Run(args, a =>
            {
                if (serviceInstallerArguments.ServiceInstallationRequested)
                {
                    // reparse, stripping out only the installer-related arguments.
                    var consoleArguments = AddInstallerOptions(new SessionArguments(new OptionSet()), new ServiceInstallerArguments<T>(daemon)).Strip(args);
                    return runAsServiceInstaller.Run(serviceInstallerArguments, consoleArguments);
                }
                return runAsConsoleApplication.Run(daemon, a);
            });
        }

        private TArgs AddInstallerOptions<TArgs, T>(TArgs sessionArguments, ServiceInstallerArguments<T> serviceInstallerArguments) where TArgs : SessionArguments
        {
            sessionArguments.Options.Add("install", i => serviceInstallerArguments.Install());
            sessionArguments.Options.Add("uninstall", i => serviceInstallerArguments.Uninstall());
            sessionArguments.Options.Add("reinstall", i => serviceInstallerArguments.Reinstall());
            sessionArguments.Options.Add("service-name=", i => serviceInstallerArguments.ServiceName = i);
            sessionArguments.Options.Add("service-user=", i => serviceInstallerArguments.ServiceUser = i);
            sessionArguments.Options.Add("service-password=", i => serviceInstallerArguments.ServicePassword = i);
            return sessionArguments;
        }

    }
}