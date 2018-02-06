using System.IO;

namespace Bluewire.Common.Console.NUnit3.Filesystem
{
    public static class FileSystemHelpers
    {
        public static void CleanDirectory(string directoryPath)
        {
            Clean(new DirectoryInfo(directoryPath));
        }

        public static void CleanFile(string filePath)
        {
            Clean(new FileInfo(filePath));
        }

        public static void Clean(FileSystemInfo entry)
        {
            if (entry is DirectoryInfo directory)
            {
                foreach (var fileSystemInfo in directory.EnumerateFileSystemInfos())
                {
                    Clean(fileSystemInfo);
                }
            }
            entry.Attributes = FileAttributes.Normal;
            entry.Delete();
        }
    }
}
