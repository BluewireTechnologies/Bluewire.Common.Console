using System;
using System.Diagnostics;

namespace Bluewire.Common.Console.Progress
{
    public abstract class PowerOfTenMilestoneCounter : IProgressReceiver
    {
        private Stopwatch stopwatch;
        private long count = 0;
        private long nextMilestone;

        protected PowerOfTenMilestoneCounter(int initialMagnitude = 3)
        {
            this.nextMilestone = (long)Math.Pow(10, initialMagnitude);
            this.stopwatch = new Stopwatch();
        }


        public virtual void Start()
        {
            this.stopwatch.Start();
        }

        public void Increment()
        {
            this.count++;

            if (this.count == this.nextMilestone)
            {
                Milestone(this.count, this.stopwatch.Elapsed);
                this.nextMilestone *= 10;
            }
        }

        protected abstract void Milestone(long milestone, TimeSpan elapsed);

        public virtual void End()
        {
            this.stopwatch.Stop();
            Milestone(this.count, this.stopwatch.Elapsed);
        }

        /// <summary>
        /// Implementation of rounding for duration.
        /// For 0: returns 0.
        /// For 1 >= x > 0: returns 1.
        /// Else: returns round(x).
        /// Rounds 0 to 0.
        /// Rounds numbers between zero and one to
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        protected static long RoundDuration(double num)
        {
            if (num == 0) return 0;
            if (num <= 1) return 1;
            return (long)Math.Round(num);
        }

        protected static object FormatItemDuration(TimeSpan totalDuration, long count)
        {
            var itemDuration = TimeSpan.FromTicks(totalDuration.Ticks / count);

            if (itemDuration.TotalMilliseconds < 3000)
            {
                return String.Format("{0}ms", itemDuration.TotalMilliseconds);
            }
            return TimeSpan.FromSeconds(RoundDuration(itemDuration.TotalSeconds));
        }

    }
}