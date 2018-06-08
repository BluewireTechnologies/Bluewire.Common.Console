using System;
using System.Collections.Generic;
using System.Linq;
using Bluewire.Common.Console.ThirdParty;
using log4net.Core;

namespace Bluewire.Common.Console
{
    public class VerbosityList : VerbosityList<Level>
    {
        public VerbosityList() : base(Level.Fatal, Level.Error, Level.Warn, Level.Info, Level.Debug)
        {
            Default = Level.Warn;
        }
    }

    /// <summary>
    /// Generic implementation of an ordered set of log verbosity levels.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VerbosityList<T> : IEnumerable<T>, IReceiveOptions
    {
        private readonly List<T> levels;

        public VerbosityList(params T[] levelsInOrderOfIncreasingVerbosity)
        {
            this.levels = levelsInOrderOfIncreasingVerbosity.ToList();
            defaultLogLevel = logLevel = 0;
        }

        private int defaultLogLevel;
        private int logLevel;

        /// <summary>
        /// Increase verbosity by one level.
        /// </summary>
        public void Verbose()
        {
            logLevel++;
        }

        /// <summary>
        /// Decrease verbosity by one level.
        /// </summary>
        public void Quiet()
        {
            logLevel--;
        }

        /// <summary>
        /// True if the selected log level is more verbose than the default.
        /// </summary>
        public bool IsVerbose => logLevel > defaultLogLevel;

        /// <summary>
        /// True if the selected log level is less verbose than the default.
        /// </summary>
        public bool IsQuiet => logLevel < defaultLogLevel;

        /// <summary>
        /// Set the default verbosity level.
        /// </summary>
        /// <remarks>
        /// Unless specified using this method, the default verbosity level of the list
        /// will be the first one, ie. the minimum.
        /// If a level appears more than once in the list, this method will pick the first
        /// occurrence.
        /// </remarks>
        public T Default
        {
            get => GetLevelByIndex(defaultLogLevel);
            set
            {
                if (!levels.Contains(value)) throw new ArgumentException($"Default level {value} is not in the list of available verbosity levels.");
                // If a level appears multiple times, err on the quiet side.
                // Should not really have duplicates, though.
                defaultLogLevel = logLevel = levels.IndexOf(value);
            }
        }

        /// <summary>
        /// Currently selected verbosity level.
        /// </summary>
        public T CurrentVerbosity
        {
            get
            {
                var currentLogLevel = logLevel;
                return GetLevelByIndex(currentLogLevel);
            }
        }

        private T GetLevelByIndex(int currentLogLevel)
        {
            if (currentLogLevel < 0) currentLogLevel = 0;
            if (currentLogLevel >= levels.Count) currentLogLevel = levels.Count - 1;
            return levels[currentLogLevel];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return levels.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IReceiveOptions.ReceiveFrom(OptionSet options)
        {
            options.Add("v|verbose", "Verbose console logging.", v => Verbose());
            options.Add("q|quiet", "Quiet mode: reduce console logging.", v => Quiet());
        }
    }
}
