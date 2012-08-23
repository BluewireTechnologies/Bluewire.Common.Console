using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace Bluewire.Common.Console.Daemons
{
    public class ServiceAccountCredentialsFactory
    {
        public ServiceAccountCredentials? Create(string userName, string password)
        {
            if (userName == null) return null;
            if (password == null)
            {
                switch (userName.ToLowerInvariant())
                {
                    case "system":
                    case "local system":
                        return new ServiceAccountCredentials(ServiceAccount.LocalSystem);
                    case "local service":
                        return new ServiceAccountCredentials(ServiceAccount.LocalService);
                    case "network service":
                        return new ServiceAccountCredentials(ServiceAccount.NetworkService);
                }
            }

            string domain = null;
            var user = userName;
            // not really sure how we're supposed to handle multiple backslashes. split on the first one for now.
            var split = userName.IndexOf('\\');
            if (split >= 0)
            {
                domain = userName.Substring(0, split);
                user = userName.Substring(split + 1);
                // if we have multiple backslashes, the remainder will show up in the user name.
                if (user.Contains("\\")) throw new ArgumentException(String.Format("Specified user name '{0}' was not valid: too many '\\'", userName));
            }
            
            return new ServiceAccountCredentials(user, password, domain);
        }
    }
}