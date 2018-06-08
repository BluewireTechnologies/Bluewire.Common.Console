using System.Collections.Generic;
using Bluewire.Common.Console.Arguments;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Arguments
{
    [TestFixture]
    public class ArgumentListTests
    {
        [Test]
        public void ParsesPositionalArguments()
        {
            string first = null, second = null;
            var argumentList = new ArgumentList()
            {
                { "First", s => first = s },
                { "Second", s => second = s }
            };

            argumentList.Parse(new[] { "One", "Two" });

            Assert.That(first, Is.EqualTo("One"));
            Assert.That(second, Is.EqualTo("Two"));
        }

        [Test]
        public void ParsesRemainingPositionalArguments()
        {
            string first = null;
            var remaining = new List<string>();
            var argumentList = new ArgumentList()
            {
                { "First", s => first = s }
            }.AddRemainder("Remaining...", remaining.Add);

            argumentList.Parse(new[] { "One", "Two", "Three" });

            Assert.That(first, Is.EqualTo("One"));
            Assert.That(remaining, Is.EqualTo(new object[] { "Two", "Three" }));
        }
    }
}
