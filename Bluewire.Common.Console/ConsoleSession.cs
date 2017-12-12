using System;
using System.IO;
using System.Threading.Tasks;
using Bluewire.Common.Console.Arguments;
using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console
{
    public class ConsoleSession : SessionArguments
    {
        private readonly ConsoleArguments consoleArguments;

        public ConsoleSession()
        {
            consoleArguments = Options.AddCollector(new ConsoleArguments());
        }

        public int Run(string[] args, Func<int> application)
        {
            try
            {
                Parse(args);
                if (OnBeforeRun())
                {
                    return application();
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

        public int Run(string[] args, Func<Task<int>> application)
        {
            return RunAsync(args, application).GetAwaiter().GetResult();
        }

        public async Task<int> RunAsync(string[] args, Func<Task<int>> application)
        {
            try
            {
                Parse(args);
                if (OnBeforeRun())
                {
                    return await application();
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
            return $"{Path.GetFileName(Application)} <options> {ListParameterUsage ?? ArgumentList.GetArgumentDescriptions()}".Trim();
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
            if (ex.InnerException != null) WriteExceptionStack(ex.InnerException);
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
            Options.WriteOptionDescriptions(System.Console.Error);
            if (!String.IsNullOrWhiteSpace(ExtendedUsageDetails))
            {
                System.Console.Error.WriteLine(ExtendedUsageDetails);
            }
        }

        public class ConsoleArguments : IReceiveOptions
        {
            public bool ShowUsage { get; set; }
            public bool PauseWhenDone { get; set; }

            void IReceiveOptions.ReceiveFrom(OptionSet options)
            {
                options.Add("pause", "When finished, wait for the user to press <Enter> before terminating.", v => PauseWhenDone = true);
                options.Add("h|?|help", "Show usage.", v => ShowUsage = true);
            }
        }
    }
}
