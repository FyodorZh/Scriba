using Scriba.JsonFactory;

namespace Scriba
{
    public struct MessageData
    {
        public IJsonObject Data { get; }
        
        internal MessageData(IJsonObject data)
        {
            Data = data;
        }

        public string Severity
        {
            get
            {
                if (!Data.Get(MessageAttributes.Severity).TryGet(out string severity))
                {
                    severity = "UNKNOWN";
                }
                return severity;
            }
        }

        public string Time
        {
            get
            {
                if (!Data.Get(MessageAttributes.Time).TryGet(out string time))
                {
                    time = "UNKNOWN";
                }
                return time;
            }
        }

        public bool WriteTimeTo(System.IO.TextWriter output)
        {
            return Data.Get(MessageAttributes.Time).WriteTo(output);
        }

        public bool WriteMessageTo(System.IO.TextWriter output)
        {
            if (Data.Get(MessageAttributes.Tags).TryGet(out IJsonArray tags))
            {
                int count = tags.Count;
                for (int i = count - 1; i >= 0; --i)
                {
                    var element = tags[i];

                    if (element.TryGet(out IJsonObject tagPair))
                    {
                        int pairs = tagPair.Count;
                        for (int j = 0; j < pairs; ++j)
                        {
                            (string Name, JsonElement Field) kv = tagPair[j];
                            if (kv.Field.TryGet(out string value))
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
                        output.Write(tag);
                        output.Write("; ");
                    }
                }
            }
            return Data.Get(MessageAttributes.Message).WriteTo(output, false);
        }

        public int StackTraceDepth
        {
            get
            {
                if (Data.Get(MessageAttributes.Stack).TryGet(out IJsonArray stack))
                {
                    return stack.Count;
                }
                return 0;
            }
        }

        public bool WriteStackTrace(string prefix, System.IO.TextWriter output)
        {
            if (Data.Get(MessageAttributes.Stack).TryGet(out IJsonArray stack))
            {
                int cnt = stack.Count;
                for (int i = 0; i < cnt; ++i)
                {
                    if (!stack[i].TryGet(out IJsonObject frame) || !WriteStackFrame(frame, prefix, output))
                    {
                        return false;
                    }
                    output.WriteLine();
                }
            }
            return false;
        }

        public bool WriteStackFrame(int frameId, string prefix, System.IO.TextWriter output)
        {
            if (Data.Get(MessageAttributes.Stack).TryGet(out IJsonArray stack))
            {
                if (stack[frameId].TryGet(out IJsonObject frame))
                {
                    return WriteStackFrame(frame, prefix, output);
                }
            }
            return false;
        }

        private bool WriteStackFrame(IJsonObject frame, string prefix, System.IO.TextWriter output)
        {
            string className;
            string methodName;
            string fileName;
            long linePos;

            if (!frame.Get(MessageAttributes.StackFrameClass).TryGet(out className) ||
                !frame.Get(MessageAttributes.StackFrameMethod).TryGet(out methodName))
            {
                return false;
            }

            if (!frame.Get(MessageAttributes.StackFrameFile).TryGet(out fileName))
            {
                fileName = "";
            }

            if (!frame.Get(MessageAttributes.StackFrameLine).TryGet(out linePos))
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