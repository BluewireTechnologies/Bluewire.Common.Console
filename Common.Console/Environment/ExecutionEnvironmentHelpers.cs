using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Bluewire.Common.Console.Environment
{
    static class ExecutionEnvironmentHelpers
    {
        public static Assembly GuessPrimaryAssembly()
        {
            // First check: use the entry assembly.
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null) return entryAssembly;

            // Otherwise, walk the stack to find the first assembly which isn't this one.
            // May give odd results when the first call into Bluewire.Common.Console is made from a
            // library rather than the application itself.
            var thisAssembly = Assembly.GetExecutingAssembly();
            var stack = new StackTrace().GetFrames();
            if (stack == null) return null; // When can this happen?
            return stack.Select(f => f.GetMethod().ReflectedType.Assembly).FirstOrDefault(a => a != thisAssembly);
        }
    }
}