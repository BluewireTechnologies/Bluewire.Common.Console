using System;
using System.IO;
using Bluewire.Common.Console.Arguments;
using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Logging
{
    public abstract class LoggingPolicy
    {
        protected abstract void Initialise(IExecutionEnvironment environment);
        protected abstract void ShutDown();

        private static LoggingPolicy current;

        public static IDisposable Register(SessionArguments session, LoggingPolicy policy)
        {
            var environment = new ApplicationEnvironment(Path.GetFileNameWithoutExtension(session.Application));
            return Register(environment, policy);
        }

        public static IDisposable Register(IExecutionEnvironment environment, LoggingPolicy policy)
        {
            lock (typeof(LoggingPolicy))
            {
                if (!ReferenceEquals(current, null)) throw new InvalidOperationException($"Logging policy already configured: {current}");
                try
                {
                    policy.Initialise(environment);
                }
                catch
                {
                    try { policy.ShutDown(); } catch { }
                    throw;
                }
                current = policy;
                return new PolicyLifetime(policy);
            }
        }

        private static void Unregister(LoggingPolicy policy)
        {
            if (policy == null) return;
            lock (typeof(LoggingPolicy))
            {
                if (!ReferenceEquals(current, policy)) return;
                current = null;
                policy.ShutDown();
            }
        }

        internal static void Reset()
        {
            Unregister(current);
        }

        class PolicyLifetime : IDisposable
        {
            private readonly LoggingPolicy policy;

            public PolicyLifetime(LoggingPolicy policy)
            {
                this.policy = policy;
            }

            void IDisposable.Dispose() => Unregister(policy);
        }
    }
}
