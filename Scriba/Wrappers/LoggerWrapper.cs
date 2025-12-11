using System;

namespace Scriba
{
    internal abstract class LoggerWrapper : ILogger
    {
        private readonly ILogger _log;

        protected LoggerWrapper(ILogger log)
        {
            _log = log;
        }

        protected abstract void AppendTags(JsonFactory.IJsonArray tags);

        #region ILogger

        public Severity LogFor
        {
            get => _log.LogFor;
            set => _log.LogFor = value;
        }

        public Severity IgnoreStackFor
        {
            get => _log.IgnoreStackFor;
            set => _log.IgnoreStackFor = value;
        }

        public string AppId
        {
            get => _log.AppId;
            set => _log.AppId = value;
        }

        public string MachineName
        {
            get => _log.MachineName;
            set => _log.MachineName = value;
        }

        public ITagList Tags => _log.Tags;

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
            if (!json.TryGet(MessageAttributes.Tags, out var tagsField) || !tagsField.TryGet(out JsonFactory.IJsonArray? tags))
            {
                tags = json.AddArray(MessageAttributes.Tags);
            }
            
            AppendTags(tags);

            _log.json(json);
        }

        #endregion

        private void Message(Severity severity, string format, params object[] args)
        {
            var json = LogMessageBuilder.Build(severity, IgnoreStackFor, format, args);
            var tags = json.AddArray(MessageAttributes.Tags);
            AppendTags(tags);
            _log.json(json);
        }
    }
}