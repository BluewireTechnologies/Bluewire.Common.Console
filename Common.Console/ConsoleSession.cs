using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Options;

namespace Bluewire.Common.Console
{
    public class ConsoleSession<T>
    {
        public string Application { get; set; }

        private string GetUsageString()
        {
            var usage = String.Format("{0} <options>", Path.GetFileName(Application));
            ForArgumentsInterface<IArgumentList>(a => Usage += " " + ListParameterUsage);
            return usage;
        }

        private string usage;
        public string Usage
        {
            get { return this.usage ?? GetUsageString(); }
            set { this.usage = value; }
        }

        public string ListParameterUsage { get; set; }

        public ConsoleSession(T arguments, OptionSet options)
        {
            this.arguments = arguments;
            sessionArguments = new SessionArguments();
            this.options = options;
            
            Application = Assembly.GetEntryAssembly().Location;
            ListParameterUsage = "<file names ...>";
            
            AddStandardOptions();
        }

        public int Run(string[] args, Func<T, int> application)
        {
            if (hasRun) throw new NotSupportedException("ConsoleSession<T>#Run() may only be called once.");
            hasRun = true;

            try
            {
                ParseArguments(args);
                if (OnBeforeRun())
                {
                    return application(this.arguments);
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

        private void AddStandardOptions()
        {
            ForArgumentsInterface<IVerbosityArgument>(a => options.Add("v|verbose", "Verbose mode.", v => a.Verbose()));

            options.Add("pause", "When finished, wait for the user to press <Enter> before terminating.", v => sessionArguments.PauseWhenDone = true);
            options.Add("h|?|help", "Show usage.", v => sessionArguments.ShowUsage = true);
        }





        private readonly T arguments;
        private readonly OptionSet options;

        private bool HasArgumentsInterface<TInterface>()
        {
            return this.arguments is TInterface;
        }

        private bool ForArgumentsInterface<TInterface>(Action<TInterface> action)
        {
            if (HasArgumentsInterface<TInterface>())
            {
                action((TInterface)(object)this.arguments);
                return true;
            }
            return false;
        }

        private bool hasRun;
        private SessionArguments sessionArguments;

        private bool OnBeforeRun()
        {
            if (sessionArguments.ShowUsage)
            {
                ShowUsage();
                return false;
            }
            return true;
        }

        private void OnAfterRun()
        {
            if (sessionArguments.PauseWhenDone)
            {
                System.Console.Error.WriteLine("Press Enter to continue...");
                System.Console.In.ReadLine();
            }
        }

        private void OnHandledException(ErrorWithReturnCodeException ex)
        {
            System.Console.Error.WriteLine(ex.Message);
            if (ex.ShowUsage) ShowUsage();
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

        private void ShowUsage()
        {
            System.Console.Error.WriteLine("Usage: {0}", Usage);
            this.options.WriteOptionDescriptions(System.Console.Error);
        }

        private void ParseArguments(string[] args)
        {
            try
            {
                var definitelyNotOptions = args.SkipWhile(a => a != "--");

                var spareArguments = this.options.Parse(args.TakeWhile(a => a != "--")).ToArray();

                var possiblyUnprocessedOptions = spareArguments.Where(a => a.StartsWith("-")).ToArray();
                if (possiblyUnprocessedOptions.Any())
                {
                    throw new InvalidArgumentsException("Unrecognised option(s): {0}", String.Join(", ", possiblyUnprocessedOptions));
                }

                ForArgumentsInterface<IArgumentList>(a => { foreach (var s in spareArguments.Concat(definitelyNotOptions)) a.ArgumentList.Add(s); });
            }
            catch (OptionException ex)
            {
                throw new InvalidArgumentsException(ex);
            }
        }

        class SessionArguments
        {
            public bool ShowUsage { get; set; }
            public bool PauseWhenDone { get; set; }
        }
    }
}