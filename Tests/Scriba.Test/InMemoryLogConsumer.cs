using System.Collections.Generic;
using Scriba.JsonFactory;

namespace Scriba.Consumers
{
    public class InMemoryLogConsumer : ILogConsumer
    {
        private readonly List<string> _logs = new();

        public string[] TakeAll()
        {
            lock (_logs)
            {
                var res = _logs.ToArray();
                _logs.Clear();
                return res;
            }
        }
        #region ILogConsumer

        void ILogConsumer.AddRef()
        {
            // DO NOTHING
        }

        void ILogConsumer.Message(MessageData logMessage)
        {
            lock (_logs)
            {
                if (logMessage.Data.Serialize(out var str))
                {
                    _logs.Add(str);
                }
            }
        }

        void ILogConsumer.Release()
        {
            // DO NOTHING
        }

        #endregion
    }

}
