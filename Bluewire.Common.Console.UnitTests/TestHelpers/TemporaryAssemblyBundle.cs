using System;
using System.IO;
using System.Reflection;

namespace Bluewire.Common.Console.UnitTests.TestHelpers
{
    public class TemporaryAssemblyBundle : IDisposable
    {
        private readonly string targetDirectory;

        public TemporaryAssemblyBundle() : this(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()))
        {
        }

        public TemporaryAssemblyBundle(string targetDirectory)
        {
            if (Directory.Exists(targetDirectory)) throw new ArgumentException($"Target directory already exists: {targetDirectory}");
            this.targetDirectory = targetDirectory;
            Directory.CreateDirectory(targetDirectory);
        }

        public string Add(Assembly assembly)
        {
            var targetPath = Path.Combine(targetDirectory, Path.GetFileName(assembly.Location));
            File.Copy(assembly.Location, targetPath);
            return targetPath;
        }

        public void Dispose()
        {
            Directory.Delete(targetDirectory, true);
        }
    }
}
