using System;
using System.Collections;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Environment;
using Microsoft.Win32;

namespace Bluewire.Common.Console
{
    public static class DaemonRunner
    {
        public static int Run(string[] args, IDaemonisable daemon)
        {
            return Run(args, daemon, new EnvironmentAnalyser().GetEnvironment());
        }

        /// <summary>
        /// Intended for testing. This override permits the execution environment to be specified explicitly.
        /// </summary>
        /// <remarks>
        /// Refrain from using this. In real applications it will cause incorrect behaviour whenever the daemon
        /// is not actually running in the specified execution environment.
        /// </remarks>
        /// <param name="args"></param>
        /// <param name="daemon"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static int Run(string[] args, IDaemonisable daemon, IExecutionEnvironment environment)
        {
            return new DaemonSession(
                new RunAsConsoleApplication(),
                new RunAsService(),
                new RunAsServiceInstaller(),
                new RunAsHostedService(),
                new TestAsConsoleApplication()).Run(environment, daemon, args);
        }



        public class RunAsHostedService : IRunAsHostedService
        {
            public int Run(InitialisedHostedEnvironment environment, IDaemonisable daemon)
            {
                var instance = new HostedDaemonMonitor(daemon);
                instance.Start();
                environment.RegisterForShutdownNotification(instance);
                instance.WaitForTermination();
                return 0;
            }
        }

        public class RunAsService : IRunAsService
        {
            public int Run(ServiceEnvironment environment, IDaemonisable daemon, string[] staticArgs)
            {
                var servicesToRun = new ServiceBase[] { new DaemonService(daemon, staticArgs) };
                ServiceBase.Run(servicesToRun);
                return 0;
            }
        }

        public class RunAsConsoleApplication : IRunAsConsoleApplication
        {
            public int Run(ApplicationEnvironment environment, IDaemonisable daemon)
            {
                var monitor = new ConsoleDaemonMonitor(daemon);
                monitor.Start();
                monitor.WaitForTermination();
                return 0;
            }
        }

        public class TestAsConsoleApplication : ITestAsConsoleApplication
        {
            public int Test(ApplicationEnvironment environment, IDaemonisable daemon)
            {
                var instance = new HostedDaemonMonitor(daemon);
                instance.Start();
                instance.RequestShutdown();
                instance.Wait();
                return 0;
            }
        }

        public class RunAsServiceInstaller : IRunAsServiceInstaller
        {
            public int Run(ApplicationEnvironment environment, ServiceInstallerArguments arguments, string[] serviceArguments)
            {
                var serviceInstaller = CreateServiceInstaller(arguments);
                var installer = CreateInstaller(serviceInstaller, arguments);

                if (arguments.RunUninstall)
                {
                    System.Console.Out.WriteLine("Uninstalling service {0}", serviceInstaller.ServiceName);
                    installer.Uninstall(null);
                }
                if (arguments.RunInstall)
                {
                    System.Console.Out.WriteLine("Installing service {0}", serviceInstaller.ServiceName);
                    System.Console.Out.WriteLine("\tStart type: {0}", serviceInstaller.StartType);
                    System.Console.Out.WriteLine("\tArguments:  {0}", String.Join(" ", serviceArguments));
                    installer.Install(new Hashtable());

                    SetServiceArguments(serviceInstaller.ServiceName, serviceArguments);
                }

                return 0;
            }

            private static TransactedInstaller CreateInstaller(ServiceInstaller serviceInstaller, ServiceInstallerArguments arguments)
            {
                var serviceProcessInstaller = new ServiceProcessInstaller();
                arguments.GetAccount().Apply(serviceProcessInstaller);

                var context = new InstallContext();
                context.Parameters["assemblyPath"] = Assembly.GetEntryAssembly().Location;
                return new TransactedInstaller
                {
                    Context = context,
                    Installers =
                        {
                            serviceProcessInstaller,
                            serviceInstaller
                        }
                };
            }

            private static ServiceInstaller CreateServiceInstaller(ServiceInstallerArguments arguments)
            {
                var serviceInstaller = new ServiceInstaller();
                serviceInstaller.ServiceName = arguments.ServiceName;
                serviceInstaller.StartType = ServiceStartMode.Automatic;
                serviceInstaller.ServicesDependedOn = arguments.Daemon.GetDependencies();

                return serviceInstaller;
            }

            private void SetServiceArguments(string serviceName, string[] serviceArguments)
            {
                var argumentString = String.Join(" ", serviceArguments.Select(FormatArgument).ToArray());
                System.Console.Out.WriteLine("Setting service arguments for {0}: {1}", serviceName, argumentString);

                using (var configKey = Registry.LocalMachine.OpenSubKey(String.Format(@"SYSTEM\CurrentControlSet\services\{0}", serviceName), true))
                {
                    var existingImagePath = configKey.GetValue("ImagePath");
                    configKey.SetValue("ImagePath", existingImagePath + " " + argumentString);
                }
            }

            private static string FormatArgument(string arg)
            {
                if (arg.Any(Char.IsWhiteSpace))
                {
                    return '"' + arg + '"';
                }
                return arg;
            }
        }
    }
}
