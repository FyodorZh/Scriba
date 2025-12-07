namespace Scriba
{
    public interface ILogConsumer
    {
        void Message(MessageData logMessage);

        void AddRef();
        void Release();        
    }
}