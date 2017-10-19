using System;
using System.IO;
using Microsoft.Win32;

namespace Bluewire.Common.NativeMessaging.Installation
{
    public class NativeHostInstaller
    {
        private readonly RegistryHive hive;

        public NativeHostInstaller(RegistryHive hive)
        {
            this.hive = hive;
        }

        public void Install(ManifestDescription manifest)
        {
            if (String.IsNullOrWhiteSpace(manifest.Path)) throw new ArgumentException("No Path specified for manifest");
            if (!File.Exists(manifest.Path)) throw new FileNotFoundException($"Manifest file does not exist: {manifest.Path}");

            new ChromeInstaller(hive).Install(manifest);
            new FirefoxInstaller(hive).Install(manifest);
        }

        public void Uninstall(ManifestDescription manifest)
        {
            new ChromeInstaller(hive).Uninstall(manifest);
            new FirefoxInstaller(hive).Uninstall(manifest);
        }

        interface IKeyInstaller
        {
            void Install(ManifestDescription manifest);
            void Uninstall(ManifestDescription manifest);
        }

        class ChromeInstaller : IKeyInstaller
        {
            private readonly RegistryHive hive;

            public ChromeInstaller(RegistryHive hive)
            {
                this.hive = hive;
            }

            public void Install(ManifestDescription manifest)
            {
                using (var root = RegistryKey.OpenBaseKey(hive, RegistryView.Registry32))
                using (var subkey = root.OpenSubKey(@"Software\Google\Chrome\NativeMessagingHosts", true))
                {
                    using (var manifestKey = subkey?.CreateSubKey(manifest.Name))
                    {
                        manifestKey?.SetValue(null, manifest.Path);
                    }
                }
            }

            public void Uninstall(ManifestDescription manifest)
            {
                using (var root = RegistryKey.OpenBaseKey(hive, RegistryView.Registry32))
                using (var subkey = root.OpenSubKey(@"Software\Google\Chrome\NativeMessagingHosts", true))
                {
                    subkey?.DeleteSubKey(manifest.Name);
                }
            }
        }

        class FirefoxInstaller : IKeyInstaller
        {
            private readonly RegistryHive hive;

            public FirefoxInstaller(RegistryHive hive)
            {
                this.hive = hive;
            }

            public void Install(ManifestDescription manifest)
            {
                using (var root = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                using (var subkey = root.OpenSubKey(@"Software\Mozilla\NativeMessagingHosts", true))
                {
                    using (var manifestKey = subkey?.CreateSubKey(manifest.Name))
                    {
                        manifestKey?.SetValue(null, manifest.Path);
                    }
                }
            }

            public void Uninstall(ManifestDescription manifest)
            {
                using (var root = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                using (var subkey = root.OpenSubKey(@"Software\Mozilla\NativeMessagingHosts", true))
                {
                    subkey?.DeleteSubKey(manifest.Name);
                }
            }
        }
    }
}
