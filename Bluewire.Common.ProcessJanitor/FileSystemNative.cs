using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Bluewire.Common.ProcessJanitor
{
    // https://stackoverflow.com/questions/2281531/how-can-i-compare-directory-paths-in-c/39399232#39399232
    internal static class FileSystemNative
    {
        public static bool IsDescendantDirectory(DirectoryInfo container, DirectoryInfo subject)
        {
            if (subject.Parent == null) return false;
            if (AreDirsEqual(container, subject.Parent)) return true;
            if (subject.FullName.Length < container.FullName.Length) return false;  // Probably...?
            return IsDescendantDirectory(container, subject.Parent);
        }

        public static bool AreDirsEqual(DirectoryInfo dir1, DirectoryInfo dir2, bool resolveJunctionAndNetworkPaths = true)
        {
            if (dir1 == null) throw new ArgumentNullException(nameof(dir1));
            if (dir2 == null) throw new ArgumentNullException(nameof(dir2));

            // Assume FullName normalizes/fixes case and path separators to Path.DirectorySeparatorChar
            if (StringComparer.OrdinalIgnoreCase.Equals(Normalise(dir1.FullName), Normalise(dir2.FullName))) return true;

            if ( !resolveJunctionAndNetworkPaths ) return false;
            return AreFileSystemObjectsEqual(RemoveRoot(Normalise(dir1.FullName)), RemoveRoot(Normalise(dir2.FullName)));
        }

        private static string Normalise(string path) => path.TrimEnd(Path.DirectorySeparatorChar);

        private static string RemoveRoot(string fullPath)
        {
            var index = fullPath.IndexOf(Path.DirectorySeparatorChar);
            if (index < 0) return fullPath;
            return fullPath.Substring(index);
        }

        private static bool AreFileSystemObjectsEqual(string dirName1, string dirName2)
        {
            // NOTE: we cannot lift the call to GetFileHandle out of this routine, because we _must_
            // have both file handles open simultaneously in order for the objectFileInfo comparison
            // to be guaranteed as valid.
            using (SafeFileHandle directoryHandle1 = GetFileHandle(dirName1), directoryHandle2 = GetFileHandle(dirName2))
            {
                BY_HANDLE_FILE_INFORMATION? objectFileInfo1 = GetFileInfo(directoryHandle1);
                BY_HANDLE_FILE_INFORMATION? objectFileInfo2 = GetFileInfo(directoryHandle2);
                return objectFileInfo1 != null
                       && objectFileInfo2 != null
                       && (objectFileInfo1.Value.FileIndexHigh == objectFileInfo2.Value.FileIndexHigh)
                       && (objectFileInfo1.Value.FileIndexLow == objectFileInfo2.Value.FileIndexLow)
                       && (objectFileInfo1.Value.VolumeSerialNumber == objectFileInfo2.Value.VolumeSerialNumber);
            }
        }

        private static SafeFileHandle GetFileHandle(string dirName)
        {
            const int FILE_ACCESS_NEITHER = 0;
            //const int FILE_SHARE_READ = 1;
            //const int FILE_SHARE_WRITE = 2;
            //const int FILE_SHARE_DELETE = 4;
            const int FILE_SHARE_ANY = 7;//FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE
            const int CREATION_DISPOSITION_OPEN_EXISTING = 3;
            const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
            return CreateFile(dirName, FILE_ACCESS_NEITHER, FILE_SHARE_ANY, System.IntPtr.Zero, CREATION_DISPOSITION_OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, System.IntPtr.Zero);
        }


        private static BY_HANDLE_FILE_INFORMATION? GetFileInfo(SafeFileHandle directoryHandle)
        {
            BY_HANDLE_FILE_INFORMATION objectFileInfo;
            if ((directoryHandle == null) || (!GetFileInformationByHandle(directoryHandle.DangerousGetHandle(), out objectFileInfo)))
            {
                return null;
            }
            return objectFileInfo;
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode,
            IntPtr SecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(IntPtr hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        [StructLayout(LayoutKind.Sequential)]
        private struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        };
    }
}
