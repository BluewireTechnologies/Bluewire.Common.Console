using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bluewire.Common.Console.Progress;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;

namespace Bluewire.Common.Console.Tests.Progress
{
    [TestFixture]
    public class FormattingHelpersTests
    {
        [Test]
        [Row(1, 1.0924334, 1)]
        [Row(1.09, 1.0924334, 3)]
        [Row(1.1, 1.0924334, 2)]
        [Row(1100, 1092.4334, 2)]
        [Row(1092, 1092.4334, 4)]
        public void CanRoundToSignificantFigures(double expected, double input, int sigFig)
        {
            Assert.AreApproximatelyEqual(expected, input.RoundToSignificantFigures(sigFig), input / 1000000);
        }


        [Test]
        [Row("00:00:05", 5, 1)]
        [Row("2500ms", 5, 2)]
        [Row("1200ms", 6, 5)]
        [Row("857ms", 6, 7)]
        [Row("667ms", 4, 6)]
        [Row("0.737ms", 1, 1357)]
        public void TestPrettyPrintingItemDuration(string expected, double seconds, long itemCount)
        {
            Assert.AreEqual(expected, FormattingHelpers.PrettyPrintItemDuration(TimeSpan.FromSeconds(seconds), itemCount));
        }

        [Test]
        [Row("< 1", 5, 1)]
        [Row("< 1", 5, 2)]
        [Row("~ 1", 6, 5)]
        [Row("~ 1", 6, 7)]
        [Row("1.5", 4, 6)]
        [Row("1360", 1, 1357)]
        public void TestPrettyPrintingItemsPerSecond(string expected, double seconds, long itemCount)
        {
            Assert.AreEqual(expected, FormattingHelpers.PrettyPrintItemsPerSecond(TimeSpan.FromSeconds(seconds), itemCount));
        }
    }
}
