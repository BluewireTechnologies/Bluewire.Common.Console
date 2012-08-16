using System;
using System.IO;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;

namespace Bluewire.Common.Console.Logging
{
    public abstract class OutputDescriptorBase : IOutputDescriptor, IOutputDescriptorConfiguration
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
                File = GetLogFilePath("{0}.log", this.ApplicationName),
                Layout = Init(new PatternLayout("%date [%3thread] %-5level %logger %ndc - %message%newline")),
                LockingModel = new FileAppender.MinimalLock()
            };
        }

        protected string GetLogFilePath(string filenamePattern, params object[] args)
        {
            var relativePath = String.Format(filenamePattern, args);
            if (String.IsNullOrEmpty(logRootDirectory)) return relativePath;
            return Path.Combine(logRootDirectory, relativePath);
        }

        private string logRootDirectory;
        void IOutputDescriptorConfiguration.SetLogRootDirectory(string rootDir)
        {
            this.logRootDirectory = rootDir;
        }
    }
}