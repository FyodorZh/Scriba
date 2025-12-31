using System;
using System.Diagnostics;
using System.IO;
using Scriba.JsonFactory;

namespace Scriba
{
    internal static class LogMessageBuilder
    {
        class DateTimeFormatWrapper : IExternalJson
        {
            private readonly DateTime _value;

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
                output.Write('.');

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

        public static IJsonObject Build(Severity severity, Severity ignoreStackFor, bool addTime, string message, params object[] list)
        {
            IJsonObject logMessage = JsonObject.Construct();

            if (addTime)
            {
                logMessage.AddElement(MessageAttributes.Time, new DateTimeFormatWrapper(DateTime.UtcNow));
            }
            logMessage.AddElement(MessageAttributes.Severity, severity.Serialize());
            if (list.Length > 0)
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
}