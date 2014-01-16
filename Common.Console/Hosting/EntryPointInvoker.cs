using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        private int InvokeEntryPointAsConsoleApplication(string[] arguments)
        {
            try
            {
                var result = assembly.EntryPoint.Invoke(null, new object[] { arguments });
                if (result is int)
                {
                    return (int)result;
                }
                return 0;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException == null) throw; // Failed to invoke entry point?

                // Unhandled exceptions thrown by the entry point generate an exit code of 255 for console apps.
                return 255;
            }
        }

        public int Invoke(string[] arguments)
        {
            return InvokeEntryPointAsConsoleApplication(arguments);
        }

        public void InvokeAsync(AsyncExitCodeReceiver receiver, string[] arguments)
        {
            Task.Factory.StartNew(() => InvokeEntryPointAsConsoleApplication(arguments)).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    receiver.Exception(t.Exception);
                }
                else
                {
                    receiver.ExitCode(t.Result);
                }
            });
        }
    }
}
