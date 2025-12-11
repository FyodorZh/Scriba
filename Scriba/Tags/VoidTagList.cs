namespace Scriba
{
    public class VoidTagList : ITagList
    {
        public static readonly VoidTagList Instance = new VoidTagList();
        
        public bool Add(string tag, string? value = null)
        {
            return false;
        }

        public bool Remove(string tag)
        {
            return false;
        }
    }
}