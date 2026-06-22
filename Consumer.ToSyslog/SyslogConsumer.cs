using SyslogNet.Client;
using SyslogNet.Client.Serialization;
using SyslogNet.Client.Transport;
using System;

namespace Scriba.Consumers.ToSyslog
{
    public class SyslogConsumer : ILogConsumer
    {
        private const int MAX_TEXT_LENGTH = 512;
        private SyslogUdpSender sender = null;
        private ISyslogMessageSerializer serializer = null;
        private readonly string machineName;
        private string appName = "app";
        private static readonly char[] separators = { '\r', '\n' };

        private readonly System.IO.StringWriter mBuffer = new System.IO.StringWriter();

        private int mRefCount = 0;

        public SyslogConsumer(string ip, int port)
        {
            try
            {
                mRefCount = 1;
                sender = new SyslogUdpSender(ip, port);
                serializer = new SyslogRfc3164MessageSerializer();

                machineName = Environment.MachineName;

                try
                {
                    System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                    foreach (System.Net.IPAddress address in host.AddressList)
                    {
                        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            machineName += "-" + address;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.wtf(e);
                }
            }
            catch (Exception e)
            {
                Log.wtf(e);

                sender = null;
                serializer = null;
                machineName = "";
            }
        }

        public bool IsOptional
        {
            get
            {
                return sender == null;
            }
        }

        public void Message(MessageData logMessage)
        {
            if (sender != null && serializer != null)
            {
                SyslogNet.Client.Severity severity;
                
                var logSeverity = logMessage.Severity;
                switch (logSeverity)
                {
                    case Severity.DEBUG:
                        severity = SyslogNet.Client.Severity.Debug;
                        break;
                    case Severity.INFO:
                        severity = SyslogNet.Client.Severity.Informational;
                        break;
                    case Severity.WARN:
                        severity = SyslogNet.Client.Severity.Warning;
                        break;
                    case Severity.ERROR:
                        severity = SyslogNet.Client.Severity.Error;
                        break;
                    case Severity.FATAL:
                        severity = SyslogNet.Client.Severity.Critical;
                        break;
                    default:
                        severity = SyslogNet.Client.Severity.Error;
                        break;
                }

                string message;
                lock (mBuffer)
                {
                    logMessage.WriteMessageTo(mBuffer);
                    message = mBuffer.ToString();
                    mBuffer.GetStringBuilder().Length = 0;
                }
                
                try
                {
                    string[] lines = message.Split(separators);

                    int iLine = 0;
                    foreach (var t in lines)
                    {
                        string str = t.Trim();
                        if (string.IsNullOrEmpty(str))
                            continue;
                        for (int pos = 0; pos < str.Length; pos += MAX_TEXT_LENGTH)
                        {
                            string strP = (lines.Length > 1) ? $"({logSeverity.ToString()}{iLine}) " : string.Empty;
                            string strOut = $"{strP}{message}:{str.Substring(pos, Math.Min(MAX_TEXT_LENGTH, str.Length - pos))}";

                            var msg = new SyslogMessage(DateTimeOffset.Now, Facility.LocalUse0, severity, machineName, appName, strOut);
                            sender.Send(msg, serializer);
                            iLine++;
                        }
                    }

                    //for (int i = 0; i < stack.Length; ++i)
                    //{
                    //    string stackLine = stack[i];
                    //    if (stackLine.Length > MAX_TEXT_LENGTH)
                    //        stackLine = stackLine.Substring(0, MAX_TEXT_LENGTH);
                    //    msg = new SyslogMessage(DateTimeOffset.Now, Facility.LocalUse0, Severity.Debug, machineName, appName, stackLine);
                    //    sender.Send(msg, serializer);
                    //}
                }
                catch (Exception e)
                {
                    sender = null;
                    serializer = null;
                    Log.RemoveConsumer(this);
                    Log.wtf(e);
                }
            }
        }

        void ILogConsumer.AddRef()
        {
            ++mRefCount;
        }

        void ILogConsumer.Release()
        {
            if (--mRefCount == 0)
            {
                if (sender != null)
                {
                    sender.Dispose();
                    sender = null;
                }
            }
        }
    }
}