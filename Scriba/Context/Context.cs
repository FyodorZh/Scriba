using Scriba.JsonFactory;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Scriba
{
    internal class Context : IContext
    {
        private readonly List<ILogConsumer> _logConsumers = new();
        private readonly TagList _tags = new();
        
        private readonly ReaderWriterLockSlim _locker = new ();

        public Severity LogFor { get; set; }

        public Severity IgnoreStackFor { get; set; }

        public string AppId { get; set; }

        public string MachineName { get; set; }

        public ITagList Tags => _tags;

        public Context()
        {
            LogFor = Severity.DEBUG;
            IgnoreStackFor = Severity.ERROR; // полное отключение логирования колстека
            MachineName = Environment.MachineName;
            AppId = "unknown";
        }

        public void AddConsumer(ILogConsumer logConsumer)
        {
            _locker.EnterWriteLock();
            try
            {
                _logConsumers.Add(logConsumer);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void RemoveConsumer(ILogConsumer logConsumer)
        {
            _locker.EnterWriteLock();
            try
            {
                _logConsumers.Remove(logConsumer);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
            logConsumer.Release();
        }

        public void RemoveConsumerByType(Type type)
        {
            List<ILogConsumer> toRemove = new List<ILogConsumer>();
            
            _locker.EnterWriteLock();
            try
            {
                for (int i = 0; i < _logConsumers.Count; ++i)
                {
                    if (_logConsumers[i].GetType() == type)
                    {
                        toRemove.Add(_logConsumers[i]);
                        _logConsumers.RemoveAt(i);
                    }
                }
            }
            finally
            {
                _locker.ExitWriteLock();
            }
            
            foreach (var consumer in toRemove)
            {
                consumer.Release();
            }
        }

        public void Message(IJsonObject message)
        {
            message.AddElement(MessageAttributes.AppId, AppId);
            message.AddElement(MessageAttributes.MachineName, MachineName);
            if (!message.TryGet(MessageAttributes.Tags, out JsonElement field) || !field.TryGet(out IJsonArray tags))
            {
                tags = message.AddArray(MessageAttributes.Tags);
            }
            _tags.WriteTo(tags);

            _locker.EnterReadLock();
            try
            {
                for (int i = _logConsumers.Count - 1; i >= 0; --i)
                {
                    try
                    {
                        _logConsumers[i].Message(new MessageData(message));
                    }
                    catch
                    {
                        // TODO
                    }
                }
            }
            finally
            {
                _locker.ExitReadLock();
            }

            message.Dispose();
        }

        public void Destroy()
        {
            _locker.EnterWriteLock();
            try
            {
                foreach (var consumer in _logConsumers)
                {
                    consumer.Release();
                }
                _logConsumers.Clear();
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }
    }
}