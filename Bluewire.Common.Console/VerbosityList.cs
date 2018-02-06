using System;
using System.Collections.Generic;
using System.Linq;

namespace Bluewire.Common.Console
{
    /// <summary>
    /// Generic implementation of an ordered set of log verbosity levels.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VerbosityList<T> : IEnumerable<T>
    {
        private readonly List<T> levels;

        public VerbosityList(params T[] levelsInOrderOfIncreasingVerbosity)
        {
            this.levels = levelsInOrderOfIncreasingVerbosity.ToList();
            logLevel = 0;
        }

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
        /// Set the default verbosity level.
        /// </summary>
        /// <remarks>
        /// Unless specified using this method, the default verbosity level of the list
        /// will be the first one, ie. the minimum.
        /// If a level appears more than once in the list, this method will pick the first
        /// occurrence.
        /// </remarks>
        /// <param name="defaultLevel"></param>
        /// <returns></returns>
        public VerbosityList<T> Default(T defaultLevel)
        {
            if (!levels.Contains(defaultLevel)) throw new ArgumentException(String.Format("Default level {0} is not in the list of available verbosity levels.", defaultLevel));
            // If a level appears multiple times, err on the quiet side.
            // Should not really have duplicates, though.
            logLevel = levels.IndexOf(defaultLevel);
            return this;
        }

        /// <summary>
        /// Currently selected verbosity level.
        /// </summary>
        public T CurrentVerbosity
        {
            get
            {
                var currentLogLevel = logLevel;
                if (currentLogLevel < 0) currentLogLevel = 0;
                if (currentLogLevel >= levels.Count) currentLogLevel = levels.Count - 1;
                return levels[currentLogLevel];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return levels.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
