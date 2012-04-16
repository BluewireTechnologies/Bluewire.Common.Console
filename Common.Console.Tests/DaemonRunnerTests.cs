using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.ThirdParty;
using MbUnit.Framework;
using Moq;

namespace Bluewire.Common.Console.Tests
{
    [TestFixture]
    public class DaemonRunnerTests
    {
        private Mock<IRunAsConsoleApplication> runAsConsoleApplication;
        private Mock<IRunAsService> runAsService;
        private Mock<IRunAsServiceInstaller> runAsServiceInstaller;
        private Mock<IExecutionEnvironment> executionEnvironment;
        private DaemonRunner<NoArguments> runner;
        private IDaemonisable<NoArguments> daemon;

        [SetUp]
        public void SetUp()
        {
            runAsConsoleApplication = new Mock<IRunAsConsoleApplication>();
            runAsService = new Mock<IRunAsService>();
            runAsServiceInstaller = new Mock<IRunAsServiceInstaller>();
            executionEnvironment = new Mock<IExecutionEnvironment>();
            runner = new DaemonRunner<NoArguments>(executionEnvironment.Object, runAsConsoleApplication.Object, runAsService.Object, runAsServiceInstaller.Object);

            var daemon = new Mock<IDaemonisable<NoArguments>>();
            daemon.Setup(d => d.Configure()).Returns(() => new SessionArguments<NoArguments>(new NoArguments(), new OptionSet()));
            this.daemon = daemon.Object;
        }

        [Test]
        public void IfInvokedAsAService_RunsDaemonAsAService()
        {
            executionEnvironment.Setup(e => e.IsRunningAsService()).Returns(true);
            runner.Run(daemon, "arg");

            runAsService.Verify(s => s.Run(daemon, new [] { "arg" }));
        }


        [Test]
        public void IfInvokedFromTheConsole_RunsDaemonAsAConsoleApplication()
        {
            runner.Run(daemon, "arg");

            runAsConsoleApplication.Verify(s => s.Run(daemon, It.IsAny<NoArguments>()));
        }

        [Test]
        public void IfInvokedWithInstallSwitch_InstallsDaemon()
        {
            runner.Run(daemon, "--install", "arg");

            runAsServiceInstaller.Verify(s => s.Run(daemon, It.Is<ServiceInstallerArguments>(a => a.RunInstall && !a.RunUninstall), new [] { "arg" }));
        }

        [Test]
        public void IfInvokedWithUninstallSwitch_UninstallsDaemon()
        {
            runner.Run(daemon, "--uninstall", "arg");

            runAsServiceInstaller.Verify(s => s.Run(daemon, It.Is<ServiceInstallerArguments>(a => a.RunUninstall && !a.RunInstall), new[] { "arg" }));
        }

        [Test]
        public void IfInvokedWithReinstallSwitch_UninstallsAndInstallsDaemon()
        {
            runner.Run(daemon, "--reinstall", "arg");

            runAsServiceInstaller.Verify(s => s.Run(daemon, It.Is<ServiceInstallerArguments>(a => a.RunUninstall && a.RunInstall), new[] { "arg" }));
        }


        [Test]
        public void IfInstallSwitchIsAfterDoubleDash_RunsDaemonAsAConsoleApplication()
        {
            runner.Run(daemon, "--", "--install", "arg");

            runAsConsoleApplication.Verify(s => s.Run(daemon, It.IsAny<NoArguments>()));
        }
    }
}
