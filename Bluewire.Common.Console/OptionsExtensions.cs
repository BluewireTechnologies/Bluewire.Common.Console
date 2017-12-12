using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console
{
    public static class OptionsExtensions
    {
        public static T AddCollector<T>(this OptionSet options, T collector) where T : IReceiveOptions
        {
            collector?.ReceiveFrom(options);
            return collector;
        }
    }
}
