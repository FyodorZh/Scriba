using System;

namespace Scriba
{
    public interface ILogger
    {
        Severity LogFor { get; set; }
        Severity IgnoreStackFor { get; set; }
        string AppId { get; set; }
        string MachineName { get; set; }
        ITagList Tags { get; }

        void d(string format, params object[] args);
        void i(string format, params object[] args);
        void w(string format, params object[] args);
        void e(string format, params object[] args);

        void wtf(string message, Exception exception);
        void wtf(Exception exception);

        void json(JsonFactory.IJsonObject message);
    }
}