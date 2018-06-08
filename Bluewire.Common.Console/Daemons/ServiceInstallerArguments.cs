using System;
using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console.Daemons
{
    public class ServiceInstallerArguments : IReceiveOptions
    {
        public IDaemonisable Daemon { get; }

        public ServiceInstallerArguments(IDaemonisable daemon)
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

        void IReceiveOptions.ReceiveFrom(OptionSet options)
        {
            options.Add("test", "Start and stop the application to verify its configuration. This will run it under the current user account.", i => Test());
            options.Add("install", "Install this application as a service.", i => Install());
            options.Add("uninstall", "Uninstall this service.", i => Uninstall());
            options.Add("reinstall", "Reinstall this service.", i => Reinstall());
            options.Add("service-name=", $"Use the specified name for the service. Default: {ServiceName}", i => ServiceName = i);
            options.Add("service-user=", "Run the service under the specified account. Default: LocalService", i => ServiceUser = i);
            options.Add("service-password=", i => ServicePassword = i);
        }
    }
}
