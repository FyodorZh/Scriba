using System;
using Scriba.JsonFactory;

namespace Scriba
{
    public class StaticLogger : ILoggerExt
    {
        public static readonly StaticLogger Instance = new StaticLogger();
        
        public Severity LogFor
        {
            get => Log.LogFor;
            set => Log.LogFor = value;
        }

        public Severity IgnoreStackFor
        {
            get => Log.IgnoreStackFor;
            set => Log.IgnoreStackFor = value;
        }

        public string? AppId
        {
            get => Log.AppId;
            set => Log.AppId = value;
        }

        public string? MachineName
        {
            get => Log.MachineName;
            set => Log.MachineName = value;
        }

        public bool LogTime
        {
            get => Log.LogTime;
            set => Log.LogTime = value;
        }

        public ITagList Tags => Log.Tags;
        
        public void AddConsumer(ILogConsumer logConsumer)
        {
            Log.AddConsumer(logConsumer);
        }

        public void RemoveConsumer(ILogConsumer logConsumer)
        {
            Log.RemoveConsumer(logConsumer);
        }

        public void RemoveConsumerByType(Type type)
        {
            Log.RemoveConsumerByType(type);
        }

        public void d(string format, params object[] args)
        {
            Log.d(format, args);
        }

        public void i(string format, params object[] args)
        {
            Log.i(format, args);
        }

        public void w(string format, params object[] args)
        {
            Log.w(format, args);
        }

        public void e(string format, params object[] args)
        {
            Log.e(format, args);
        }

        public void wtf(string message, Exception exception)
        {
            Log.wtf(message, exception);
        }

        public void wtf(Exception exception)
        {
            Log.wtf(exception);
        }

        public void json(IJsonObject message)
        {
            Log.json(message);
        }

        public void Publish(MessageData message)
        {
            Log.Publish(message);
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}