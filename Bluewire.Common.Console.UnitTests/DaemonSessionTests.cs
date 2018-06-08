using System;
using System.Reflection;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Environment;
using Moq;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests
{
    [TestFixture]
    public class DaemonSessionTests
    {
        private Mock<IRunAsConsoleApplication> runAsConsoleApplication;
        private Mock<ITestAsConsoleApplication> testAsConsoleApplication;
        private Mock<IRunAsService> runAsService;
        private Mock<IRunAsServiceInstaller> runAsServiceInstaller;
        private Mock<IRunAsHostedService> runAsHostedService;
        private DaemonSession runner;
        private IDaemonisable daemon;

        private ServiceInstallerArguments serviceInstallerArguments;
        private string[] passthroughArguments;

        private const string DAEMON_NAME = "Daemon";

        [SetUp]
        public void SetUp()
        {
            runAsConsoleApplication = new Mock<IRunAsConsoleApplication>();
            testAsConsoleApplication = new Mock<ITestAsConsoleApplication>();
            runAsService = new Mock<IRunAsService>();
            runAsServiceInstaller = new Mock<IRunAsServiceInstaller>();
            runAsHostedService = new Mock<IRunAsHostedService>();

            runner = new DaemonSession(runAsConsoleApplication.Object, runAsService.Object, runAsServiceInstaller.Object, runAsHostedService.Object, testAsConsoleApplication.Object);

            daemon = Mock.Of<IDaemonisable>(d => d.Name == DAEMON_NAME);

            // I can't figure out if Moq provides direct access to recorded invocations. The docs are unhelpful.
            serviceInstallerArguments = null;
            passthroughArguments = null;
            runAsServiceInstaller.Setup(s => s.Run(It.IsAny<ApplicationEnvironment>(), It.IsAny<ServiceInstallerArguments>(), It.IsAny<string[]>())).Callback((ApplicationEnvironment env, ServiceInstallerArguments sia, string[] a) =>
            {
                serviceInstallerArguments = sia;
                passthroughArguments = a;
            });
        }

        private void VerifyServiceArguments(Action<ServiceInstallerArguments, string[]> asserts)
        {
            Assert.IsNotNull(serviceInstallerArguments, "IRunAsServiceInstaller#Run(IDaemon, ServiceInstallerArguments, string[]) was not called.");
            Assert.IsNotNull(passthroughArguments, "IRunAsServiceInstaller#Run(IDaemon, ServiceInstallerArguments, string[]) was not called.");
            asserts(serviceInstallerArguments, passthroughArguments);
        }

        [Test]
        public void IfInvokedAsAService_RunsDaemonAsAService()
        {
            runner.Run(new ServiceEnvironment(), daemon, "arg");

            runAsService.Verify(s => s.Run(It.IsAny<ServiceEnvironment>(), daemon, new [] { "arg" }));
        }

        [Test]
        public void IfInvokedInAnInitialisedHostedEnvironment_RunsDaemonAsAHostedService()
        {
            runner.Run(new InitialisedHostedEnvironment(new HostedEnvironmentDefinition(Assembly.GetExecutingAssembly().GetName()), new HostedEnvironment()), daemon, "arg");

            runAsHostedService.Verify(s => s.Run(It.IsAny<InitialisedHostedEnvironment>(), daemon));
        }

        [Test]
        public void IfInvokedInAnUninitialisedHostedEnvironment_ReturnsErrorCode()
        {
            Assert.AreNotEqual(0, runner.Run(new HostedEnvironment(), daemon, "arg"));
        }


        [Test]
        public void IfInvokedFromTheConsole_RunsDaemonAsAConsoleApplication()
        {
            runner.Run(new ApplicationEnvironment(Assembly.GetExecutingAssembly()), daemon, "arg");

            runAsConsoleApplication.Verify(s => s.Run(It.IsAny<ApplicationEnvironment>(), daemon));
        }

        [Test]
        public void IfInvokedWithInstallSwitch_InstallsDaemon()
        {
            runner.Run(new ApplicationEnvironment(Assembly.GetExecutingAssembly()), daemon, "--install", "arg");

            VerifyServiceArguments((sa, ca) =>
            {
                Assert.IsTrue(sa.RunInstall);
                Assert.IsFalse(sa.RunUninstall);
                CollectionAssert.AreEqual(new[] { "arg" }, ca);
            });
        }


        [Test]
        public void ServiceNameDefaultsToDaemonName()
        {
            runner.Run(new ApplicationEnvironment(Assembly.GetExecutingAssembly()), daemon, "--install", "arg");

            VerifyServiceArguments((sa, ca) => Assert.AreEqual(DAEMON_NAME, sa.ServiceName));
        }

        [Test]
        public void CanOverrideServiceName()
        {
            runner.Run(new ApplicationEnvironment(Assembly.GetExecutingAssembly()), daemon, "--install", "--service-name", "Test Name", "arg");

            VerifyServiceArguments((sa, ca) => Assert.AreEqual("Test Name", sa.ServiceName));
        }

        [Test]
        public void CanSpecifyServiceAccount()
        {
            runner.Run(new ApplicationEnvironment(Assembly.GetExecutingAssembly()), daemon, "--install", "--service-user", "User", "--service-password", "Password", "arg");

            VerifyServiceArguments((sa, ca) =>
            {
                Assert.AreEqual("User", sa.ServiceUser);
                Assert.AreEqual("Password", sa.ServicePassword);
            });
        }

        [Test]
        public void IfInvokedWithUninstallSwitch_UninstallsDaemon()
        {
            runner.Run(new ApplicationEnvironment(Assembly.GetExecutingAssembly()), daemon, "--uninstall", "arg");

            VerifyServiceArguments((sa, ca) =>
            {
                Assert.IsFalse(sa.RunInstall);
                Assert.IsTrue(sa.RunUninstall);
                CollectionAssert.AreEqual(new[] { "arg" }, ca);
            });
        }

        [Test]
        public void IfInvokedWithReinstallSwitch_UninstallsAndInstallsDaemon()
        {
            runner.Run(new ApplicationEnvironment(Assembly.GetExecutingAssembly()), daemon, "--reinstall", "arg");
            VerifyServiceArguments((sa, ca) =>
            {
                Assert.IsTrue(sa.RunInstall);
                Assert.IsTrue(sa.RunUninstall);
                CollectionAssert.AreEqual(new[] { "arg" }, ca);
            });
        }


        [Test]
        public void IfInstallSwitchIsAfterDoubleDash_RunsDaemonAsAConsoleApplication()
        {
            runner.Run(new ApplicationEnvironment(Assembly.GetExecutingAssembly()), daemon, "--", "--install", "arg");

            runAsConsoleApplication.Verify(s => s.Run(It.IsAny<ApplicationEnvironment>(), daemon));
        }

        [Test]
        public void IfInvokedWithTestSwitch_RunsDaemonInTestMode()
        {
            runner.Run(new ApplicationEnvironment(Assembly.GetExecutingAssembly()), daemon, "--test");

            testAsConsoleApplication.Verify(s => s.Test(It.IsAny<ApplicationEnvironment>(), daemon));
        }
    }
}
