using System;

namespace Bluewire.Common.Console.Util
{
    internal static class Disposable
    {
        public static IDisposable Empty { get; } = new EmptyDisposable();

        class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
