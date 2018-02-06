using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Bluewire.Common.Console.Hosting
{
    public class HostedDaemonExe : IHostingBehaviour
    {
        private string configurationFilePath;
        private readonly string configurationRoot;
        private XmlDocument configurationXml;
        private readonly Dictionary<string, byte[]> configurationStreams = new Dictionary<string, byte[]>();

        public HostedDaemonExe(AssemblyName daemonAssemblyName, AppDomainSetup appDomainSetup = null)
        {
            if (daemonAssemblyName == null) throw new ArgumentNullException("daemonAssemblyName");
            AssemblyName = daemonAssemblyName;
            AppDomainSetup = appDomainSetup ?? AppDomain.CurrentDomain.SetupInformation;
            configurationRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        public AssemblyName AssemblyName { get; private set; }
        public AppDomainSetup AppDomainSetup { get; private set; }

        public HostedDaemonExe UseConfiguration(XmlDocument configuration, bool? keepExistingBindingRedirects = null)
        {
            var clonedConfiguration = (XmlDocument)configuration.Clone();
            if (keepExistingBindingRedirects != false)
            {
                if (TryReadBindingRedirects(keepExistingBindingRedirects == true, out var bindingRedirects))
                {
                    bindingRedirects.ApplyTo(clonedConfiguration);
                }
            }
            UseConfigurationInternal(clonedConfiguration);
            return this;
        }

        private void UseConfigurationInternal(XmlDocument configuration)
        {
            configurationFilePath = Path.Combine(configurationRoot, Path.GetRandomFileName());
            configurationXml = configuration;
            AppDomainSetup.ConfigurationFile = configurationFilePath;
        }

        /// <summary>
        /// Supply an additional configuration file at a path relative to the main one.
        /// </summary>
        /// <remarks>
        /// This won't have any (useful) effect if you don't provide a main configuration file as well.
        /// </remarks>
        public HostedDaemonExe UseAdditionalConfiguration(Stream configuration, string relativePath)
        {
            using (var copy = new MemoryStream())
            {
                configuration.CopyTo(copy);
                AddConfigurationStream(copy, relativePath);
                return this;
            }
        }

        public HostedDaemonExe UseAdditionalConfiguration(XmlDocument configuration, string relativePath)
        {
            using (var ms = new MemoryStream())
            {
                configuration.Save(ms);
                ms.Position = 0;
                AddConfigurationStream(ms, relativePath);
                return this;
            }
        }

        private void AddConfigurationStream(MemoryStream stream, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentNullException(nameof(relativePath));
            if (Path.IsPathRooted(relativePath)) throw new ArgumentException("Must specify a relative path.", nameof(relativePath));
            configurationStreams.Add(relativePath, stream.ToArray());
        }

        public XmlDocument ReadOriginalConfiguration(bool throwIfNotAFilesystemAssembly = false)
        {
            var assemblyLocation = new Uri(AssemblyName.CodeBase);

            if (!assemblyLocation.IsFile || !File.Exists(assemblyLocation.LocalPath))
            {
                // If we're trying to reaed configuration but the assembly isn't where we expect, it's
                // possibly GAC'd. If the caller's expecting this, it shouldn't ask us to look for the
                // configuration file in the first place.
                if (throwIfNotAFilesystemAssembly) throw new FileNotFoundException($"Unable to read existing binding redirects because the assembly file '{assemblyLocation}' could not be found.");
                return null;
            }
            var likelyConfigurationLocation = $"{assemblyLocation.LocalPath}.config";
            if (!File.Exists(likelyConfigurationLocation))
            {
                // If we're looking for the configuration of an assembly on the filesystem, it is still
                // permissible for the entire configuration file to be simply absent.
                return null;
            }
            var xml = new XmlDocument();
            xml.Load(likelyConfigurationLocation);
            return xml;
        }

        private bool TryReadBindingRedirects(bool throwIfNotAFilesystemAssembly, out BindingRedirects bindingRedirects)
        {
            var xml = ReadOriginalConfiguration(throwIfNotAFilesystemAssembly);
            if (xml == null)
            {
                bindingRedirects = null;
                return false;
            }
            bindingRedirects = BindingRedirects.ReadFrom(xml);
            return true;
        }

        public static HostedDaemonExe FromAssemblyFile(string file)
        {
            if (!Path.IsPathRooted(file)) throw new ArgumentException("Specified path is not absolute.");
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
            if (configurationXml == null) return;

            CleanConfigurationRoot();
            Directory.CreateDirectory(configurationRoot);
            configurationXml.Save(configurationFilePath);
            foreach (var config in configurationStreams)
            {
                var filePath = Path.GetFullPath(Path.Combine(configurationRoot, config.Key));
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllBytes(filePath, config.Value);
            }
        }

        private void CleanConfigurationRoot()
        {
            if (!Directory.Exists(configurationRoot)) return;
            Directory.Delete(configurationRoot, true);
        }

        void IHostingBehaviour.OnAfterStop()
        {
            try
            {
                CleanConfigurationRoot();
            } catch { }
        }
    }
}
