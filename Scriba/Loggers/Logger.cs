using System;
using System.Collections.Generic;

namespace Scriba
{
    public class Logger : ILogger
    {
        private readonly Context _context = new Context();

        public Logger(ILogConsumer consumer)
        {
            _context.AddConsumer(consumer);
        }

        public Logger(IEnumerable<ILogConsumer> consumers)
        {
            foreach (var consumer in consumers)
            {
                if (consumer != null)
                {
                    _context.AddConsumer(consumer);
                }
            }
        }

        public Severity LogFor
        {
            get => _context.LogFor;
            set => _context.LogFor = value;
        }

        public Severity IgnoreStackFor
        {
            get => _context.IgnoreStackFor;
            set => _context.IgnoreStackFor = value;
        }

        public string AppId
        {
            get => _context.AppId;
            set => _context.AppId = value;
        }

        public string MachineName
        {
            get => _context.MachineName;
            set => _context.MachineName = value;
        }

        public ITagList Tags => _context.Tags;

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

        public void json(JsonFactory.IJsonObject message)
        {
            _context.Message(message);
        }

        private void Message(Severity severity, string format, params object[] args)
        {
            if (severity <= _context.LogFor)
            {
                _context.Message(LogMessageBuilder.Build(severity, _context.IgnoreStackFor, format, args));
            }
        }
    }
}