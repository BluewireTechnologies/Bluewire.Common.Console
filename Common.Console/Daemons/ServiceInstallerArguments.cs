﻿namespace Bluewire.Common.Console.Daemons
{
    public class ServiceInstallerArguments
    {
        public bool ServiceInstallationRequested { get; private set; }

        private void AssertOnlyOneRequest()
        {
            if (ServiceInstallationRequested) throw new InvalidArgumentsException("Specify only one of --install, --reinstall or --uninstall.");
            ServiceInstallationRequested = true;
        }

        public bool RunInstall { get; private set; }
        public bool RunUninstall { get; private set; }

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