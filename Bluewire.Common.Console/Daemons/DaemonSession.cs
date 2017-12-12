using System;
using System.Linq;
using Bluewire.Common.Console.Arguments;
using Bluewire.Common.Console.Environment;
using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console.Daemons
{
    public class DaemonSession
    {
        private readonly IRunAsConsoleApplication runAsConsoleApplication;
        private readonly IRunAsService runAsService;
        private readonly IRunAsServiceInstaller runAsServiceInstaller;
        private readonly IRunAsHostedService runAsHostedService;
        private readonly ITestAsConsoleApplication testAsConsoleApplication;

        public DaemonSession(
            IRunAsConsoleApplication runAsConsoleApplication,
            IRunAsService runAsService,
            IRunAsServiceInstaller runAsServiceInstaller,
            IRunAsHostedService runAsHostedService,
            ITestAsConsoleApplication testAsConsoleApplication)
        {
            this.runAsConsoleApplication = runAsConsoleApplication;
            this.runAsService = runAsService;
            this.runAsServiceInstaller = runAsServiceInstaller;
            this.runAsHostedService = runAsHostedService;
            this.testAsConsoleApplication = testAsConsoleApplication;
        }

        public int Run(IExecutionEnvironment environment, IDaemonisable daemon, params string[] args)
        {
            if (environment is ServiceEnvironment serviceEnvironment)
            {
                return runAsService.Run(serviceEnvironment, daemon, args);
            }

            if (environment is ApplicationEnvironment applicationEnvironment)
            {
                return RunInApplicationEnvironment(applicationEnvironment, daemon, args);
            }

            if (environment is InitialisedHostedEnvironment hostedEnvironment)
            {
                var session = new SessionArguments();
                session.Options.AddCollector(daemon as IReceiveOptions);
                session.ArgumentList.AddCollector(daemon as IReceiveArgumentList);
                session.Parse(args);
                return runAsHostedService.Run(hostedEnvironment, daemon);
            }

            // Exit code 10 is a Windows standard exit code meaning 'The environment is incorrect'.
            // Try 'net helpmsg 10' at the shell.
            return 10;
        }


        private int RunInApplicationEnvironment(ApplicationEnvironment applicationEnvironment, IDaemonisable daemon, string[] args)
        {
            var session = new ConsoleSession();
            var serviceInstallerArguments = session.Options.AddCollector(new ServiceInstallerArguments(daemon));
            session.Options.AddCollector(daemon as IReceiveOptions);
            session.ArgumentList.AddCollector(daemon as IReceiveArgumentList);

            return session.Run(args, () =>
            {
                if (serviceInstallerArguments.ServiceInstallationRequested)
                {
                    // reparse, stripping out only the installer-related arguments.
                    var dummyArgs = new OptionSet();
                    dummyArgs.AddCollector(new ServiceInstallerArguments(daemon));
                    var parseResult = new OptionsParser().Parse(dummyArgs, args);
                    var consoleArguments = parseResult.RemainingArguments.ToArray();

                    return runAsServiceInstaller.Run(applicationEnvironment, serviceInstallerArguments, consoleArguments);
                }
                if (serviceInstallerArguments.RunTest)
                {
                    return testAsConsoleApplication.Test(applicationEnvironment, daemon);
                }
                return runAsConsoleApplication.Run(applicationEnvironment, daemon);
            });
        }
    }
}
