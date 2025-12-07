using System;

namespace Scriba
{
    internal abstract class LoggerWrapper : ILogger
    {
        private readonly ILogger mLog;

        protected LoggerWrapper(ILogger log)
        {
            mLog = log;
        }

        protected abstract void AppendTags(JsonFactory.IJsonArray tags);

        #region ILogger

        public Severity LogFor
        {
            get => mLog.LogFor;
            set => mLog.LogFor = value;
        }

        public Severity IgnoreStackFor
        {
            get => mLog.IgnoreStackFor;
            set => mLog.IgnoreStackFor = value;
        }

        public string AppId
        {
            get => mLog.AppId;
            set => mLog.AppId = value;
        }

        public string MachineName
        {
            get => mLog.MachineName;
            set => mLog.MachineName = value;
        }

        public ITagList Tags => mLog.Tags;

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

        public void json(JsonFactory.IJsonObject json)
        {
            JsonFactory.IJsonArray tags;
            if (!json.Get(MessageAttributes.Tags).TryGet(out tags))
            {
                tags = json.AddArray(MessageAttributes.Tags);
            }

            if (tags != null)
            {
                AppendTags(tags);
            }

            mLog.json(json);
        }

        #endregion

        private void Message(Severity severity, string format, params object[] args)
        {
            var json = LogMessageBuilder.Build(severity, IgnoreStackFor, format, args);
            var tags = json.AddArray(MessageAttributes.Tags);
            AppendTags(tags);
            mLog.json(json);
        }
    }
}