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
            var appender = (RollingFileAppender)CreateDefaultLog();
            appender.File = GetLogFilePath("{0}.console", ApplicationName);
            return appender;
        }
        
        public override IAppender CreateStdErr()
        {
            return CreateAppender();
        }
    }
}