using System;
using System.ServiceProcess;

namespace Bluewire.Common.Console.Daemons
{
    public class ServiceInstallerArguments<T>
    {
        public IDaemonisable<T> Daemon { get; private set; }

        public ServiceInstallerArguments(IDaemonisable<T> daemon)
        {
            this.Daemon = daemon;
        }

        public bool ServiceInstallationRequested { get { return RunInstall || RunUninstall; } }

        public bool ModeSwitchSpecified { get; private set; }

        private void AssertOnlyOneRequest()
        {
            if (ModeSwitchSpecified) throw new InvalidArgumentsException("Specify only one of --install, --reinstall, --uninstall or --test.");
            ModeSwitchSpecified = true;
        }

        public bool RunTest { get; private set; }
        public bool RunInstall { get; private set; }
        public bool RunUninstall { get; private set; }

        private string serviceName;
        public string ServiceName
        {
            get
            {
                return String.IsNullOrEmpty(this.serviceName) ? this.Daemon.Name : this.serviceName;
            }
            set
            {
                this.serviceName = value;
            }
        }

        public string ServiceUser { get; set; }
        public string ServicePassword { get; set; }

        public ServiceAccountCredentials GetAccount()
        {
            return new ServiceAccountCredentialsFactory().Create(ServiceUser, ServicePassword) ?? new ServiceAccountCredentials();
        }

        public void Test()
        {
            AssertOnlyOneRequest();
            RunTest = true;
        }

        public void Install()
        {
            AssertOnlyOneRequest();
            RunInstall = true;
        }

        public void Reinstall()
        {
            AssertOnlyOneRequest();
            RunInstall = true;
            RunUninstall = true;
        }

        public void Uninstall()
        {
            AssertOnlyOneRequest();
            RunUninstall = true;
        }
    }
}
