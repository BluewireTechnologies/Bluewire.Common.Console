using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bluewire.Common.Console
{
    public static class ArgumentClassExtensions
    {
        public static IEnumerable<string> ResolveWildcards(this IFileNameListArgument arguments)
        {
            return ResolveWildcards(arguments, Directory.GetCurrentDirectory());
        }

        public static IEnumerable<string> ResolveWildcards(this IFileNameListArgument arguments, string relativeTo)
        {
            if(!Path.IsPathRooted(relativeTo)) throw new InvalidOperationException(String.Format("Not an absolute path: {0}", relativeTo));
            return arguments.FileNames.SelectMany(f => GetFilesInDirectory(relativeTo, f));
        }

        private static string[] GetFilesInDirectory(string relativeTo, string arg)
        {
            if (Path.GetInvalidPathChars().Intersect(arg).Any() || Path.GetInvalidFileNameChars().Intersect(arg).Any())
            {
                var directory = Path.Combine(relativeTo, GetDirectoryOfArg(arg));
                var wildcard = Path.GetFileName(arg);
                return Directory.GetFiles(directory, wildcard);
            }
            else
            {
                var fullPath = Path.Combine(relativeTo, arg);
                return new string[] { fullPath };
            }
        }

        private static string GetDirectoryOfArg(string arg)
        {
            if(String.IsNullOrEmpty(arg)) return ".";
            return Path.GetDirectoryName(arg);
        }
    }
}