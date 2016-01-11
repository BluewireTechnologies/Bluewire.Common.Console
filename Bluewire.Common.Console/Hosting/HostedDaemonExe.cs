using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Bluewire.Common.Console.Hosting
{
    public class HostedDaemonExe : IHostingBehaviour
    {
        private string configurationFilePath;
        private XmlDocument configurationXml;


        public HostedDaemonExe(AssemblyName daemonAssemblyName, AppDomainSetup appDomainSetup = null)
        {
            if (daemonAssemblyName == null) throw new ArgumentNullException("daemonAssemblyName");
            AssemblyName = daemonAssemblyName;
            AppDomainSetup = appDomainSetup ?? AppDomain.CurrentDomain.SetupInformation;
        }

        public AssemblyName AssemblyName { get; private set; }
        public AppDomainSetup AppDomainSetup { get; private set; }

        public HostedDaemonExe UseConfiguration(XmlDocument configuration)
        {
            configurationFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            
            configurationXml = (XmlDocument)configuration.Clone();
            AppDomainSetup.ConfigurationFile = configurationFilePath;
            return this;
        }
        
        public static HostedDaemonExe FromAssemblyFile(string file)
        {
            if(!Path.IsPathRooted(file)) throw new ArgumentException("Specified path is not absolute.");
            var assemblyName = AssemblyName.GetAssemblyName(file);
            
            return new HostedDaemonExe(assemblyName, new AppDomainSetup())
            {
                AppDomainSetup = {
                    ApplicationName = assemblyName.Name,
                    ApplicationBase = Path.GetDirectoryName(file),
                    ApplicationTrust = AppDomain.CurrentDomain.ApplicationTrust
                }
            };
        }

        AppDomainSetup IHostingBehaviour.CreateAppDomainSetup()
        {
            return AppDomainSetup;
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