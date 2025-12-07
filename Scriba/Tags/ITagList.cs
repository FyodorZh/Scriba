namespace Scriba
{
    public interface ITagList
    {
        bool Add(string tag, string value = null);
        bool Remove(string tag);
    }
}