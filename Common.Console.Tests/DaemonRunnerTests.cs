using System;
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

        private ServiceInstallerArguments<NoArguments> serviceInstallerArguments;
        private string[] passthroughArguments;

        private const string DAEMON_NAME = "Daemon";

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
            daemon.SetupGet(d => d.Name).Returns(DAEMON_NAME);
            this.daemon = daemon.Object;

            // I can't figure out if Moq provides direct access to recorded invocations. The docs are unhelpful.
            serviceInstallerArguments = null;
            passthroughArguments = null;
            runAsServiceInstaller.Setup(s => s.Run(It.IsAny<ServiceInstallerArguments<NoArguments>>(), It.IsAny<string[]>())).Callback((ServiceInstallerArguments<NoArguments> sia, string[] a) =>
            {
                serviceInstallerArguments = sia;
                passthroughArguments = a;
            });
        }

        private void VerifyServiceArguments(Action<ServiceInstallerArguments<NoArguments>, string[]> asserts)
        {
            Assert.IsNotNull(serviceInstallerArguments, "IRunAsServiceInstaller#Run(IDaemon, ServiceInstallerArguments, string[]) was not called.");
            Assert.IsNotNull(passthroughArguments, "IRunAsServiceInstaller#Run(IDaemon, ServiceInstallerArguments, string[]) was not called.");
            Assert.Multiple(() => asserts(serviceInstallerArguments, passthroughArguments));
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

            VerifyServiceArguments((sa, ca) =>
            {
                Assert.IsTrue(sa.RunInstall);
                Assert.IsFalse(sa.RunUninstall);
                Assert.AreElementsEqual(new[] { "arg" }, ca);
            });
        }


        [Test]
        public void ServiceNameDefaultsToDaemonName()
        {
            runner.Run(daemon, "--install", "arg");

            VerifyServiceArguments((sa, ca) => Assert.AreEqual(DAEMON_NAME, sa.ServiceName));
        }

        [Test]
        public void CanOverrideServiceName()
        {
            runner.Run(daemon, "--install", "--service-name", "Test Name", "arg");

            VerifyServiceArguments((sa, ca) => Assert.AreEqual("Test Name", sa.ServiceName));
        }
        
        [Test]
        public void CanSpecifyServiceAccount()
        {
            runner.Run(daemon, "--install", "--service-user", "User", "--service-password", "Password", "arg");

            VerifyServiceArguments((sa, ca) =>
            {
                Assert.AreEqual("User", sa.ServiceUser);
                Assert.AreEqual("Password", sa.ServicePassword);
            });
        }

        [Test]
        public void IfInvokedWithUninstallSwitch_UninstallsDaemon()
        {
            runner.Run(daemon, "--uninstall", "arg");

            VerifyServiceArguments((sa, ca) =>
            {
                Assert.IsFalse(sa.RunInstall);
                Assert.IsTrue(sa.RunUninstall);
                Assert.AreElementsEqual(new[] { "arg" }, ca);
            });
        }

        [Test]
        public void IfInvokedWithReinstallSwitch_UninstallsAndInstallsDaemon()
        {
            runner.Run(daemon, "--reinstall", "arg");
            VerifyServiceArguments((sa, ca) =>
            {
                Assert.IsTrue(sa.RunInstall);
                Assert.IsTrue(sa.RunUninstall);
                Assert.AreElementsEqual(new[] { "arg" }, ca);
            });
        }


        [Test]
        public void IfInstallSwitchIsAfterDoubleDash_RunsDaemonAsAConsoleApplication()
        {
            runner.Run(daemon, "--", "--install", "arg");

            runAsConsoleApplication.Verify(s => s.Run(daemon, It.IsAny<NoArguments>()));
        }
    }
}
