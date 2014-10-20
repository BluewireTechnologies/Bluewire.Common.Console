using System;
using System.IO;
using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console
{
    
    public class ConsoleSession<T>
    {
        private ConsoleArguments consoleArguments;
        private SessionArguments<T> session;

        public ConsoleSession(T arguments, OptionSet options) : this(new SessionArguments<T>(arguments, options))
        {
        }

        public ConsoleSession(SessionArguments<T> session)
        {
            ListParameterUsage = "<file names ...>";
            this.session = session;
            consoleArguments = new ConsoleArguments();
            AddConsoleOptions();
        }

        public int Run(string[] args, Func<T, int> application)
        {
            try
            {
                session.Parse(args);
                if (OnBeforeRun())
                {
                    return application(this.session.Arguments);
                }
                return 0;
            }
            catch (ErrorWithReturnCodeException ex)
            {
                OnHandledException(ex);
                return ex.ExitCode;
            }
            catch (Exception ex)
            {
                OnUnhandledException(ex);
                return 255;
            }
            finally
            {
                OnAfterRun();
            }
        }


        private string GetUsageString()
        {
            var usage = String.Format("{0} <options>", Path.GetFileName(session.Application));
            session.ForArgumentsInterface<IArgumentList>(a => usage += " " + ListParameterUsage);
            return usage;
        }

        private string customUsageString;
        /// <summary>
        /// Displayed first, above the options descriptions.
        /// </summary>
        public string Usage
        {
            get { return this.customUsageString ?? GetUsageString(); }
            set { this.customUsageString = value; }
        }

        /// <summary>
        /// If the application accepts an arbitrary parameter list, this will be appended to the
        /// default usage string.
        /// </summary>
        public string ListParameterUsage { get; set; }

        /// <summary>
        /// This is displayed below the option descriptions.
        /// </summary>
        public string ExtendedUsageDetails { get; set; }

        private void AddConsoleOptions()
        {
            session.Options.Add("pause", "When finished, wait for the user to press <Enter> before terminating.", v => consoleArguments.PauseWhenDone = true);
            session.Options.Add("h|?|help", "Show usage.", v => consoleArguments.ShowUsage = true);
        }

        private bool OnBeforeRun()
        {
            if (consoleArguments.ShowUsage)
            {
                ShowUsage();
                return false;
            }
            return true;
        }

        private void OnAfterRun()
        {
            if (consoleArguments.PauseWhenDone)
            {
                System.Console.Error.WriteLine("Press Enter to continue...");
                System.Console.In.ReadLine();
            }
        }

        private void OnHandledException(ErrorWithReturnCodeException ex)
        {
            System.Console.Error.WriteLine(ex.Message);
            if (ex.ShowUsage) ShowUsageSummary();
        }

        private void OnUnhandledException(Exception ex)
        {
            System.Console.Error.WriteLine("Unhandled exception occurred:");
            WriteExceptionStack(ex);
        }

        private void WriteExceptionStack(Exception ex)
        {
            if(ex.InnerException != null) WriteExceptionStack(ex.InnerException);
            System.Console.Error.WriteLine(ex.Message);
            System.Console.Error.WriteLine(ex.StackTrace);
        }

        private void ShowUsageSummary()
        {
            System.Console.Error.WriteLine("Usage: {0}", Usage);
            System.Console.Error.WriteLine("For more information, try specifying -?");
        }

        private void ShowUsage()
        {
            System.Console.Error.WriteLine("Usage: {0}", Usage);
            this.session.Options.WriteOptionDescriptions(System.Console.Error);
            if (!String.IsNullOrWhiteSpace(ExtendedUsageDetails))
            {
                System.Console.Error.WriteLine(ExtendedUsageDetails);
            }
        }

        public class ConsoleArguments
        {
            public bool ShowUsage { get; set; }
            public bool PauseWhenDone { get; set; }
        }
    }
}