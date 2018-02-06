using System;

namespace Bluewire.Common.Console.Hosting
{
    public class DefaultHostingBehaviour : IHostingBehaviour
    {
        private readonly AppDomainSetup setup;

        public DefaultHostingBehaviour(AppDomainSetup setup)
        {
            this.setup = setup;
        }

        public AppDomainSetup CreateAppDomainSetup()
        {
            return setup;
        }

        public void OnBeforeStart()
        {
        }

        public void OnAfterStop()
        {
        }
    }
}
