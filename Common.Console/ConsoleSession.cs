using System;
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
        }

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
                var spareArguments = Options.Parse(args).ToArray();

                ForArgumentsInterface<IFileNameListArgument>(a => { foreach (var s in spareArguments) a.FileNames.Add(s); });

                try
                {
                    return application(Arguments);
                }
                catch (ErrorWithReturnCodeException ex)
                {
                    System.Console.Error.WriteLine(ex.Message);
                    if (ex.ShowUsage)
                    {
                        Options.WriteOptionDescriptions(System.Console.Error);
                    }
                    return ex.ExitCode;
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Unhandled exception occurred:");
                System.Console.Error.WriteLine(ex.Message);
                System.Console.Error.WriteLine(ex.StackTrace);

                return 255;
            }
        }
    }
}