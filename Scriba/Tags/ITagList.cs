using System;

namespace Scriba
{
    public interface ITagList
    {
        bool IsEmpty { get; }
        void Set(string tag, string? value = null);
        void Set(string tag, Func<string> valueFactory);
        bool Remove(string tag);
        void WriteTo(JsonFactory.IJsonArray tags);
    }
}