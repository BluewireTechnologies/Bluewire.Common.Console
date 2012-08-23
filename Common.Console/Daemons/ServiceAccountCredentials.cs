using System;
using System.Linq;
using System.Net;
using System.ServiceProcess;

namespace Bluewire.Common.Console.Daemons
{
    public struct ServiceAccountCredentials
    {
        public ServiceAccountCredentials(string userName, string password, string domain = null) : this()
        {
            if (userName == null) throw new ArgumentNullException("userName");
            if (password == null) throw new ArgumentNullException("password");

            ServiceAccount = ServiceAccount.User;
            Credentials = new NetworkCredential(userName, password, domain);
        }


        public ServiceAccountCredentials(ServiceAccount serviceAccount) : this()
        {
            if (serviceAccount == ServiceAccount.User) throw new ArgumentException("User account requires name and password.");
            ServiceAccount = serviceAccount;
        }

        /// <summary>
        /// Account type. Default is LocalService (enum value 0).
        /// </summary>
        public ServiceAccount ServiceAccount { get; private set; }
        /// <summary>
        /// Credentials, if account type is User.
        /// </summary>
        public NetworkCredential Credentials { get; private set; }

        public void Apply(ServiceProcessInstaller installer)
        {
            installer.Account = ServiceAccount;
            if (Credentials != null)
            {
                var userNameForService = Credentials.Domain == null ? Credentials.UserName : String.Format(@"{0}\{1}", Credentials.Domain, Credentials.UserName);
                installer.Username = userNameForService;
                installer.Password = Credentials.Password;
            }
        }
    }
}