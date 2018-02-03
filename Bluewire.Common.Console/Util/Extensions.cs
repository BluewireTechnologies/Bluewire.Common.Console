namespace Bluewire.Common.Console.Util
{
    internal static class Extensions
    {
        public static string EnsureSingleTrailing(this string str, char trailing)
        {
            return str.TrimEnd(trailing) + trailing;
        }
    }
}
