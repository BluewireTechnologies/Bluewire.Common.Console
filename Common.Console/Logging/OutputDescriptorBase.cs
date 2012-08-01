using System;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;

namespace Bluewire.Common.Console.Logging
{
    public abstract class OutputDescriptorBase : IOutputDescriptor
    {
        protected OutputDescriptorBase(string applicationName)
        {
            ApplicationName = applicationName;
        }

        protected readonly string ApplicationName;

        public abstract IAppender CreateStdOut();
        public abstract IAppender CreateStdErr();

        protected static T Init<T>(T obj)
        {
            if (obj is IOptionHandler) ((IOptionHandler)obj).ActivateOptions();
            return obj;
        }

        public virtual IAppender CreateDefaultLog()
        {
            return new FileAppender
            {
                File = String.Format("{0}.log", this.ApplicationName),
                Layout = Init(new PatternLayout("%date [%3thread] %-5level %logger %ndc - %message%newline")),
                LockingModel = new FileAppender.MinimalLock()
            };
        }
    }
}