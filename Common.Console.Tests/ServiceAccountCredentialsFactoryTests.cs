using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Bluewire.Common.Console.Daemons;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;

namespace Bluewire.Common.Console.Tests
{
    [TestFixture]
    public class ServiceAccountCredentialsFactoryTests
    {
        private readonly ServiceAccountCredentialsFactory factory = new ServiceAccountCredentialsFactory();

        [Test]
        [Row("LOCAL SYSTEM")]
        [Row("SYSTEM")]
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
