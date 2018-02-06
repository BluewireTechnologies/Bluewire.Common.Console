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

        public void Increment(int increment = 1)
        {
            this.count += increment;

            while (this.count >= this.nextMilestone)
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
    }
}
