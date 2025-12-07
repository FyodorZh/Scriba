namespace Scriba.Consumers
{
    public class InMemoryLogAccumulator : ILogConsumer
    {
        private readonly InMemoryLogList _logs;

        public InMemoryLogList Logs => _logs;

        public InMemoryLogAccumulator(InMemoryLogList accumulator)
        {
            _logs = accumulator;
        }

        #region ILogConsumer

        void ILogConsumer.AddRef()
        {
            // DO NOTHING
        }

        void ILogConsumer.Message(MessageData logMessage)
        {
            _logs.Append(logMessage);
        }

        void ILogConsumer.Release()
        {
            // DO NOTHING
        }

        #endregion
    }

}
