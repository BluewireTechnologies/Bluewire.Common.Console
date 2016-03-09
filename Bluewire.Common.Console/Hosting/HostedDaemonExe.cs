using System;
using System.Configuration;
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

        public HostedDaemonExe UseConfiguration(XmlDocument configuration, bool? keepExistingBindingRedirects = null)
        {
            configurationFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            
            configurationXml = (XmlDocument)configuration.Clone();
            
            if(keepExistingBindingRedirects != false)
            {
                BindingRedirects bindingRedirects;
                if(TryReadBindingRedirects(keepExistingBindingRedirects == true, out bindingRedirects))
                {
                    bindingRedirects.ApplyTo(configurationXml);
                }
            }

            AppDomainSetup.ConfigurationFile = configurationFilePath;
            return this;
        }

        private bool TryReadBindingRedirects(bool throwIfNotAFilesystemAssembly, out BindingRedirects bindingRedirects)
        {
            bindingRedirects = null;
            var assemblyLocation = new Uri(AssemblyName.CodeBase);

            if(!assemblyLocation.IsFile || !File.Exists(assemblyLocation.LocalPath))
            {
                // If we're trying to inherit binding redirects but the assembly isn't where we expect, it's
                // possibly GAC'd. If the caller's expecting this, it shouldn't ask us to look for the
                // configuration file in the first place.
                if(throwIfNotAFilesystemAssembly) throw new FileNotFoundException($"Unable to read existing binding redirects because the assembly file '{assemblyLocation}' could not be found.");
                return false;
            }
            var likelyConfigurationLocation = $"{assemblyLocation.LocalPath}.config";
            if(!File.Exists(likelyConfigurationLocation))
            {
                // If we're looking for the redirects of an assembly on the filesystem, it is still permissible
                // for the entire configuration file to be simply absent.
                return false;
            }
            bindingRedirects = BindingRedirects.ReadFrom(likelyConfigurationLocation);
            return true;
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