using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Options;

namespace Bluewire.Common.Console
{
    public class ConsoleSession<T>
    {
        public ConsoleSession(T arguments, OptionSet options)
        {
            Arguments = arguments;
            sessionArguments = new SessionArguments();
            Options = options;
            ForArgumentsInterface<IVerbosityArgument>(a => Options.Add("v|verbose", "Verbose mode.", v => a.Verbose()));
            

            Application = Assembly.GetEntryAssembly().Location;

            Usage = String.Format("{0} <options>", Path.GetFileName(Application));
            ForArgumentsInterface<IFileNameListArgument>(a => Usage += " <file names ...>");

            Options.Add("pause", "When finished, wait for the user to press <Enter> before terminating.", v => sessionArguments.PauseWhenDone = true);
            Options.Add("h|?|help", "Show usage.", v => sessionArguments.ShowUsage = true);
        }

        class SessionArguments
        {
            public bool ShowUsage { get; set; }
            public bool PauseWhenDone { get; set; }
        }

        public string Application { get; set; }

        public string Usage { get; set; }

        protected T Arguments { get; private set; }

        protected OptionSet Options { get; private set; }

        protected bool HasArgumentsInterface<TInterface>()
        {
            return Arguments is TInterface;
        }

        protected bool ForArgumentsInterface<TInterface>(Action<TInterface> action)
        {
            if (HasArgumentsInterface<TInterface>())
            {
                action((TInterface)(object)Arguments);
                return true;
            }
            return false;
        }

        private bool hasRun;
        private SessionArguments sessionArguments;

        public int Run(string[] args, Func<T, int> application)
        {
            if (hasRun)
            {
                throw new NotSupportedException("ConsoleSession<T>#Run() may only be called once.");
            }
            hasRun = true;

            try
            {
                ParseArguments(args);
                if (sessionArguments.ShowUsage)
                {
                    ShowUsage();
                    return 0;
                }
                return application(Arguments);
            }
            catch (ErrorWithReturnCodeException ex)
            {
                System.Console.Error.WriteLine(ex.Message);
                if (ex.ShowUsage)
                {
                    ShowUsage();
                }
                return ex.ExitCode;
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Unhandled exception occurred:");
                System.Console.Error.WriteLine(ex.Message);
                System.Console.Error.WriteLine(ex.StackTrace);

                return 255;
            }
            finally
            {
                if (sessionArguments.PauseWhenDone)
                {
                    System.Console.Error.WriteLine("Press Enter to continue...");
                    System.Console.In.ReadLine();
                }
            }
        }

        private void ShowUsage()
        {
            System.Console.Error.WriteLine("Usage: {0}", Usage);
            Options.WriteOptionDescriptions(System.Console.Error);
        }

        private void ParseArguments(string[] args)
        {
            try
            {
                var definitelyNotOptions = args.SkipWhile(a => a != "--");

                var spareArguments = Options.Parse(args).ToArray();

                var possiblyUnprocessedOptions = spareArguments.Except(definitelyNotOptions).Where(a => a.StartsWith("-")).ToArray();
                if (possiblyUnprocessedOptions.Any())
                {
                    throw new InvalidArgumentsException("Unrecognised option(s): {0}", String.Join(", ", possiblyUnprocessedOptions));
                }

                ForArgumentsInterface<IFileNameListArgument>(a => { foreach (var s in spareArguments) a.FileNames.Add(s); });
            }
            catch (OptionException ex)
            {
                throw new InvalidArgumentsException(ex);
            }
        }
    }
}