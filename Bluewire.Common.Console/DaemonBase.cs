using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace Bluewire.Common.Console
{
    public class DaemonBase : IDaemon
    {
        private readonly Stack<IDisposable> disposables = new Stack<IDisposable>();
        public T Track<T>(T instance)
        {
            if (instance is IDisposable disposable) disposables.Push(disposable);
            return instance;
        }

        protected void CleanUpTrackedInstances()
        {
            while (disposables.Any())
            {
                var disposable = disposables.Pop();
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger(GetType()).Error(ex);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanUpTrackedInstances();
            }
        }

        ~DaemonBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
