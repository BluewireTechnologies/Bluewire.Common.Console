using System;
using System.Reflection;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Environment;
using Bluewire.Common.Console.ThirdParty;
using NUnit.Framework;
using Moq;

namespace Bluewire.Common.Console.Tests
{
    [TestFixture]
    public class DaemonRunnerTests
    {
        private Mock<IRunAsConsoleApplication> runAsConsoleApplication;
        private Mock<IRunAsService> runAsService;
        private Mock<IRunAsServiceInstaller> runAsServiceInstaller;
        private Mock<IRunAsHostedService> runAsHostedService;
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
            runAsHostedService = new Mock<IRunAsHostedService>();
            
            runner = new DaemonRunner<NoArguments>(runAsConsoleApplication.Object, runAsService.Object, runAsServiceInstaller.Object, runAsHostedService.Object);

            var daemon = new Mock<IDaemonisable<NoArguments>>();
            daemon.Setup(d => d.Configure()).Returns(() => new SessionArguments<NoArguments>(new NoArguments(), new OptionSet()));
            daemon.SetupGet(d => d.Name).Returns(DAEMON_NAME);
            this.daemon = daemon.Object;

            // I can't figure out if Moq provides direct access to recorded invocations. The docs are unhelpful.
            serviceInstallerArguments = null;
            passthroughArguments = null;
            runAsServiceInstaller.Setup(s => s.Run(It.IsAny<ApplicationEnvironment>(), It.IsAny<ServiceInstallerArguments<NoArguments>>(), It.IsAny<string[]>())).Callback((ApplicationEnvironment env, ServiceInstallerArguments<NoArguments> sia, string[] a) =>
            {
                serviceInstallerArguments = sia;
                passthroughArguments = a;
            });
        }

        private void VerifyServiceArguments(Action<ServiceInstallerArguments<NoArguments>, string[]> asserts)
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
            runner.Run(new InitialisedHostedEnvironment(new HostedEnvironmentDefinition(), new HostedEnvironment()), daemon, "arg");

            runAsHostedService.Verify(s => s.Run(It.IsAny<InitialisedHostedEnvironment>(), daemon, It.IsAny<NoArguments>()));
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

            runAsConsoleApplication.Verify(s => s.Run(It.IsAny<ApplicationEnvironment>(), daemon, It.IsAny<NoArguments>()));
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

            runAsConsoleApplication.Verify(s => s.Run(It.IsAny<ApplicationEnvironment>(), daemon, It.IsAny<NoArguments>()));
        }
    }
}
