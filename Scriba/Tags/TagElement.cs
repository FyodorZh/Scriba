namespace Scriba
{
    internal class TagElement
    {
        public string Tag { get; private set; }
        public string? Value { get; private set; }

        public TagElement(string tag, string? value = null)
        {
            Tag = tag;
            Value = value;
        }
    }
}