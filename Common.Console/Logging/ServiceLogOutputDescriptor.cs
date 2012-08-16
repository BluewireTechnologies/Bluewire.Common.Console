using System;
using System.IO;
using log4net.Appender;
using log4net.Layout;

namespace Bluewire.Common.Console.Logging
{
    public class ServiceLogOutputDescriptor : OutputDescriptorBase
    {
        public ServiceLogOutputDescriptor(string applicationName) : base(applicationName)
        {
        }

        private IAppender CreateAppender()
        {
            var appender = (FileAppender)CreateDefaultLog();
            appender.File = GetLogFilePath("{0}.console.log", ApplicationName);
            return appender;
        }

        public override IAppender CreateStdOut()
        {
            return CreateAppender();
        }

        public override IAppender CreateStdErr()
        {
            return CreateAppender();
        }
    }
}