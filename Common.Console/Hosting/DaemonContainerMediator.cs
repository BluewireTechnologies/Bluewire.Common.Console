using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Hosting
{
    /// <summary>
    /// Runs inside the child appdomain to allow communication with the DaemonContainer.
    /// </summary>
    public sealed class DaemonContainerMediator : MarshalByRefObject
    {
        private InitialisedHostedEnvironment Environment
        {
            get
            {
                return new EnvironmentAnalyser().GetHostedEnvironment();
            }
        }

        public void InitialiseHostedEnvironment(HostedEnvironmentDefinition definition)
        {
            // DefineHostedEnvironment only succeeds on one thread, so this is safe:
            new EnvironmentAnalyser().DefineHostedEnvironment(definition);
        }
        
        public bool RequestShutdown(TimeSpan timeout)
        {
            return Environment.RequestShutdown().Wait(timeout);
        }

        public string[] GetDaemonNames()
        {
            return Environment.GetDaemonInfo().Select(d => d.Name).ToArray();
        }
    }
}
