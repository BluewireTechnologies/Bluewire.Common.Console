using System;
using System.Reflection;
using System.Runtime.Serialization;
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
                if (result is int i) return i;
                return 0;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException == null) throw new HostedEntryPointException(ex); // Failed to invoke entry point?

                throw new HostedEntryPointException(ex.InnerException);
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

    [Serializable]
    public class HostedEntryPointException : Exception
    {
        public int ExitCode { get; private set; }

        public HostedEntryPointException(Exception exception)
            : base("The application's entry point terminated with an unhandled exception.", exception)
        {
            // Unhandled exceptions thrown by the entry point generate an exit code of 255 for console apps.
            ExitCode = 255;
        }

        public HostedEntryPointException(TargetInvocationException exception)
            : base("Failed to invoke the application's entry point.", exception)
        {
            // Unhandled exceptions thrown by the entry point generate an exit code of 255 for console apps.
            ExitCode = 255;
        }
        public HostedEntryPointException(int errorCode) : base(String.Format("The application's entry point terminated with code {0}", errorCode))
        {
            ExitCode = errorCode;
        }

        public HostedEntryPointException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
