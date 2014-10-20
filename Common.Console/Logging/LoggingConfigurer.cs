﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace Bluewire.Common.Console.Logging
{
    public class LoggingConfigurer : IDisposable
    {
        private readonly IOutputDescriptor outputDescriptor;

        public static void EnsureLog4NetIsConfigured()
        {
            var repository = LogManager.GetRepository();
            // No idea if this is the correct thing to do to cope with the situation where logging is never configured (ie. the
            // config file is invalid, or the configurator is never invoked) but it appears to pass the tests:
            if (!((Hierarchy)repository).Configured)
            {
                GetRootLogger(repository).Level = Level.Warn; // default
                ((Hierarchy)repository).Configured = true;
            }
        }

        public LoggingConfigurer(IOutputDescriptor outputDescriptor)
        {
            this.outputDescriptor = outputDescriptor;
            var repository = LogManager.GetRepository();

            repository.ConfigurationChanged += OnConfigurationChanged;

            ConfigureDefaultLogging(repository);
            EnsureLog4NetIsConfigured();
        }

        private void OnConfigurationChanged(object sender, EventArgs args)
        {
            var repository = LogManager.GetRepository();
            ConfigureDefaultLogging(repository);
        }

        /// <summary>
        /// Messages at or above this threshold will be logged to the console.
        /// </summary>
        private Level consoleLogLevel = Level.Warn;

        public Level ConsoleVerbosity
        {
            get { return consoleLogLevel; }
            set
            {
                consoleLogLevel = value;
                SetLoggerLevel((Logger) Console.Logger, consoleLogLevel);
            }
        }

        public ILog Console { get { return LogManager.GetLogger("Console"); } }

        private void SetLoggerLevel(Logger logger, Level level)
        {
            // ReSharper disable RedundantCheckBeforeAssignment
            // not necessarily redundant. does setting Level trigger the ConfigurationChanged event?
            if (logger.Level != level)
                // ReSharper restore RedundantCheckBeforeAssignment
            {
                logger.Level = level;
            }
        }

        private void ConfigureDefaultLogging(ILoggerRepository repository)
        {
            var root = GetRootLogger(repository);
            if (root.Appenders.Count == 0)
            {
                root.AddAppender(CreateDefaultLogAppender());
            }
            ConfigureConsoleLogging(repository, (Logger)Console.Logger);
        }

        private static Logger GetRootLogger(ILoggerRepository repository)
        {
            return ((Hierarchy)repository).Root;
        }

        private void ConfigureConsoleLogging(ILoggerRepository repository, Logger consoleLogger)
        {
            consoleLogger.Additivity = false;
            SetLoggerLevel(consoleLogger, consoleLogLevel);
            AddAppenderIfMissing(consoleLogger, ConfigureStdErr(repository));
        }

        private IAppender ConfigureStdErr(ILoggerRepository repository)
        {
            return GetOrCreateConsoleAppender(repository, "Console.STDERR",
                new LevelRangeFilter { AcceptOnMatch = false },
                outputDescriptor.CreateStdErr);
        }
        
        private IAppender CreateDefaultLogAppender()
        {
            var appender = outputDescriptor.CreateDefaultLog();
            appender.Name = "DefaultLogAppender";
            return Init(appender);
        }

        private static IAppender GetOrCreateConsoleAppender(ILoggerRepository repository, string name,
            LevelRangeFilter filter,
            Func<IAppender> defaultAppender)
        {
            var appender = repository.GetAppenders().FirstOrDefault(a => a.Name == name);
            
            if (appender == null)
            {
                appender = defaultAppender();
                appender.Name = name;
            }
            AddFilterIfPossible(appender, filter);
            Init(appender);
            return appender;
        }

        private static void AddFilterIfPossible<T>(IAppender appender, T filter) where T : IFilter
        {
            if (appender is AppenderSkeleton)
            {
                var filterable = (AppenderSkeleton)appender;
                if (!EnumerateFilters(filterable.FilterHead).OfType<T>().Any())
                {
                    filterable.AddFilter(Init(filter));
                }
            }
        }


        private static void AddAppenderIfMissing(Logger logger, IAppender appender)
        {
            Init(appender);
            if (logger.Appenders.Contains(appender)) return;
            logger.AddAppender(appender);
        }


        private static IEnumerable<IFilter> EnumerateFilters(IFilter filter)
        {
            while (filter != null)
            {
                yield return filter;
                filter = filter.Next;
            }
        }

        private static T Init<T>(T obj)
        {
            if(obj is IOptionHandler) ((IOptionHandler)obj).ActivateOptions();
            return obj;
        }

        public void Dispose()
        {
            var repository = LogManager.GetRepository();
            repository.ConfigurationChanged -= OnConfigurationChanged;
        }
    }
}