using System;
using Scriba.JsonFactory;

namespace Scriba
{
    public class VoidTagList : ITagList
    {
        public static readonly VoidTagList Instance = new VoidTagList();

        public bool IsEmpty => true;

        public void Set(string tag, string? value = null)
        {
            // DO NOTHING
        }

        public void Set(string tag, Func<string> valueFactory)
        {
            // DO NOTHING
        }

        public bool Remove(string tag)
        {
            return false;
        }

        public void WriteTo(IJsonArray tags)
        {
            // DO NOTHING
        }
    }
}