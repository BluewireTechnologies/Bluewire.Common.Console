using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console
{
    public class NoArguments : SessionArguments<object>
    {
        public NoArguments()
            : base(new object(), new OptionSet())
        {
        }
    }
}