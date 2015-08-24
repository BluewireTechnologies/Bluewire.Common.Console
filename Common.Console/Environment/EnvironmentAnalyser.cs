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

        public InitialisedHostedEnvironment GetHostedEnvironment()
        {
            var environment = GetEnvironment() as InitialisedHostedEnvironment;
            if (environment == null) throw new InvalidOperationException("Not a hosted environment, or the environment has not yet been defined.");
            return environment;
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
            
            if (!HasSTDERR())
            {
                // If we can't open STDERR, probably running inside a noninteractive service.
                // We cannot and should not detect if we're 'running as a service' because there are so many
                // ways which 'almost' work and none that entirely work. All we really care about is 'can we
                // sensibly log to STDERR?'
                return new ServiceEnvironment(entryAssembly);
            }

            return new ApplicationEnvironment(entryAssembly);
        }


        private static bool HasSTDERR()
        {
            using (var stderr = System.Console.OpenStandardError())
            {
                if (stderr == null) return false;
                if (!stderr.CanWrite) return false;
                if (stderr.GetType().FullName == "System.IO.Stream+NullStream") return false;
            }
            return true;
        }

        public InitialisedHostedEnvironment DefineHostedEnvironment(HostedEnvironmentDefinition definition)
        {
            lock (initLock)
            {
                AssertThatEnvironmentWasNotAlreadyConfiguredAsSomethingElse();

                return InitialiseHostedEnvironment(definition, thisEnvironment);
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

        private static InitialisedHostedEnvironment InitialiseHostedEnvironment(HostedEnvironmentDefinition definition, IExecutionEnvironment detected)
        {
            var hostedEnvironment = new InitialisedHostedEnvironment(definition, detected);
            thisEnvironment = hostedEnvironment;
            return hostedEnvironment;
        }
    }
}