using System;
using System.Linq;
using Bluewire.Common.Console.Environment;
using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console.Daemons
{
    public class DaemonRunner<T>
    {
        private readonly IRunAsConsoleApplication runAsConsoleApplication;
        private readonly IRunAsService runAsService;
        private readonly IRunAsServiceInstaller runAsServiceInstaller;
        private readonly IRunAsHostedService runAsHostedService;

        public DaemonRunner(
            IRunAsConsoleApplication runAsConsoleApplication,
            IRunAsService runAsService,
            IRunAsServiceInstaller runAsServiceInstaller,
            IRunAsHostedService runAsHostedService)
        {
            this.runAsConsoleApplication = runAsConsoleApplication;
            this.runAsService = runAsService;
            this.runAsServiceInstaller = runAsServiceInstaller;
            this.runAsHostedService = runAsHostedService;
        }

        public int Run(IExecutionEnvironment environment, IDaemonisable<T> daemon, params string[] args)
        {
            var serviceEnvironment = environment as ServiceEnvironment;
            if (serviceEnvironment != null)
            {
                return runAsService.Run(serviceEnvironment, daemon, args);
            }

            var applicationEnvironment = environment as ApplicationEnvironment;
            if (applicationEnvironment != null)
            {
                return RunInApplicationEnvironment(applicationEnvironment, daemon, args);
            }

            var hostedEnvironment = environment as InitialisedHostedEnvironment;
            if (hostedEnvironment != null)
            {
                var session = daemon.Configure();
                session.Parse(args);
                return runAsHostedService.Run(hostedEnvironment, daemon, session.Arguments);
            }

            // Exit code 10 is a Windows standard exit code meaning 'The environment is incorrect'.
            // Try 'net helpmsg 10' at the shell.
            return 10;
        }


        private int RunInApplicationEnvironment(ApplicationEnvironment applicationEnvironment, IDaemonisable<T> daemon, string[] args)
        {
            var serviceInstallerArguments = new ServiceInstallerArguments<T>(daemon);

            var sessionArguments = AddInstallerOptions(daemon.Configure(), serviceInstallerArguments);
            var session = new ConsoleSession<T>(sessionArguments);
            return session.Run(args, a =>
            {
                if (serviceInstallerArguments.ServiceInstallationRequested)
                {
                    // reparse, stripping out only the installer-related arguments.
                    var consoleArguments =
                        AddInstallerOptions(new SessionArguments(new OptionSet()), new ServiceInstallerArguments<T>(daemon))
                            .Strip(args);
                    return runAsServiceInstaller.Run(applicationEnvironment, serviceInstallerArguments, consoleArguments);
                }
                return runAsConsoleApplication.Run(applicationEnvironment, daemon, a);
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
            return sessionArguments;
        }

    }
}