using System;
using Scriba.JsonFactory;

namespace Scriba
{
    public class VoidLogger : ILogger
    {
        public static readonly VoidLogger Instance = new VoidLogger();
        
        public bool IsActive => false;

        public Severity LogFor { get; set; }

        public Severity IgnoreStackFor { get; set; }
        public string AppId { get; set; }
        public string MachineName { get; set; }

        public ITagList Tags => VoidTagList.Instance;

        public void d(string format, params object[] args)
        {
            // DO NOTHING
        }

        public void i(string format, params object[] args)
        {
            // DO NOTHING
        }

        public void w(string format, params object[] args)
        {
            // DO NOTHING
        }

        public void e(string format, params object[] args)
        {
            // DO NOTHING
        }

        public void wtf(string message, Exception exception)
        {
            // DO NOTHING
        }

        public void wtf(Exception exception)
        {
            // DO NOTHING
        }

        public void json(IJsonObject message)
        {
            message.Dispose();
        }
    }
}