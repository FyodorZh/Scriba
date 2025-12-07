#define DEBUG

using System.Diagnostics;
namespace Scriba.Consumers
{
    public class StudioDebugConsumer : ILogConsumer
    {
        public static StudioDebugConsumer Instance { get; } = new();
        
        private readonly System.IO.StringWriter mBuffer = new ();

        public void Message(MessageData logMessage)
        {
            string text;
            lock (mBuffer)
            {
                mBuffer.Write(logMessage.Severity);
                mBuffer.Write(": ");
                logMessage.WriteMessageTo(mBuffer);

                text = mBuffer.ToString();
                mBuffer.GetStringBuilder().Length = 0;
            }

            Debug.WriteLine(text);
        }

        void ILogConsumer.AddRef()
        {
            // DO NOTHING
        }

        void ILogConsumer.Release()
        {
            // DO NOTHING
        }
    }
}
