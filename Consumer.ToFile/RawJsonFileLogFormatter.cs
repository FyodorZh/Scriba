using Scriba;

namespace LogConsumers
{
    public class RawJsonFileLogFormatter : ILogFormatter
    {
        public void Format(MessageData logMessage, CharBuffer buffer)
        {
            logMessage.Data.Serialize(buffer);
            buffer.WriteLine();
        }
    }
}