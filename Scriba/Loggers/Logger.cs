using System;
using System.Collections.Generic;
using System.Threading;
using Scriba.JsonFactory;

namespace Scriba
{
    public class Logger : ILoggerExt
    {
        private readonly List<ILogConsumer> _logConsumers = new();
        private readonly TagList _tags = new();
        private readonly ReaderWriterLockSlim _locker = new ();

        public Severity LogFor { get; set; }
        public Severity IgnoreStackFor { get; set; }
        public string? AppId { get; set; }
        public string? MachineName { get; set; }
        public bool LogTime { get; set; }
        
        public virtual ITagList Tags => _tags;

        public Logger()
        {
            LogFor = Severity.DEBUG;
            IgnoreStackFor = Severity.ERROR; // полное отключение логирования колстека
            LogTime = true;
        }

        public Logger(IEnumerable<ILogConsumer> consumers)
            :this()
        {
            foreach (var consumer in consumers)
            {
                if (consumer != null)
                {
                    _logConsumers.Add(consumer);
                }
            }
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

        public void d(string format, params object[] args)
        {
            Message(Severity.DEBUG, format, args);
        }

        public void i(string format, params object[] args)
        {
            Message(Severity.INFO, format, args);
        }

        public void w(string format, params object[] args)
        {
            Message(Severity.WARN, format, args);
        }

        public void e(string format, params object[] args)
        {
            Message(Severity.ERROR, format, args);
        }

        public void wtf(string message, Exception exception)
        {
            Message(Severity.ERROR, "Exception ({text}): {exception}", message, exception.ToString());
        }

        public void wtf(Exception exception)
        {
            Message(Severity.ERROR, "Exception : {exception}", exception.ToString());
        }

        public void json(IJsonObject message)
        {
            Message(message);
        }

        private void Message(Severity severity, string format, params object[] args)
        {
            if (severity <= LogFor)
            {
                Message(LogMessageBuilder.Build(severity, IgnoreStackFor, LogTime, format, args));
            }
        }
        
        private void Message(IJsonObject message)
        {
            var appId = AppId;
            var machineName = MachineName;
            if (appId != null)
            {
                message.AddElement(MessageAttributes.AppId, appId);
            }
            if (machineName != null)
            {
                message.AddElement(MessageAttributes.MachineName, machineName);
            }
            var tagList = Tags; // virtual call
            if (!tagList.IsEmpty)
            {
                if (!message.TryGet(MessageAttributes.Tags, out JsonElement field) || !field.TryGet(out IJsonArray tags))
                {
                    tags = message.AddArray(MessageAttributes.Tags);
                }
                tagList.WriteTo(tags);
            }

            Publish(new MessageData(message));

            message.Dispose();
        }
        
        public virtual void Publish(MessageData message)
        {
            _locker.EnterReadLock();
            try
            {
                for (int i = _logConsumers.Count - 1; i >= 0; --i)
                {
                    try
                    {
                        _logConsumers[i].Message(message);
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
        }

        public virtual void Dispose()
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
            _locker.Dispose();
        }
    }
}