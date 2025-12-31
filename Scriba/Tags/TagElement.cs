using System;

namespace Scriba
{
    internal class TagElement
    {
        public string Tag { get; private set; }
        public string? Value { get; private set; }
        public Func<string>? ValueFactory { get; set; }

        public TagElement(string tag)
        {
            Tag = tag;
        }

        public TagElement(string tag, string value)
        {
            Tag = tag;
            Value = value;
        }
        
        public TagElement(string tag, Func<string> valueFactory)
        {
            Tag = tag;
            ValueFactory = valueFactory;
        }
    }
}