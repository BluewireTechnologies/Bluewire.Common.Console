using System;
using System.Diagnostics;
using System.Reflection;

namespace Bluewire.Common.Console.Environment
{
    public class EnvironmentAnalyser
    {
        private static IExecutionEnvironment thisEnvironment;
        private static readonly object initLock = new object();

        public IExecutionEnvironment GetEnvironment()
        {
            if (thisEnvironment != null) return thisEnvironment;
            lock (initLock)
            {
                if (thisEnvironment == null) thisEnvironment = DetermineEnvironment();
                return thisEnvironment;
            }
        }

        private static IExecutionEnvironment DetermineEnvironment()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                // If we're running code in this appdomain and it has no entry assembly, it probably wasn't
                // run from the command line or as a service.
                return new HostedEnvironment();
            }

            if (NativeMethods.IsRunningAsService())
            {
                return new ServiceEnvironment(entryAssembly);
            }

            return new ApplicationEnvironment(entryAssembly);
        }

        public void DefineHostedEnvironment(HostedEnvironmentDefinition definition)
        {
            lock (initLock)
            {
                AssertThatEnvironmentWasNotAlreadyConfiguredAsSomethingElse();

                InitialiseHostedEnvironment(definition, thisEnvironment);
            }
        }

        private void AssertThatEnvironmentWasNotAlreadyConfiguredAsSomethingElse()
        {
            var wasAlreadyInitialised = thisEnvironment != null;
            var environment = GetEnvironment();
            Debug.Assert(environment != null);

            if (environment is InitialisedHostedEnvironment) throw new InvalidOperationException("Cannot redefine a hosted environment after initialisation.");

            if (environment is HostedEnvironment) return; // Detected as hosted, awaiting initialisation.
            if (!wasAlreadyInitialised) return; // Detected as something else just now, but not yet used. Allow host to override whatever we detected.

            throw new InvalidOperationException(String.Format("Environment has already been initialised as {0}.", environment.GetType().Name));
        }

        private static void InitialiseHostedEnvironment(HostedEnvironmentDefinition definition, IExecutionEnvironment detected)
        {
            thisEnvironment = new InitialisedHostedEnvironment(definition, detected);
        }
    }
}