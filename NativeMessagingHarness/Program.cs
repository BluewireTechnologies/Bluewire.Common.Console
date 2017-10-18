using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bluewire.Common.NativeMessaging;
using Bluewire.Common.NativeMessaging.Installation;
using Microsoft.Win32;

namespace NativeMessagingHarness
{
    class Program
    {
        static int Main(string[] args)
        {
            var argList = args.ToList();

            if (argList.Remove("--uninstall"))
            {
                new NativeHostInstaller(RegistryHive.CurrentUser).Uninstall(new ManifestDescription(Name));
            }
            if (argList.Remove("--install"))
            {
                var manifestPath = WriteManifest(Assembly.GetExecutingAssembly());
                new NativeHostInstaller(RegistryHive.CurrentUser).Install(new ManifestDescription(Name, manifestPath));
            }
            if (argList.Count != args.Length) return 0;

            return new Program().Run(args).GetAwaiter().GetResult();
        }

        private async Task<int> Run(string[] arguments)
        {
            var parser = new NativeHostArgumentParser();
            var session = parser.Parse(arguments);

            var service = new EchoService(session);
            await service.Run();

            return 0;
        }

        private const string Name = "com.bluewire_technologies.chrome.native_messaging_harness";

        private static string WriteManifest(Assembly assembly)
        {
            var path = Path.GetDirectoryName(Path.GetFullPath(assembly.Location));
            if (path == null) throw new InvalidOperationException($"Assembly Location is not a path: {assembly.Location}");

            var manifestPath = Path.Combine(path, "manifest.json");
            File.WriteAllText(manifestPath, $@"{{
    ""name"": ""{Name}"",
    ""description"": ""Bluewire Technologies test harness for Chrome Native Messaging"",
    ""path"": ""{Path.GetFileName(assembly.CodeBase)}"",
    ""type"": ""stdio"",
    ""allowed_origins"": [
        ""chrome-extension://knldjmfmopnpolahpmmgbagdohdnhkik/""
    ]
}}
");
            return manifestPath;
        }
    }
}
