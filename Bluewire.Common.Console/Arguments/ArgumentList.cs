using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bluewire.Common.Console.Arguments
{
    public class ArgumentList : IEnumerable<string>
    {
        private readonly List<PositionalArgument> positionalArguments = new List<PositionalArgument>();
        private PositionalArgument remainder;

        public ArgumentList Add(string description, Action<string> onArgument)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (onArgument == null) throw new ArgumentNullException(nameof(onArgument));
            positionalArguments.Add(new PositionalArgument { Description = description, Receive = onArgument });
            return this;
        }

        public ArgumentList AddRemainder(string description, Action<string> onArgument)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (onArgument == null) throw new ArgumentNullException(nameof(onArgument));
            if (remainder != null) throw new InvalidOperationException("Remainder argument handler has already been defined.");
            remainder = new PositionalArgument { Description = description, Receive = onArgument };
            return this;
        }

        public bool IsDefined => positionalArguments.Any() || remainder != null;

        public string[] Parse(IEnumerable<string> arguments)
        {
            return ParseInternal(arguments).ToArray();
        }

        private IEnumerable<string> ParseInternal(IEnumerable<string> arguments)
        {
            var i = 0;
            foreach (var arg in arguments.Where(a => a != "--"))
            {
                var handler = GetPositionalArgument(i);
                if (handler == null)
                {
                    yield return arg;
                }
                else
                {
                    handler.Receive(arg);
                }
                i++;
            }
        }

        public string GetArgumentDescriptions()
        {
            return String.Join(" ", EnumerateDescriptions().Select(a => $"<{a}>"));
        }

        private IEnumerable<string> EnumerateDescriptions()
        {
            foreach (var arg in positionalArguments) yield return arg.Description;
            if (remainder != null) yield return remainder.Description;
        }

        private PositionalArgument GetPositionalArgument(int index) => positionalArguments.ElementAtOrDefault(index) ?? remainder;
        public IEnumerator<string> GetEnumerator() => EnumerateDescriptions().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        class PositionalArgument
        {
            public string Description { get; set; }
            public Action<string> Receive { get; set; }
        }
    }
}
