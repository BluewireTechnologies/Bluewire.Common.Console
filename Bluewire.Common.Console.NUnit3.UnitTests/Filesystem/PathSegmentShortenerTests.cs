using Bluewire.Common.Console.NUnit3.Filesystem;
using NUnit.Framework;

namespace Bluewire.Common.Console.NUnit3.UnitTests.Filesystem
{
    [TestFixture]
    public class PathSegmentShortenerTests
    {
        private readonly PathSegmentShortener sut = new PathSegmentShortener();

        [Test]
        public void DoesNotShortenSegmentContainingOnlySingleCharacterParts()
        {
            var shortened = sut.GetShorteningResult("A.B.C.D", 6);
            Assert.That(shortened, Is.EqualTo(new PathSegmentShortener.Result { OriginalSegment = "A.B.C.D" }));
        }

        [Test]
        public void DoesNotShortenSegmentShorterThanLimit()
        {
            var shortened = sut.GetShorteningResult("First.Second.Third", 20);
            Assert.That(shortened, Is.EqualTo(new PathSegmentShortener.Result { OriginalSegment = "First.Second.Third" }));
        }

        [TestCase("F.S.T", "First.Second.Third", 9)]
        [TestCase("F.S.T", "FirstPartNameIsLonger.Second.Third", 9)]
        [TestCase("Firs.Seco.Thir", "FirstPartName.SecondPartName.ThirdPartName", 20)]
        [TestCase("V.S.T", "VeryVeryVeryVeryVeryVeryVeryVeryLongSegmentName.Second.Third", 9)]
        public void ShorteningIsSpreadAcrossAllParts(string expected, string original, int limit)
        {
            var shortened = sut.GetShorteningResult(original, limit);
            Assert.That(shortened.ShortenedSegment, Is.EqualTo(expected));
        }
    }
}
