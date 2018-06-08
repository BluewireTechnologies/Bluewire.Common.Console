using System;
using System.Collections.Generic;
using System.Linq;
using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console.Arguments
{
    public class OptionsParser
    {
        public Result Parse(OptionSet options, string[] args)
        {
            try
            {
                var definitelyNotOptions = args.SkipWhile(a => a != "--");

                var spareArguments = options.Parse(args.TakeWhile(a => a != "--")).ToArray();

                var possiblyUnprocessedOptions = spareArguments.Where(a => a.StartsWith("-")).ToArray();
                return new Result(spareArguments.Concat(definitelyNotOptions).ToArray(), possiblyUnprocessedOptions);
            }
            catch (OptionException ex)
            {
                throw new InvalidArgumentsException(ex);
            }
        }

        public class Result
        {
            public Result(string[] remainingArguments, string[] unrecognisedOptions)
            {
                RemainingArguments = remainingArguments.AsEnumerable();
                UnrecognisedOptions = unrecognisedOptions.AsEnumerable();
            }

            public IEnumerable<string> RemainingArguments { get; }

            public IEnumerable<string> UnrecognisedOptions { get; }

            public void AssertNoUnrecognisedOptions()
            {
                if (UnrecognisedOptions.Any())
                {
                    throw new InvalidArgumentsException("Unrecognised option(s): {0}", String.Join(", ", UnrecognisedOptions));
                }
            }
        }
    }
}
