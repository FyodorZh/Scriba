namespace Scriba
{
    public abstract class MultiRefLogConsumer : ILogConsumer
    {
        private int _refCount = 1;

        public void AddRef()
        {
            if (_refCount > 0)
            {
                System.Threading.Interlocked.Increment(ref _refCount);
            }
        }

        public void Release()
        {
            if (System.Threading.Interlocked.Decrement(ref _refCount) == 0)
            {
                Dispose();
            }
        }

        protected virtual void Dispose()
        {
            // Override to implement custom dispose logic
        }

        public abstract void Message(MessageData logMessage);
    }
}