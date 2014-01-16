using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Bluewire.Common.Console.Hosting
{
    public class HostedDaemonExe : IHostingBehaviour
    {
        private readonly AppDomainSetup appDomainSetup;
        private string configurationFilePath;
        private XmlDocument configurationXml;

        public HostedDaemonExe(AssemblyName daemonAssemblyName, AppDomainSetup setup = null)
        {
            if (daemonAssemblyName == null) throw new ArgumentNullException("daemonAssemblyName");
            AssemblyName = daemonAssemblyName;
            appDomainSetup = setup ?? AppDomain.CurrentDomain.SetupInformation;
        }

        public AssemblyName AssemblyName { get; private set; }
        
        public HostedDaemonExe UseConfiguration(XmlDocument configuration)
        {
            configurationFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            
            configurationXml = (XmlDocument)configuration.Clone();
            appDomainSetup.ConfigurationFile = configurationFilePath;
            return this;
        }

        AppDomainSetup IHostingBehaviour.CreateAppDomainSetup()
        {
            return appDomainSetup;
        }

        void IHostingBehaviour.OnBeforeStart()
        {
            if (configurationFilePath != null)
            {
                configurationXml.Save(configurationFilePath);
            }
        }

        void IHostingBehaviour.OnAfterStop()
        {
            if (configurationFilePath != null && File.Exists(configurationFilePath))
            {
                try
                {
                    File.Delete(configurationFilePath);
                }
                catch { }
            }
        }
    }
}