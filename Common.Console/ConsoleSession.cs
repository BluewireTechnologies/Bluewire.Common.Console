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
            Options = options;
            ForArgumentsInterface<IVerbosityArgument>(a => Options.Add("v|verbose", "Verbose mode.", v => a.Verbose()));

            Application = Assembly.GetEntryAssembly().Location;

            Usage = String.Format("{0} <options>", Path.GetFileName(Application));
            ForArgumentsInterface<IFileNameListArgument>(a => Usage += " <file names ...>");
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

        public int Run(string[] args, Func<T, int> application)
        {
            try
            {
                ParseArguments(args);
                return application(Arguments);
            }
            catch (ErrorWithReturnCodeException ex)
            {
                System.Console.Error.WriteLine(ex.Message);
                if (ex.ShowUsage)
                {
                    System.Console.Error.WriteLine("Usage: {0}", Usage);
                    Options.WriteOptionDescriptions(System.Console.Error);
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