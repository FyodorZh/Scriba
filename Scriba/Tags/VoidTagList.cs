using Scriba.JsonFactory;

namespace Scriba
{
    public class VoidTagList : ITagList
    {
        public static readonly VoidTagList Instance = new VoidTagList();

        public bool IsEmpty => true;

        public bool Add(string tag, string? value = null)
        {
            return false;
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