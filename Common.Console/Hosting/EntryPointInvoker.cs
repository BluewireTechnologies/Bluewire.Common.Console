using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bluewire.Common.Console.Hosting
{
    /// <summary>
    /// Shim for running a console app without showing a window.
    /// </summary>
    public class EntryPointInvoker : MarshalByRefObject
    {
        private static EntryPointInvoker singleton;
        private readonly Assembly assembly;

        private void AssertSingletonInstance()
        {
            lock (typeof(EntryPointInvoker))
            {
                if (singleton != null) throw new InvalidOperationException();
                singleton = this;
            }
        }

        public EntryPointInvoker(AssemblyName assemblyName)
        {
            assembly = Assembly.Load(assemblyName);
            AssertSingletonInstance();
        }

        public int Invoke(string[] arguments)
        {
            var result = assembly.EntryPoint.Invoke(null, new object[] { arguments });
            if(assembly.EntryPoint.ReturnType == typeof(void)) return 0;
            if (result is int)
            {
                return (int)result;
            }
            return 0;
        }
    }
}
