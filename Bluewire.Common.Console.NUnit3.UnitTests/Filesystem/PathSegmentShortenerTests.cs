using Bluewire.Common.Console.NUnit3.Filesystem;
using NUnit.Framework;

namespace Bluewire.Common.Console.NUnit3.UnitTests.Filesystem
{
    [TestFixture]
    public class PathSegmentShortenerTests
    {
        [Test]
        public void DoesNotShortenSegmentContainingOnlySingleCharacterParts()
        {
            var shortened = new PathSegmentShortener().AggressivelyShortenDottedSegment("A.B.C.D", 6);
            Assert.That(shortened, Is.EqualTo("A.B.C.D"));
        }

        [Test]
        public void DoesNotShortenSegmentShorterThanLimit()
        {
            var shortened = new PathSegmentShortener().AggressivelyShortenDottedSegment("First.Second.Third", 20);
            Assert.That(shortened, Is.EqualTo("First.Second.Third"));
        }

        [TestCase("F.S.TC8A3", "First.Second.Third", 9)]
        [TestCase("F.S.TBFB8", "FirstPartNameIsLonger.Second.Third", 9)]
        [TestCase("Firs.Seco.Thir7064", "FirstPartName.SecondPartName.ThirdPartName", 20)]
        [TestCase("V.S.T3922", "VeryVeryVeryVeryVeryVeryVeryVeryLongSegmentName.Second.Third", 9)]
        public void ShorteningIsSpreadAcrossAllParts(string expected, string original, int limit)
        {
            var shortened = new PathSegmentShortener().AggressivelyShortenDottedSegment(original, limit);
            Assert.That(shortened, Is.EqualTo(expected));
        }
    }
}
