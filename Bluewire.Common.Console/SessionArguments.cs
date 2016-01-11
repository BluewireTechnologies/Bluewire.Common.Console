using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console
{
    public class SessionArguments
    {
        public SessionArguments(OptionSet options)
        {
            Options = options;
        }

        public OptionSet Options { get; private set; }
        
        protected string[] Parse(string[] args)
        {
            try
            {
                var definitelyNotOptions = args.SkipWhile(a => a != "--");

                var spareArguments = Options.Parse(args.TakeWhile(a => a != "--")).ToArray();

                var possiblyUnprocessedOptions = spareArguments.Where(a => a.StartsWith("-")).ToArray();
                if (possiblyUnprocessedOptions.Any())
                {
                    throw new InvalidArgumentsException("Unrecognised option(s): {0}", String.Join(", ", possiblyUnprocessedOptions));
                }

                return spareArguments.Concat(definitelyNotOptions).ToArray();
            }
            catch (OptionException ex)
            {
                throw new InvalidArgumentsException(ex);
            }
        }

        /// <summary>
        /// Removes all recognised options from the argument list and returns the remainder.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual string[] Strip(string[] args)
        {
            try
            {
                var definitelyNotOptions = args.SkipWhile(a => a != "--");

                var spareArguments = Options.Parse(args.TakeWhile(a => a != "--")).ToArray();
                return spareArguments.Concat(definitelyNotOptions).ToArray();
            }
            catch (OptionException ex)
            {
                throw new InvalidArgumentsException(ex);
            }
        }
    }


    public class SessionArguments<T> : SessionArguments
    {
        public SessionArguments(T arguments, OptionSet options) : base(options)
        {
            Arguments = arguments;

            var entryAssembly = Assembly.GetEntryAssembly();
            Application = entryAssembly == null ? "(unknown)" : Assembly.GetEntryAssembly().Location;
            
            AddStandardOptions();
        }

        private void AddStandardOptions()
        {
            ForArgumentsInterface<IVerbosityArgument>(a => Options.Add("v|verbose", "Verbose mode.", v => a.Verbose()));
        }

        public T Arguments { get; private set; }
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

        public new void Parse(string[] args)
        {
            AssertHasNotYetParsed();

            var spareArguments = base.Parse(args);

            ForArgumentsInterface<IArgumentList>(a => { foreach (var s in spareArguments) a.ArgumentList.Add(s); });
        }

        private void AssertHasNotYetParsed()
        {
            if (hasParsed) throw new NotSupportedException("SessionArguments#Parse may only be called once.");
            hasParsed = true;
        }

        public string Application { get; set; }
    }
}