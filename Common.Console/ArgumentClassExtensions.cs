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
            return arguments.FileNames.SelectMany(GetFilesInDirectory);
        }

        private static string[] GetFilesInDirectory(string arg)
        {
            return Directory.GetFiles(Path.GetFullPath(GetDirectoryOfArg(arg)), Path.GetFileName(arg));
        }

        private static string GetDirectoryOfArg(string arg)
        {
            if(String.IsNullOrEmpty(arg)) return ".";
            return Path.GetDirectoryName(arg);
        }
    }
}