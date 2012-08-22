using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Logging;
using Microsoft.Win32;

namespace Bluewire.Common.Console
{
    public static class DaemonRunner
    {
        public static int Run<T>(string[] args, IDaemonisable<T> daemon)
        {
            return new DaemonRunner<T>(
                new DefaultExecutionEnvironment(),
                new RunAsConsoleApplication(),
                new RunAsService(),
                new RunAsServiceInstaller()).Run(daemon, args);
        }



        public class RunAsService : IRunAsService
        {
            public void Run<T>(IDaemonisable<T> daemon, string[] staticArgs)
            {
                var servicesToRun = new ServiceBase[] { new DaemonService<T>(daemon, staticArgs) };
                ServiceBase.Run(servicesToRun);
            }
        }

        public class RunAsConsoleApplication : IRunAsConsoleApplication
        {
            public int Run<T>(IDaemonisable<T> daemon, T arguments)
            {
                return new ConsoleDaemonMonitor(daemon.Start(arguments)).WaitForTermination();
            }
        }

        public class DefaultExecutionEnvironment : IExecutionEnvironment
        {
            public bool IsRunningAsService()
            {
                return NativeMethods.IsRunningAsService();
            }
        }

        public class RunAsServiceInstaller : IRunAsServiceInstaller
        {
            public int Run<T>(IDaemonisable<T> daemon, ServiceInstallerArguments arguments, string[] serviceArguments)
            {
                var serviceInstaller = CreateServiceInstaller(daemon, arguments);
                var serviceName = serviceInstaller.ServiceName;
                var installer = CreateInstaller(serviceInstaller, arguments);

                if (arguments.RunUninstall)
                {
                    Log.Console.InfoFormat("Uninstalling service {0}", serviceName);
                    installer.Uninstall(null);
                }
                if (arguments.RunInstall)
                {
                    Log.Console.InfoFormat("Installing service {0}\n\tstart type:{1}\n\targuments:{2}", serviceInstaller.ServiceName, serviceInstaller.StartType, String.Join(", ", serviceArguments));
                    installer.Install(new Hashtable());

                    SetServiceArguments(serviceName, serviceArguments);
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

            private static ServiceInstaller CreateServiceInstaller<T>(IDaemonisable<T> daemon, ServiceInstallerArguments arguments)
            {
                var serviceInstaller = new ServiceInstaller();
                serviceInstaller.ServiceName = String.IsNullOrEmpty(arguments.ServiceName) ? daemon.Name : arguments.ServiceName;
                serviceInstaller.StartType = ServiceStartMode.Automatic;
                return serviceInstaller;
            }

            private void SetServiceArguments(string serviceName, string[] serviceArguments)
            {
                var argumentString = String.Join(" ", serviceArguments.Select(FormatArgument).ToArray());
                Log.Console.InfoFormat("Setting service arguments for {0}: {1}", serviceName, argumentString);

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