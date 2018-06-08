using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using Bluewire.Common.Console.Util;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Repository.Hierarchy;

namespace Bluewire.Common.Console.Logging
{
    internal static class Log4NetHelper
    {
        public static Hierarchy DefaultHierachy => (Hierarchy)LogManager.GetRepository();

        public  static T Init<T>(T obj)
        {
            if (obj is IOptionHandler handler) handler.ActivateOptions();
            return obj;
        }

        internal static bool HasLog4NetConfiguration()
        {
            return ConfigurationManager.GetSection("log4net") != null;
        }

        internal static bool HasLog4NetConfiguration(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            if (!PathValidator.IsValidPath(filePath)) throw new ArgumentException($"Value must be a valid path: {filePath}", nameof(filePath));
            if (!Path.IsPathRooted(filePath)) throw new ArgumentException($"Not an absolute path: {filePath}", nameof(filePath));
            if (!File.Exists(filePath)) throw new ArgumentException($"File does not exist: {filePath}", nameof(filePath));

            var xml = new XmlDocument();
            xml.Load(filePath);
            return xml.GetElementsByTagName("log4net").Count > 0;
        }

        public static IEnumerable<IFilter> EnumerateFilterChain(this IFilter filter)
        {
            while (filter != null)
            {
                yield return filter;
                filter = filter.Next;
            }
        }

        public static void ReentrancySafeSetLoggerLevel(this Logger logger, Level level)
        {
            // ReSharper disable RedundantCheckBeforeAssignment
            // not necessarily redundant. does setting Level trigger the ConfigurationChanged event?
            if (logger.Level == level) return;
            // ReSharper restore RedundantCheckBeforeAssignment
            logger.Level = level;
        }

        public static void AddFilterIfPossible<T>(this IAppender appender, T filter) where T : IFilter
        {
            if (appender is AppenderSkeleton filterable)
            {
                if (!filterable.FilterHead.EnumerateFilterChain().OfType<T>().Any())
                {
                    filterable.AddFilter(Init(filter));
                }
            }
        }

        public static IAppender GetAppenderByName(this Hierarchy hierarchy, string name)
        {
            return hierarchy.GetAppenders().FirstOrDefault(a => a.Name == name);
        }
    }
}
