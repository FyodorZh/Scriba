namespace Scriba
{
    public interface ITagList
    {
        bool IsEmpty { get; }
        bool Add(string tag, string? value = null);
        bool Remove(string tag);
        void WriteTo(JsonFactory.IJsonArray tags);
    }
}