using System.IO;

namespace Scriba
{
    public interface ILogFormatter
    {
        void Format(MessageData logMessage, TextWriter dst);
    }
}