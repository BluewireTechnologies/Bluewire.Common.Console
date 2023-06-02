using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Bluewire.Common.Console.Hosting
{
    class DirectoryCopier
    {
        public void Copy(string source, string destination)
        {
            if (!Path.IsPathRooted(source)) throw new ArgumentException($"Specified path is not absolute: {source}", nameof(source));
            if (!Path.IsPathRooted(destination)) throw new ArgumentException($"Specified path is not absolute: {destination}", nameof(destination));

            var exploreQueue = new Queue<Pair>();
            exploreQueue.Enqueue(new Pair { Source = source, Destination = destination, IsFile = File.Exists(source) });
            var copyQueue = new Queue<Pair>();

            while (exploreQueue.Count > 0)
            {
                var item = exploreQueue.Dequeue();
                if (item.IsFile)
                {
                    if (File.Exists(item.Destination)) throw new ArgumentException($"Destination file already exists: {item.Destination}");
                    if (Directory.Exists(item.Destination)) throw new ArgumentException($"Destination file already exists as a directory: {item.Destination}");
                }
                copyQueue.Enqueue(item);
                if (!item.IsFile)
                {
                    foreach (var child in new DirectoryInfo(item.Source).EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly))
                    {
                        exploreQueue.Enqueue(new Pair { Source = child.FullName, Destination = Path.Combine(item.Destination, child.Name), IsFile = child is FileInfo });
                    }
                }
            }

            while (copyQueue.Count > 0)
            {
                var item = copyQueue.Dequeue();
                if (item.IsFile)
                {
                    if (File.Exists(item.Destination)) throw new ArgumentException($"Destination file already exists: {item.Destination}");
                    if (Directory.Exists(item.Destination)) throw new ArgumentException($"Destination file already exists as a directory: {item.Destination}");
                    if (IsExcludedFile(Path.GetFileName(item.Source))) continue;
                    File.Copy(item.Source, item.Destination);
                }
                else
                {
                    Directory.CreateDirectory(item.Destination);
                }
            }
        }

        private static bool IsExcludedFile(string name)
        {
            // NUnit runner trace files.
            if (Regex.IsMatch(name, @"^InternalTrace\.\d+\.log")) return true;

            return false;
        }

        struct Pair
        {
            public string Source { get; set; }
            public string Destination { get; set; }
            public bool IsFile { get; set; }
        }
    }
}
