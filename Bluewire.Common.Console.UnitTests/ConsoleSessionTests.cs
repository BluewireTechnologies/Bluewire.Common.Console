using System.Threading.Tasks;
using Bluewire.Common.Console.ThirdParty;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests
{
    [TestFixture]
    public class ConsoleSessionTests
    {
        [Test]
        public void CanMarshalAsyncSessionBackToMain()
        {
            var session = new ConsoleSession<NoArguments>(new NoArguments(), new OptionSet());
            var exitCode = session.Run(new string[0], async a => await DoSomethingAsyncAndReturn(42));

            Assert.That(exitCode, Is.EqualTo(42));
        }

        public async Task<int> DoSomethingAsyncAndReturn(int exitCode)
        {
            await Task.Delay(50);
            return await Task.Run(() => exitCode);
        }
    }
}
