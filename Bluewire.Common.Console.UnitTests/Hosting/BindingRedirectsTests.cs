using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Bluewire.Common.Console.Hosting;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Hosting
{
    [TestFixture]
    public class BindingRedirectsTests
    {
        [Test]
        public void CollectsBindingRedirectsFromConfigurationXml()
        {
            var xml = new XmlDocument();
            xml.LoadXml(@"
<configuration>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
      <dependentAssembly>
        <assemblyIdentity name='SomeReference' publicKeyToken='79cab3b4ccd030ec' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-1.2.3.4' newVersion='1.2.3.4' />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
");

            var redirects = BindingRedirects.ReadFrom(xml);

            Assert.That(redirects, Is.Not.Null);
            Assert.That(redirects.Count, Is.EqualTo(1));
        }
    }
}
