using Bluewire.Common.Console.Arguments;

namespace Bluewire.Common.Console
{
    public interface IReceiveArgumentList
    {
        /// <summary>
        /// Add argument-parsing information to an ArgumentList object.
        /// </summary>
        void ReceiveFrom(ArgumentList argumentList);
    }
}
