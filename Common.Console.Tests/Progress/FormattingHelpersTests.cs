using System;
using Bluewire.Common.Console.Progress;
using NUnit.Framework;

namespace Bluewire.Common.Console.Tests.Progress
{
    [TestFixture]
    public class FormattingHelpersTests
    {
        [TestCase(1, 1.0924334, 1)]
        [TestCase(1.09, 1.0924334, 3)]
        [TestCase(1.1, 1.0924334, 2)]
        [TestCase(1100, 1092.4334, 2)]
        [TestCase(1092, 1092.4334, 4)]
        public void CanRoundToSignificantFigures(double expected, double input, int sigFig)
        {
            Assert.That(input.RoundToSignificantFigures(sigFig), Is.EqualTo(expected).Within(input / 1000000));
        }


        [TestCase("00:00:05", 5, 1)]
        [TestCase("2500ms", 5, 2)]
        [TestCase("1200ms", 6, 5)]
        [TestCase("857ms", 6, 7)]
        [TestCase("667ms", 4, 6)]
        [TestCase("0.737ms", 1, 1357)]
        public void TestPrettyPrintingItemDuration(string expected, double seconds, long itemCount)
        {
            Assert.AreEqual(expected, FormattingHelpers.PrettyPrintItemDuration(TimeSpan.FromSeconds(seconds), itemCount));
        }

        [TestCase("< 1", 5, 1)]
        [TestCase("< 1", 5, 2)]
        [TestCase("~ 1", 6, 5)]
        [TestCase("~ 1", 6, 7)]
        [TestCase("1.5", 4, 6)]
        [TestCase("1360", 1, 1357)]
        public void TestPrettyPrintingItemsPerSecond(string expected, double seconds, long itemCount)
        {
            Assert.AreEqual(expected, FormattingHelpers.PrettyPrintItemsPerSecond(TimeSpan.FromSeconds(seconds), itemCount));
        }
    }
}
