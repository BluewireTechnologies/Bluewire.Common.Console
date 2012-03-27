using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Options;

namespace Bluewire.Common.Console
{
    public class SessionArguments<T>
    {
        public SessionArguments(T arguments, OptionSet options)
        {
            Arguments = arguments;
            Options = options;
            Application = Assembly.GetEntryAssembly().Location;
            
            AddStandardOptions();
        }

        private void AddStandardOptions()
        {
            ForArgumentsInterface<IVerbosityArgument>(a => Options.Add("v|verbose", "Verbose mode.", v => a.Verbose()));
        }

        public T Arguments { get; private set; }
        public OptionSet Options { get; private set; }
        private bool hasParsed;

        public bool HasArgumentsInterface<TInterface>()
        {
            return this.Arguments is TInterface;
        }

        public bool ForArgumentsInterface<TInterface>(Action<TInterface> action)
        {
            if (HasArgumentsInterface<TInterface>())
            {
                action((TInterface)(object)this.Arguments);
                return true;
            }
            return false;
        }

        public void Parse(string[] args)
        {
            if (hasParsed) throw new NotSupportedException("SessionArguments#Parse may only be called once.");
            hasParsed = true;

            try
            {
                var definitelyNotOptions = args.SkipWhile(a => a != "--");

                var spareArguments = Options.Parse(args.TakeWhile(a => a != "--")).ToArray();

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


        public string Application { get; set; }

    }
}