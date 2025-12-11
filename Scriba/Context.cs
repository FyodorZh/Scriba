using Scriba.JsonFactory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Scriba
{
    internal static class LogMessageBuilder
    {
        class DateTimeFormatWrapper : IExternalJson
        {
            private DateTime _value;

            public DateTimeFormatWrapper(DateTime value)
            {
                _value = value;
            }

            public void WriteTo(TextWriter output)
            {
                output.Write('\"');

                Format(output, _value);

                output.Write('\"');
            }

            public void WriteToAsText(TextWriter output)
            {
                Format(output, _value);
            }

            public void Release()
            {
            }

            private static void Format(TextWriter output, DateTime value)
            {
                AppendDigits(output, value.Year, 4);
                output.Write('-');

                AppendDigits(output, value.Month, 2);
                output.Write('-');

                AppendDigits(output, value.Day, 2);
                output.Write(' ');

                AppendDigits(output, value.Hour, 2);
                output.Write(':');

                AppendDigits(output, value.Minute, 2);
                output.Write(':');

                AppendDigits(output, value.Second, 2);
                output.Write(',');

                AppendDigits(output, value.Millisecond, 3);
            }

            private static void AppendDigits(TextWriter output, int digits, int digitsCount)
            {
                for (int digitIndex = digitsCount - 1; digitIndex >= 0; --digitIndex)
                {
                    int tmp;
                    switch (digitIndex)
                    {
                        case 3: tmp = digits / 1000; break;
                        case 2: tmp = digits / 100; break;
                        case 1: tmp = digits / 10; break;
                        default: tmp = digits; break;
                    }

                    output.Write((char)('0' + tmp % 10));
                }
            }
        }

        public static IJsonObject Build(Severity severity, Severity ignoreStackFor, string message, params object[] list)
        {
            IJsonObject logMessage = JsonObject.Construct();

            logMessage.AddElement(MessageAttributes.Time, new DateTimeFormatWrapper(DateTime.UtcNow));
            logMessage.AddElement(MessageAttributes.Severity, severity.Serialize());
            if (list != null && list.Length > 0)
            {
                logMessage.AddMultiElement(MessageAttributes.Message, message, list);
            }
            else
            {
                logMessage.AddElement(MessageAttributes.Message, message);
            }

            if (severity < ignoreStackFor)
            {
                IJsonArray jsonStack = logMessage.AddArray(MessageAttributes.Stack);

                var stack = new StackTrace(3, true);

                for (int i = 0; i < stack.FrameCount; ++i)
                {
                    StackFrame sf = stack.GetFrame(i);
                    var method = sf.GetMethod();

                    IJsonObject frame = jsonStack.AddObject();
                    frame.AddElement(MessageAttributes.StackFrameClass, method.DeclaringType!.Name);
                    frame.AddElement(MessageAttributes.StackFrameMethod, method.ToString());

                    string fileName = sf.GetFileName();
                    if (fileName != null)
                    {
                        frame.AddElement(MessageAttributes.StackFrameFile, fileName);
                        frame.AddElement(MessageAttributes.StackFrameLine, sf.GetFileLineNumber());
                    }
                }
            }

            return logMessage;
        }
    }

    internal class Context : IContext
    {
        private readonly List<ILogConsumer> _logConsumers = new List<ILogConsumer>();

        public Severity LogFor { get; set; }

        public Severity IgnoreStackFor { get; set; }

        public string AppId { get; set; }

        public string MachineName { get; set; }

        public TagList Tags { get; private set; }

        public Context()
        {
            LogFor = Severity.DEBUG;
            IgnoreStackFor = Severity.ERROR; // полное отключение логирования колстека
            MachineName = Environment.MachineName;
            AppId = "unknown";
            Tags = new TagList();
        }

        public void AddConsumer(ILogConsumer logConsumer)
        {
            _logConsumers.Add(logConsumer);
        }

        public void RemoveConsumer(ILogConsumer logConsumer)
        {
            logConsumer.Release();
            _logConsumers.Remove(logConsumer);
        }

        public void RemoveConsumerByType(Type type)
        {
            int j = 0;
            for (int i = 0; i < _logConsumers.Count; ++i)
            {
                if (_logConsumers[i].GetType() == type)
                {
                    _logConsumers[i].Release();
                }
                else
                {
                    _logConsumers[j++] = _logConsumers[i];
                }
            }

            _logConsumers.RemoveRange(j, _logConsumers.Count - j);
        }

        public void Message(IJsonObject message)
        {
            message.AddElement(MessageAttributes.AppId, AppId);
            message.AddElement(MessageAttributes.MachineName, MachineName);

            {
                if (!message.TryGet(MessageAttributes.Tags, out JsonElement field) || !field.TryGet(out IJsonArray? tags))
                {
                    tags = message.AddArray(MessageAttributes.Tags);
                }
                Tags.WriteTo(tags);
            }

            for (int i = _logConsumers.Count - 1; i >= 0; --i)
            {
                try
                {
                    _logConsumers[i].Message(new MessageData(message));
                }
                catch
                {
                    // TODO
                }
            }

            message.Dispose();
        }

        public void Destroy()
        {
            foreach (var consumer in _logConsumers)
            {
                consumer.Release();
            }

            _logConsumers.Clear();
        }
    }
}