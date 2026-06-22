using System;
using System.IO;
using System.Text;
using Scriba.JsonFactory;

namespace Scriba
{
    public readonly struct MessageData
    {
        public IJsonObject Data { get; }
        
        internal MessageData(IJsonObject data)
        {
            Data = data;
        }

        public Severity Severity
        {
            get
            {
                if (!Data.TryGet(MessageAttributes.Severity, out var severityField) || !severityField.TryGet(out long severity))
                {
                    return Severity.UNKNOWN;
                }
                return (Severity)severity;
            }
        }

        public DateTime? Time
        {
            get
            {
                if (!Data.TryGet(MessageAttributes.Time, out var field) || !field.TryGet(out long binaryTime))
                {
                    return null;
                }

                return DateTime.FromBinary(binaryTime);
            }
        }

        public bool WriteTagsTo(System.IO.TextWriter output, Predicate<string>? tagsSelector = null)
        {
            if (Data.TryGet(MessageAttributes.Tags, out var tagsField) && tagsField.TryGet(out IJsonArray? tags))
            {
                int count = tags.Count;
                for (int i = count - 1; i >= 0; --i)
                {
                    var element = tags[i];

                    if (element.TryGet(out IJsonObject? tagPair))
                    {
                        int pairs = tagPair.Count; // must be 1
                        for (int j = 0; j < pairs; ++j)
                        {
                            (string Name, JsonElement Field) kv = tagPair[j];
                            if ((tagsSelector?.Invoke(kv.Name) ?? true) && kv.Field.TryGet(out string value))
                            {
                                output.Write(kv.Name);
                                output.Write("=");
                                output.Write(value);
                                output.Write("; ");
                            }
                        }
                    }

                    if (element.TryGet(out string tag))
                    {
                        if (tagsSelector?.Invoke(tag) ?? true)
                        {
                            output.Write(tag);
                            output.Write("; ");
                        }
                    }
                }
            }

            return true;
        }

        public bool WriteMessageTo(TextWriter output)
        {
            return Data.TryGet(MessageAttributes.Message, out var messageField) && messageField.WriteTo(output, false);
        }

        public int StackTraceDepth
        {
            get
            {
                if (Data.TryGet(MessageAttributes.Stack, out var stackField) && stackField.TryGet(out IJsonArray? stack))
                {
                    return stack.Count;
                }
                return 0;
            }
        }

        public bool WriteStackTrace(string prefix, TextWriter output)
        {
            if (Data.TryGet(MessageAttributes.Stack, out var stackField) && stackField.TryGet(out IJsonArray? stack))
            {
                int cnt = stack.Count;
                for (int i = 0; i < cnt; ++i)
                {
                    if (!stack[i].TryGet(out IJsonObject? frame) || !WriteStackFrame(frame, prefix, output))
                    {
                        return false;
                    }
                    output.WriteLine();
                }

                return true;
            }
            return false;
        }

        public bool WriteStackFrame(int frameId, string prefix, System.IO.TextWriter output)
        {
            if (Data.TryGet(MessageAttributes.Stack, out var stackField) && stackField.TryGet(out IJsonArray? stack))
            {
                if (stack[frameId].TryGet(out IJsonObject? frame))
                {
                    return WriteStackFrame(frame, prefix, output);
                }
            }
            return false;
        }

        private bool WriteStackFrame(IJsonObject frame, string prefix, System.IO.TextWriter output)
        {
            if (!frame.TryGet(MessageAttributes.StackFrameClass, out var stackFrameClassField) || !stackFrameClassField.TryGet(out string className) ||
                !frame.TryGet(MessageAttributes.StackFrameMethod, out var stackFrameMethodField) || !stackFrameMethodField.TryGet(out string methodName))
            {
                return false;
            }

            if (!frame.TryGet(MessageAttributes.StackFrameFile, out var fileNameField) || !fileNameField.TryGet(out string fileName))
            {
                fileName = "";
            }

            if (!frame.TryGet(MessageAttributes.StackFrameLine, out var stackFrameLineField) || !stackFrameLineField.TryGet(out long linePos))
            {
                linePos = 0;
            }

            output.Write(className);
            output.Write('.');
            output.Write(methodName);
            output.Write(" (");
            output.Write(fileName);
            output.Write('#');
            output.Write(linePos);
            output.Write(')');

            return true;
        }
    }
}