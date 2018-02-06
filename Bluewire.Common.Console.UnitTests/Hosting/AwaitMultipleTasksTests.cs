using System.Threading.Tasks;
using Bluewire.Common.Console.Hosting;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Hosting
{
    [TestFixture]
    public class AwaitMultipleTasksTests
    {
        [Test]
        public void AwaitingNoTasksCompletesImmediately()
        {
            var awaiter = new AwaitMultipleTasks();
            var task = awaiter.GetWaitTask();
            Assert.True(task.IsCompleted);
        }

        [Test]
        public void AwaitingATaskIsNotCompleteWhenThatTaskIsNotComplete()
        {
            var source = new TaskCompletionSource<object>();
            Assume.That(!source.Task.IsCompleted);

            var awaiter = new AwaitMultipleTasks();
            awaiter.Track(source.Task);

            var task = awaiter.GetWaitTask();
            Assert.False(task.IsCompleted);
        }

        [Test]
        public void AwaitingMultipleTasksIsNotCompleteWhenAnyOfThoseTasksAreIncomplete()
        {
            var sourceA = new TaskCompletionSource<object>();
            var sourceB = new TaskCompletionSource<object>();
            var sourceC = new TaskCompletionSource<object>();

            sourceA.SetResult(new object());
            sourceC.SetResult(new object());

            var awaiter = new AwaitMultipleTasks();
            awaiter.Track(sourceA.Task);
            awaiter.Track(sourceB.Task);
            awaiter.Track(sourceC.Task);

            var task = awaiter.GetWaitTask();
            Assert.False(task.IsCompleted);
        }

        [Test]
        public void AwaitingMultipleTasksCompletesWhenAllOfThoseTasksAreComplete()
        {
            var sourceA = new TaskCompletionSource<object>();
            var sourceB = new TaskCompletionSource<object>();
            var sourceC = new TaskCompletionSource<object>();

            sourceA.SetResult(new object());

            var awaiter = new AwaitMultipleTasks();
            awaiter.Track(sourceA.Task);
            awaiter.Track(sourceB.Task);
            awaiter.Track(sourceC.Task);

            sourceC.SetResult(new object());

            var task = awaiter.GetWaitTask();

            sourceB.SetResult(new object());
            Assert.True(task.IsCompleted);
        }

        [Test]
        public void AwaitingAlreadyCompletedTaskCompletesImmediately()
        {
            var source = new TaskCompletionSource<object>();
            source.SetResult(new object());

            var awaiter = new AwaitMultipleTasks();
            awaiter.Track(source.Task);

            var task = awaiter.GetWaitTask();
            Assert.True(task.IsCompleted);
        }

        [Test]
        public void AwaitingATaskCompletesWhenThatTaskDoes()
        {
            var source = new TaskCompletionSource<object>();

            var awaiter = new AwaitMultipleTasks();
            awaiter.Track(source.Task);

            var task = awaiter.GetWaitTask();

            source.SetResult(new object());

            Assert.True(task.IsCompleted);
        }

        [Test]
        public void CallingGetWaitTaskMultipleTimesDoesNotCorruptState()
        {
            var sourceA = new TaskCompletionSource<object>();
            var sourceB = new TaskCompletionSource<object>();
            var sourceC = new TaskCompletionSource<object>();

            sourceA.SetResult(new object());
            sourceC.SetResult(new object());

            var awaiter = new AwaitMultipleTasks();
            awaiter.Track(sourceA.Task);
            awaiter.Track(sourceB.Task);
            awaiter.Track(sourceC.Task);

            Assert.False(awaiter.GetWaitTask().IsCompleted);
            Assert.False(awaiter.GetWaitTask().IsCompleted);

            sourceB.SetResult(new object());
            Assert.True(awaiter.GetWaitTask().IsCompleted);
        }
    }
}
