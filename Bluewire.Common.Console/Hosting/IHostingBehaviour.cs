using System;

namespace Bluewire.Common.Console.Hosting
{
    public interface IHostingBehaviour
    {
        AppDomainSetup CreateAppDomainSetup();
        void OnBeforeStart();
        void OnAfterStop();
    }
}