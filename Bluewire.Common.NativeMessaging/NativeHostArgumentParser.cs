using System;
using System.Collections.Generic;

namespace Bluewire.Common.NativeMessaging
{
    public class NativeHostArgumentParser
    {
        public NativeHostSessionArguments Parse(IList<string> arguments)
        {
            var session = new NativeHostSessionArguments();

            using (var iterator = arguments.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (session.Origin == null)
                    {
                        Uri uri;
                        if (Uri.TryCreate(iterator.Current, UriKind.Absolute, out uri))
                        {
                            session.Origin = uri;
                        }
                    }
                    if (session.ParentWindowHandle == null)
                    {
                        int handle;
                        if (TryParseParentWindowHandle(iterator, out handle))
                        {
                            session.ParentWindowHandle = new IntPtr(handle);
                        }
                    }
                }
            }
            return session;
        }

        private static bool TryParseParentWindowHandle(IEnumerator<string> iterator, out int handle)
        {
            const string parentWindowArgument = "--parent-window";
            handle = 0;
            if (iterator.Current?.StartsWith(parentWindowArgument) != true) return false;

            var remaining = iterator.Current.Substring(parentWindowArgument.Length).TrimStart('=');
            if (!String.IsNullOrWhiteSpace(remaining))
            {
                if (int.TryParse(remaining, out handle)) return true;
                throw new FormatException($"Not a decimal integer: {remaining}");
            }

            if (!iterator.MoveNext()) return false;
            
            if (int.TryParse(iterator.Current, out handle)) return true;
            throw new FormatException($"Not a decimal integer: {iterator.Current}");
        }
    }

    public class NativeHostSessionArguments
    {
        public Uri Origin { get; set; }
        public IntPtr? ParentWindowHandle { get; set; }
    }
}
