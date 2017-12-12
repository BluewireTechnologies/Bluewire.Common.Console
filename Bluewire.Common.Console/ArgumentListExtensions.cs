using Bluewire.Common.Console.Arguments;

namespace Bluewire.Common.Console
{
    public static class ArgumentListExtensions
    {
        public static T AddCollector<T>(this ArgumentList argumentList, T collector) where T : IReceiveArgumentList
        {
            collector?.ReceiveFrom(argumentList);
            return collector;
        }
    }
}
