using System;
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

        private static TArgs AddInstallerOptions<TArgs>(TArgs sessionArguments, ServiceInstallerArguments<T> serviceInstallerArguments) where TArgs : SessionArguments
        {
            sessionArguments.Options.Add("install", "Install this application as a service.", i => serviceInstallerArguments.Install());
            sessionArguments.Options.Add("uninstall", "Uninstall this service.", i => serviceInstallerArguments.Uninstall());
            sessionArguments.Options.Add("reinstall", "Reinstall this service.", i => serviceInstallerArguments.Reinstall());
            sessionArguments.Options.Add("service-name=", String.Format("Use the specified name for the service. Default: {0}", serviceInstallerArguments.ServiceName), i => serviceInstallerArguments.ServiceName = i);
            sessionArguments.Options.Add("service-user=", "Run the service under the specified account. Default: LocalService", i => serviceInstallerArguments.ServiceUser = i);
            sessionArguments.Options.Add("service-password=", i => serviceInstallerArguments.ServicePassword = i);
            sessionArguments.Options.Add("depends-on=", "Depends on service name", i => serviceInstallerArguments.DependsOn = i);
            return sessionArguments;
        }

    }
}