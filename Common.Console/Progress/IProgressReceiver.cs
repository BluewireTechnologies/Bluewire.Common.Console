namespace Bluewire.Common.Console.Progress
{
    public interface IProgressReceiver
    {
        void Start();
        void Increment();
        void End();
    }
}