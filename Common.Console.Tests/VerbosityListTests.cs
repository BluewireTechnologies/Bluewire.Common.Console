using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Core;
using NUnit.Framework;

namespace Bluewire.Common.Console.Tests
{
    [TestFixture]
    public class VerbosityListTests
    {
        [Test]
        public void DefaultVerbosityIsTheFirstEntry()
        {
            var list = new VerbosityList<Level>(Level.Warn, Level.Info, Level.Debug);
            Assert.AreEqual(Level.Warn, list.CurrentVerbosity);
        }

        [Test]
        public void DefaultVerbosityCanBeOverridden()
        {
            var list = new VerbosityList<Level>(Level.Warn, Level.Info, Level.Debug).Default(Level.Debug);
            Assert.AreEqual(Level.Debug, list.CurrentVerbosity);
        }

        [Test]
        public void VerbosityLevelsDoNotNeedToBeUnique()
        {
            new VerbosityList<Level>(Level.Warn, Level.Info, Level.Warn, Level.Debug);
        }

        [Test]
        public void QuietSelectsAnEarlierLevelInTheList()
        {
            var list = new VerbosityList<Level>(Level.Warn, Level.Info, Level.Debug).Default(Level.Info);
            list.Quiet();
            Assert.AreEqual(Level.Warn, list.CurrentVerbosity);
        }

        [Test]
        public void VerboseSelectsALaterLevelInTheList()
        {
            var list = new VerbosityList<Level>(Level.Warn, Level.Info, Level.Debug).Default(Level.Info);
            list.Verbose();
            Assert.AreEqual(Level.Debug, list.CurrentVerbosity);
        }

        [Test]
        public void OverridingDefaultVerbosity_SelectsEarliestOccurrenceOfTheSpecifiedLevel()
        {
            var list = new VerbosityList<Level>(Level.Warn, Level.Info, Level.Warn, Level.Debug);
            list.Default(Level.Warn);
            list.Verbose();
            Assert.AreEqual(Level.Info, list.CurrentVerbosity);
        }

        [Test]
        public void RequestingQuieterThanQuietestLevel_SelectsQuietestLevel()
        {
            var list = new VerbosityList<Level>(Level.Warn, Level.Info, Level.Debug).Default(Level.Info);
            list.Quiet();
            list.Quiet();
            list.Quiet();
            Assert.AreEqual(Level.Warn, list.CurrentVerbosity);
        }

        [Test]
        public void RequestingMoreVerboseThanMostVerboseLevel_SelectsMostVerboseLevel()
        {
            var list = new VerbosityList<Level>(Level.Warn, Level.Info, Level.Debug).Default(Level.Info);
            list.Verbose();
            list.Verbose();
            list.Verbose();
            Assert.AreEqual(Level.Debug, list.CurrentVerbosity);
        }
    }
}
