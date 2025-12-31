using System;

namespace Scriba.Test
{
    public interface ILogCommand
    {
        void Execute(ILogger logger);
    }

    public class SetLogForCommand : ILogCommand
    {
        private readonly Severity _severity;
        public SetLogForCommand(Severity severity) => _severity = severity;
        public void Execute(ILogger logger) => logger.LogFor = _severity;
        public override string ToString() => $"SetLogForCommand({_severity})";
    }

    public class SetIgnoreStackForCommand : ILogCommand
    {
        private readonly Severity _severity;
        public SetIgnoreStackForCommand(Severity severity) => _severity = severity;
        public void Execute(ILogger logger) => logger.IgnoreStackFor = _severity;
        public override string ToString() => $"SetIgnoreStackForCommand({_severity})";
    }

    public class SetAppIdCommand : ILogCommand
    {
        private readonly string _appId;
        public SetAppIdCommand(string appId) => _appId = appId;
        public void Execute(ILogger logger) => logger.AppId = _appId;
        public override string ToString() => $"SetAppIdCommand({_appId})";
    }

    public class SetMachineNameCommand : ILogCommand
    {
        private readonly string _machineName;
        public SetMachineNameCommand(string machineName) => _machineName = machineName;
        public void Execute(ILogger logger) => logger.MachineName = _machineName;
        public override string ToString() => $"SetMachineNameCommand({_machineName})";
    }

    public class AddTag : ILogCommand
    {
        private readonly string _tag;
        private readonly string? _value;
        public AddTag(string tag, string? value) => (_tag, _value) = (tag, value);
        public void Execute(ILogger logger) => logger.Tags.Set(_tag, _value);
        public override string ToString() => $"AddTag({_tag}, {_value})";
    }
    
    public class RemoveTag : ILogCommand
    {
        private readonly string _tag;
        public RemoveTag(string tag) => _tag = tag;
        public void Execute(ILogger logger) => logger.Tags.Remove(_tag);
        public override string ToString() => $"RemoveTag({_tag})";
    }

    public class AddConsumerCommand : ILogCommand
    {
        private readonly ILogConsumer _logConsumer;
        public AddConsumerCommand(ILogConsumer logConsumer) => _logConsumer = logConsumer;
        public void Execute(ILogger logger) => logger.AddConsumer(_logConsumer);
        public override string ToString() => $"AddConsumerCommand({_logConsumer})";
    }

    public class RemoveConsumerCommand : ILogCommand
    {
        private readonly ILogConsumer _logConsumer;
        public RemoveConsumerCommand(ILogConsumer logConsumer) => _logConsumer = logConsumer;
        public void Execute(ILogger logger) => logger.RemoveConsumer(_logConsumer);
        public override string ToString() => $"RemoveConsumerCommand({_logConsumer})";
    }

    public class RemoveConsumerByTypeCommand : ILogCommand
    {
        private readonly Type _type;
        public RemoveConsumerByTypeCommand(Type type) => _type = type;
        public void Execute(ILogger logger) => logger.RemoveConsumerByType(_type);
        public override string ToString() => $"RemoveConsumerByTypeCommand({_type})";
    }

    public class DebugCommand : ILogCommand
    {
        private readonly string _format;
        private readonly object[] _args;
        public DebugCommand(string format, params object[] args)
        {
            _format = format;
            _args = args;
        }
        public void Execute(ILogger logger) => logger.d(_format, _args);
        public override string ToString() => $"DebugCommand({_format})";
    }

    public class InfoCommand : ILogCommand
    {
        private readonly string _format;
        private readonly object[] _args;
        public InfoCommand(string format, params object[] args)
        {
            _format = format;
            _args = args;
        }
        public void Execute(ILogger logger) => logger.i(_format, _args);
        public override string ToString() => $"InfoCommand({_format})";
    }

    public class WarnCommand : ILogCommand
    {
        private readonly string _format;
        private readonly object[] _args;
        public WarnCommand(string format, params object[] args)
        {
            _format = format;
            _args = args;
        }
        public void Execute(ILogger logger) => logger.w(_format, _args);
        public override string ToString() => $"WarnCommand({_format})";
    }

    public class ErrorCommand : ILogCommand
    {
        private readonly string _format;
        private readonly object[] _args;
        public ErrorCommand(string format, params object[] args)
        {
            _format = format;
            _args = args;
        }
        public void Execute(ILogger logger) => logger.e(_format, _args);
        public override string ToString() => $"ErrorCommand({_format})";
    }

    public class WtfWithMessageCommand : ILogCommand
    {
        private readonly string _message;
        private readonly Exception _exception;
        public WtfWithMessageCommand(string message, Exception exception)
        {
            _message = message;
            _exception = exception;
        }
        public void Execute(ILogger logger) => logger.wtf(_message, _exception);
        public override string ToString() => $"WtfWithMessageCommand({_message}, {_exception})";
    }

    public class WtfCommand : ILogCommand
    {
        private readonly Exception _exception;
        public WtfCommand(Exception exception) => _exception = exception;
        public void Execute(ILogger logger) => logger.wtf(_exception);
        public override string ToString() => $"WtfCommand({_exception})";
    }

    public class JsonCommand : ILogCommand
    {
        private readonly JsonFactory.IJsonObject _message;
        public JsonCommand(JsonFactory.IJsonObject message) => _message = message;
        public void Execute(ILogger logger) => logger.json(_message);
        public override string ToString() => $"JsonCommand({_message})";
    }
}