using System;

namespace Bluewire.Common.Console.Progress
{
    public static class FormattingHelpers
    {
        public static double RoundToSignificantFigures(this double number, int sigFig)
        {
            var magnitude = Math.Floor(Math.Log10((double)number)) + 1;
            var scaleFactor = Math.Pow(10, magnitude);
            return scaleFactor * Math.Round(number / scaleFactor, sigFig);
        }

        /// <summary>
        /// Implementation of rounding for duration.
        /// For 0: returns 0.
        /// For 1 >= x > 0: returns 1.
        /// Else: returns round(x).
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static long RoundDuration(double num)
        {
            if (num == 0) return 0;
            if (num <= 1) return 1;
            return (long)Math.Round(num);
        }

        public static string PrettyPrintItemDuration(TimeSpan totalDuration, long count)
        {
            var itemDuration = TimeSpan.FromTicks(totalDuration.Ticks / count);

            if (itemDuration.TotalMilliseconds < 3000)
            {
                return String.Format("{0}ms", itemDuration.TotalMilliseconds.RoundToSignificantFigures(3));
            }
            return TimeSpan.FromSeconds(RoundDuration(itemDuration.TotalSeconds)).ToString();
        }

        public static string PrettyPrintItemsPerSecond(TimeSpan totalDuration, long count)
        {
            var itemDuration = TimeSpan.FromTicks(totalDuration.Ticks / count);
            if (itemDuration.TotalSeconds > 1.5) return "< 1";
            if (itemDuration.TotalSeconds > 0.8) return "~ 1";
            var itemsPerSecond = 1 / itemDuration.TotalSeconds;
            if (itemsPerSecond < 10) return itemsPerSecond.ToString("#.#");

            return itemsPerSecond.RoundToSignificantFigures(3).ToString();
        }
    }
}
