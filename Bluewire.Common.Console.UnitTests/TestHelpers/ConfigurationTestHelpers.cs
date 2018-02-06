using System;
using System.IO;
using System.Reflection;
using Bluewire.Common.Console.NUnit3.Filesystem;

namespace Bluewire.Common.Console.UnitTests.TestHelpers
{
    class ConfigurationTestHelpers
    {
        public static Stream GetConfigurationStream(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{assembly.GetName().Name}.{name}";
            var resource = assembly.GetManifestResourceStream(resourceName);
            if (resource == null) throw new ArgumentException($"Resource {resourceName} was not found.", "name");
            return resource;
        }

        public static string GetConfigurationStreamAsTempFile(string name)
        {
            var filePath = Path.Combine(TemporaryDirectory.ForCurrentTest(), name);
            using (var configStream = GetConfigurationStream(name))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using (var reader = new StreamReader(configStream))
                {
                    File.WriteAllText(filePath, reader.ReadToEnd());
                }
                return filePath;
            }
        }
    }
}
