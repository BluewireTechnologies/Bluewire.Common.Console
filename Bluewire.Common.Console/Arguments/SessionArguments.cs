using System;
using System.Linq;
using System.Reflection;
using Bluewire.Common.Console.Logging;
using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.Console.Arguments
{
    public class SessionArguments
    {
        public OptionSet Options { get; } = new OptionSet();
        public ArgumentList ArgumentList { get; } = new ArgumentList();
        public string Application { get; set; } = GetEntryPointApplicationPath();
        public ExcessPositionalArgumentsPolicy ExcessPositionalArgumentsPolicy { get; set; } = ExcessPositionalArgumentsPolicy.Ignore;
        private bool hasParsed;

        public void Parse(string[] args)
        {
            if (hasParsed) throw new NotSupportedException("SessionArguments#Parse may only be called once.");
            hasParsed = true;
            var parseResult = new OptionsParser().Parse(Options, args);
            parseResult.AssertNoUnrecognisedOptions();
            var extraArguments = ArgumentList.Parse(parseResult.RemainingArguments);
            if (extraArguments.Any())
            {
                var exception = new ExcessArgumentsException(extraArguments);
                switch (ExcessPositionalArgumentsPolicy)
                {
                    case ExcessPositionalArgumentsPolicy.Ignore:
                        break;
                    case ExcessPositionalArgumentsPolicy.Warn:
                        System.Console.Error.WriteLine(exception.Message);
                        break;
                    case ExcessPositionalArgumentsPolicy.Error:
                        throw exception;
                    default:
                        throw new ArgumentOutOfRangeException($"Unrecognised ExcessPositionalArgumentsPolicy: {ExcessPositionalArgumentsPolicy}");
                }
            }
        }

        public static string GetEntryPointApplicationPath() => Assembly.GetEntryAssembly()?.Location;
    }
}
