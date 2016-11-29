using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Bluewire.Common.Console.NUnit3.Filesystem
{
    public class PathSegmentSanitiser
    {
        private readonly Regex rxInvalidChars;

        public PathSegmentSanitiser()
        {
            var invalidChars = new String(Path.GetInvalidFileNameChars());
            rxInvalidChars = new Regex($"[{Regex.Escape(invalidChars)}]", RegexOptions.Compiled);
        }

        public static readonly PathSegmentSanitiser Instance = new PathSegmentSanitiser();

        public string Sanitise(string segment)
        {
            return rxInvalidChars.Replace(segment, "_");
        }
    }
}
