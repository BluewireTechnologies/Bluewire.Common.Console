using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Bluewire.Common.Console.Hosting
{
    public class ProcessDaemonExe
    {
        private readonly string configurationFilePath;
        private string originalConfigurationFilePath;
        private XmlDocument configurationXml;
        private readonly Dictionary<string, byte[]> configurationStreams = new Dictionary<string, byte[]>();
        public string ApplicationSourceDirectory { get; }
        public string ApplicationSourceFile { get; }

        public ProcessDaemonExe(AssemblyName daemonAssemblyName)
        {
            if (daemonAssemblyName == null) throw new ArgumentNullException("daemonAssemblyName");
            var codeBaseUri = new Uri(daemonAssemblyName.CodeBase);
            if (string.IsNullOrWhiteSpace(codeBaseUri.LocalPath)) throw new ArgumentException($"Not a local path: {daemonAssemblyName.CodeBase}");

            AssemblyName = daemonAssemblyName;
            ApplicationSourceFile = codeBaseUri.LocalPath;
            ApplicationSourceDirectory = Path.GetDirectoryName(codeBaseUri.LocalPath);
            configurationFilePath = ApplicationSourceFile + ".config";
        }

        void BackupConfiguration()
        {
            originalConfigurationFilePath = $"{configurationFilePath}.original";
            if (File.Exists(originalConfigurationFilePath)) throw new InvalidOperationException("Configuration file backup already exists.");
            if (File.Exists(configurationFilePath)) File.Copy(configurationFilePath, originalConfigurationFilePath);
        }

        public AssemblyName AssemblyName { get; private set; }

        public ProcessDaemonExe UseConfiguration(XmlDocument configuration, bool? keepExistingBindingRedirects = null)
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
            configurationXml = configuration;
        }

        /// <summary>
        /// Supply an additional configuration file at a path relative to the main one.
        /// </summary>
        /// <remarks>
        /// This won't have any (useful) effect if you don't provide a main configuration file as well.
        /// </remarks>
        public ProcessDaemonExe UseAdditionalConfiguration(Stream configuration, string relativePath)
        {
            using (var copy = new MemoryStream())
            {
                configuration.CopyTo(copy);
                AddConfigurationStream(copy, relativePath);
                return this;
            }
        }

        public ProcessDaemonExe UseAdditionalConfiguration(XmlDocument configuration, string relativePath)
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

        public XmlDocument ReadCurrentConfiguration(bool throwIfNotAFilesystemAssembly = false)
        {
            return ReadConfiguration(configurationFilePath, throwIfNotAFilesystemAssembly);
        }

        public XmlDocument ReadOriginalConfiguration(bool throwIfNotAFilesystemAssembly = false)
        {
            // If we didn't back it up, we can't be sure that it's the original file.
            if (originalConfigurationFilePath == null) throw new InvalidOperationException("Original configuration file is not available.");

            return ReadConfiguration(originalConfigurationFilePath, throwIfNotAFilesystemAssembly);
        }

        private XmlDocument ReadConfiguration(string path, bool throwIfNotAFilesystemAssembly = false)
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
            if (!File.Exists(path))
            {
                // If we're looking for the configuration of an assembly on the filesystem, it is still
                // permissible for the entire configuration file to be simply absent.
                return null;
            }
            var xml = new XmlDocument();
            xml.Load(path);
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

        public static ShadowCopiedProcessDaemonExe FromShadowCopiedAssemblyFile(string file)
        {
            if (!Path.IsPathRooted(file)) throw new ArgumentException("Specified path is not absolute.");
            var assemblyName = AssemblyName.GetAssemblyName(file);
            return FromShadowCopiedAssemblyFile(assemblyName);
        }

        public static ShadowCopiedProcessDaemonExe FromShadowCopiedAssemblyFile(AssemblyName assemblyName)
        {
            // Apply the usual validation:
            var sourceDaemon = new ProcessDaemonExe(assemblyName);

            var container = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                new DirectoryCopier().Copy(sourceDaemon.ApplicationSourceDirectory, container);
                var targetFile = Path.Combine(container, Path.GetFileName(sourceDaemon.ApplicationSourceFile));
                var targetAssemblyName = AssemblyName.GetAssemblyName(targetFile);
                var copy = new ShadowCopiedProcessDaemonExe(new ProcessDaemonExe(targetAssemblyName), container);
                copy.Daemon.BackupConfiguration();
                return copy;
            }
            catch
            {
                Directory.Delete(container, true);
                throw;
            }
        }

        public void OnBeforeStart()
        {
            if (configurationXml != null)
            {
                configurationXml.Save(configurationFilePath);
            }
            foreach (var config in configurationStreams)
            {
                var filePath = Path.GetFullPath(Path.Combine(ApplicationSourceDirectory, config.Key));
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllBytes(filePath, config.Value);
            }
        }

        public class ShadowCopiedProcessDaemonExe : IDisposable
        {
            internal string TemporaryContainer { get; }

            internal ShadowCopiedProcessDaemonExe(ProcessDaemonExe daemon, string temporaryContainer)
            {
                Daemon = daemon;
                this.TemporaryContainer = temporaryContainer;
            }

            public ProcessDaemonExe Daemon { get; }

            public void CleanUp()
            {
                Directory.Delete(TemporaryContainer, true);
            }

            public void Dispose()
            {
                try
                {
                    CleanUp();
                }
                catch
                {
                }
            }
        }
    }
}
