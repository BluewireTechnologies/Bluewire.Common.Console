namespace Bluewire.Common.Console.Progress
{
    public interface IProgressReceiver
    {
        void Start();
        void Increment(int increment = 1);
        void End();
    }
}