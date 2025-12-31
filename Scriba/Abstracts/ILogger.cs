using System;

namespace Scriba
{
    public interface ILogger : IDisposable
    {
        Severity LogFor { get; set; }
        Severity IgnoreStackFor { get; set; }
        string? AppId { get; set; }
        string? MachineName { get; set; }
        bool LogTime { get; set; }
        ITagList Tags { get; }
        
        void AddConsumer(ILogConsumer logConsumer);
        void RemoveConsumer(ILogConsumer logConsumer);
        void RemoveConsumerByType(Type type);

        void d(string format, params object[] args);
        void i(string format, params object[] args);
        void w(string format, params object[] args);
        void e(string format, params object[] args);

        void wtf(string message, Exception exception);
        void wtf(Exception exception);

        void json(JsonFactory.IJsonObject message);
    }

    public interface ILoggerExt : ILogger
    {
        void Publish(MessageData message);
    }
}