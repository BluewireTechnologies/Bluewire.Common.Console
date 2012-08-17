using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using Bluewire.Common.Console.Daemons;
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
                var installer = CreateServiceInstaller(daemon, arguments);

                if (arguments.RunUninstall)
                {
                    installer.Uninstall(null);
                }
                if (arguments.RunInstall)
                {
                    installer.Install(new Hashtable());

                    SetServiceArguments(daemon, serviceArguments);
                }

                return 0;
            }

            private TransactedInstaller CreateServiceInstaller<T>(IDaemonisable<T> daemon, ServiceInstallerArguments arguments)
            {
                var serviceProcessInstaller = new ServiceProcessInstaller();
                var serviceInstaller = new ServiceInstaller();
                serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
                serviceProcessInstaller.Password = null;
                serviceProcessInstaller.Username = null;
                serviceInstaller.ServiceName = String.IsNullOrEmpty(arguments.ServiceName) ? daemon.Name : arguments.ServiceName;
                serviceInstaller.StartType = ServiceStartMode.Automatic;
                
                var context = new InstallContext();
                context.Parameters["assemblyPath"] = Assembly.GetEntryAssembly().Location;
                return new TransactedInstaller()
                {
                    Context = context,
                    Installers =
                        {
                            serviceProcessInstaller,
                            serviceInstaller
                        }
                };
            }

            private void SetServiceArguments<T>(IDaemonisable<T> daemon, string[] serviceArguments)
            {
                using (var configKey = Registry.LocalMachine.OpenSubKey(String.Format(@"SYSTEM\CurrentControlSet\services\{0}", daemon.Name), true)) 
                {
                    var existingImagePath = configKey.GetValue("ImagePath");
                    var argumentString = String.Join(" ", serviceArguments.Select(FormatArgument).ToArray());
                    configKey.SetValue("ImagePath", existingImagePath + " " + argumentString);
                }
            }

            private string FormatArgument(string arg)
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