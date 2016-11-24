using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Bluewire.Common.Console.NUnit3.Filesystem
{
    static class TemporaryDirectoryForTest
    {
        // I think we should not see more than one assembly per appdomain, but would not like to rely upon it.
        private static readonly ConcurrentDictionary<Assembly, string> temporaryDirectoriesForAssemblies = new ConcurrentDictionary<Assembly, string>();
        
        private static string GetTemporaryDirectoryPathForAssembly(Assembly assembly)
        {
            var pid = Process.GetCurrentProcess().Id;
            var shortenedName = GetShortenedAssemblyName(assembly);

            var assemblyDirectory = $"{PathSegmentSanitiser.Instance.Sanitise(shortenedName)}-{pid}";

            return Path.Combine(Path.GetTempPath(), "NUnit3", assemblyDirectory);
        }

        private static string GetSubdirectoryNameForTest(Assembly assembly, string testFullName)
        {
            var shortenedName = GetShortenedTestName(assembly, testFullName);
            return PathSegmentSanitiser.Instance.Sanitise(shortenedName);
        }

        private static string GetShortenedAssemblyName(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            return new PathSegmentShortener().AggressivelyShortenDottedSegment(assemblyName, 20);
        }

        private static string GetShortenedTestName(Assembly assembly, string testFullName)
        {
            var assemblyName = assembly.GetName().Name;
            Debug.Assert(testFullName != assemblyName); // Should be impossible.
            var testNameWithoutAssemblyPrefix = testFullName.StartsWith(assemblyName) ? testFullName.Substring(assemblyName.Length + 1) : testFullName;

            return new PathSegmentShortener().AggressivelyShortenDottedSegment(testNameWithoutAssemblyPrefix, 25);
        }

        private static string LookupTemporaryDirectoryForAssembly(Assembly assembly)
        {
            return temporaryDirectoriesForAssemblies.GetOrAdd(assembly, GetTemporaryDirectoryPathForAssembly);
        }

        public static void CleanTemporaryDirectoryForAssembly(Assembly assembly)
        {
            var location = GetTemporaryDirectoryPathForAssembly(assembly);
            if (!Directory.Exists(location)) return;
            try
            {
                Directory.Delete(location, false);
            }
            catch { }
        }

        private const string Bluewire_TemporaryDirectoryKey = "bluewire.temporary_directory";
        
        private static Test GetActualTestFromContext(TestContext context)
        {
            var type = context.Test.GetType();
            Debug.Assert(type == typeof(TestContext.TestAdapter));
            var field = type.GetField("_test", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(field != null);
            return (Test)field.GetValue(context.Test);
        }

        public static string Allocate(TestContext context)
        {
            return Allocate(GetActualTestFromContext(context));
        }

        public static string Allocate(ITest test)
        {
            var path = Get(test.Properties);
            if (path != null) return path;

            var newPath = Generate(test);
            test.Properties.Set(Bluewire_TemporaryDirectoryKey, newPath);
            return newPath;
        }

        public static string Get(IPropertyBag testProperties)
        {
            var path = testProperties.Get(Bluewire_TemporaryDirectoryKey);
            return path?.ToString();
        }

        private static string Generate(ITest test)
        {
            var testDetails = (Test)test;
            var containingType = testDetails.TypeInfo.Type;
            return Path.Combine(
                LookupTemporaryDirectoryForAssembly(containingType.Assembly),
                GetSubdirectoryNameForTest(containingType.Assembly, test.FullName));
        }
    }
}
