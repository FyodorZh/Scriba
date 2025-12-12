using System;
using Scriba.JsonFactory;

namespace Scriba
{
    public interface IContext : ILoggerContext
    {
        Severity LogFor { get; set;  }
        Severity IgnoreStackFor { get; set;  }
        string AppId { get; set;  }
        string MachineName { get; set;  }
        ITagList Tags { get; }

        void AddConsumer(ILogConsumer logConsumer);
        void RemoveConsumer(ILogConsumer logConsumer);
        void RemoveConsumerByType(Type type);
        void Message(IJsonObject message);
    }
}