using System.ServiceProcess;
using Bluewire.Common.Console.Daemons;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests
{
    [TestFixture]
    public class ServiceAccountCredentialsFactoryTests
    {
        private readonly ServiceAccountCredentialsFactory factory = new ServiceAccountCredentialsFactory();

        [TestCase("LOCAL SYSTEM")]
        [TestCase("SYSTEM")]
        public void LocalSystem(string localSystem)
        {
            var account = factory.Create(localSystem, null).Value;
            Assert.AreEqual(ServiceAccount.LocalSystem, account.ServiceAccount);
        }

        [Test]
        public void UserCalledSystem()
        {
            var account = factory.Create("System", "Password").Value;
            Assert.AreEqual(ServiceAccount.User, account.ServiceAccount);
            Assert.AreEqual("System", account.Credentials.UserName);
            Assert.AreEqual("Password", account.Credentials.Password);
        }

        [Test]
        public void UserWithDomain()
        {
            var account = factory.Create(@"DOMAIN\User", "Password").Value;
            Assert.AreEqual(ServiceAccount.User, account.ServiceAccount);
            Assert.AreEqual("User", account.Credentials.UserName);
            Assert.AreEqual("DOMAIN", account.Credentials.Domain);
            Assert.AreEqual("Password", account.Credentials.Password);
        }

        [Test]
        public void NetworkService()
        {
            var account = factory.Create(@"NETWORK SERVICE", null).Value;
            Assert.AreEqual(ServiceAccount.NetworkService, account.ServiceAccount);
        }

        [Test]
        public void LocalService()
        {
            var account = factory.Create(@"LOCAL SERVICE", null).Value;
            Assert.AreEqual(ServiceAccount.LocalService, account.ServiceAccount);
        }

        [Test]
        public void NoUserName_ReturnsNull()
        {
            Assert.IsNull(factory.Create(null, null));
        }
    }
}
