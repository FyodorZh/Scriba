using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Scriba.Consumers.Logcat
{
    public class LogcatConsumer : MultiRefLogConsumer
    {
        private const string DefaultTag = "App";

        private static readonly bool _androidAvailable;
        private static readonly int[] _priorityMap;

        private readonly string _tag;
        private readonly StringWriter _buffer = new StringWriter();

        static LogcatConsumer()
        {
            try
            {
                __android_log_write(0, null!, null!);
                _androidAvailable = true;
            }
            catch (DllNotFoundException)
            {
                _androidAvailable = false;
            }
            catch (EntryPointNotFoundException)
            {
                _androidAvailable = false;
            }

            _priorityMap = new int[6];
            _priorityMap[(int)Severity.UNKNOWN] = 3;
            _priorityMap[(int)Severity.DEBUG] = 3;
            _priorityMap[(int)Severity.INFO] = 4;
            _priorityMap[(int)Severity.WARN] = 5;
            _priorityMap[(int)Severity.ERROR] = 6;
            _priorityMap[(int)Severity.FATAL] = 7;
        }

        public LogcatConsumer() 
            : this(DefaultTag)
        {
        }

        public LogcatConsumer(string tag)
        {
            _tag = tag;
        }

        public override void Message(MessageData logMessage)
        {
            if (!_androidAvailable)
                return;

            string text;
            lock (_buffer)
            {
                logMessage.WriteMessageTo(_buffer);

                if (logMessage.StackTraceDepth > 0)
                {
                    _buffer.WriteLine();
                    logMessage.WriteStackTrace("\t", _buffer);
                }

                text = _buffer.ToString();
                _buffer.GetStringBuilder().Length = 0;
            }

            int priority = _priorityMap[(int)logMessage.Severity];
            __android_log_write(priority, _tag, text);
        }

        [DllImport("liblog", EntryPoint = "__android_log_write")]
        private static extern int __android_log_write(int prio, string tag, string text);
    }
}
