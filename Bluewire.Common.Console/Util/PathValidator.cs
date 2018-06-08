using System.IO;

namespace Bluewire.Common.Console.Util
{
    public class PathValidator
    {
        /// <summary>
        /// Returns false if the file name contains invalid characters.
        /// </summary>
        public static bool IsValidFileName(string fileName)
        {
            return fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        /// <summary>
        /// Returns false if the path contains invalid characters.
        /// </summary>
        public static bool IsValidPath(string path)
        {
            return path.IndexOfAny(Path.GetInvalidPathChars()) < 0;
        }
    }
}
