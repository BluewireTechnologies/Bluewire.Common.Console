using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console
{
    public interface IReceiveOptions
    {
        /// <summary>
        /// Add argument-parsing information to an Options object.
        /// </summary>
        void ReceiveFrom(OptionSet options);
    }
}
