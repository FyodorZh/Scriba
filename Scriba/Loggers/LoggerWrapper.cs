using System;
using Scriba.JsonFactory;

namespace Scriba
{
    public class LoggerWrapper : Logger, ITagList
    {
        private readonly ILoggerExt _core;

        public override ITagList Tags => this;

        internal LoggerWrapper(ILoggerExt core)
        {
            _core = core;
            LogFor = core.LogFor;
            IgnoreStackFor = core.IgnoreStackFor;
            AppId = core.AppId;
            MachineName = core.MachineName;
            LogTime = core.LogTime;
        }

        public override void Publish(MessageData message)
        {
            _core.Publish(message);
            base.Publish(message);
        }

        bool ITagList.IsEmpty => base.Tags.IsEmpty && _core.Tags.IsEmpty;

        void ITagList.Set(string tag, string? value)
        {
            base.Tags.Set(tag, value);
        }
        
        void ITagList.Set(string tag, Func<string> valueFactory)
        {
            base.Tags.Set(tag, valueFactory);
        }

        bool ITagList.Remove(string tag)
        {
            return base.Tags.Remove(tag);
        }

        void ITagList.WriteTo(IJsonArray tags)
        {
            _core.Tags.WriteTo(tags);
            base.Tags.WriteTo(tags);
        }
    }

    public static class LoggerWrapper_Ext
    {
        public static ILogger Wrap(this ILogger logger)
        {
            if (logger is ILoggerExt loggerExt)
                return new LoggerWrapper(loggerExt);
            throw new InvalidOperationException("Logger must implement ILoggerExt to be wrapped");
        }
    }
}