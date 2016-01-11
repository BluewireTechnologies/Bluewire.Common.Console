using System;

namespace Bluewire.Common.Console.Formatting
{
    public static class Alignment
    {
        public static string Left(string value, int width)
        {
            return value.PadRight(width);
        }

        public static string Right(string value, int width)
        {
            return value.PadLeft(width);
        }

        public static string Centre(string value, int width)
        {
            var pad = (int)Math.Truncate((double)(width - value.Length) / 2);
            return value.PadLeft(pad + value.Length).PadRight(width);
        }
    }
}