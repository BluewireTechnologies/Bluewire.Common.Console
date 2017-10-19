using System;

namespace Bluewire.Common.NativeMessaging.Installation
{
    public class ManifestDescription
    {
        public ManifestDescription(string name, string path = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            Name = name;
            Path = path;
        }

        public string Name { get; }
        public string Path { get; }
    }
}
