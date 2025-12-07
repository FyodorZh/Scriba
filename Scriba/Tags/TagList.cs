using System.Collections.Generic;

namespace Scriba
{
    public class TagList : ITagList
    {
        private readonly List<TagElement> mList = new List<TagElement>();

        public bool Add(string tag, string value = null)
        {
            if (mList.FindIndex(el => el.Tag == tag) >= 0)
            {
                return false;
            }
            mList.Add(new TagElement(tag, value));
            return true;
        }

        public bool Remove(string tag)
        {
            return mList.RemoveAll(el => el.Tag == tag) > 0;
        }

        internal void WriteTo(JsonFactory.IJsonArray tags)
        {
            int count = mList.Count;
            for (int i = 0; i < count; ++i)
            {
                var tag = mList[i].Tag;
                var value = mList[i].Value;

                if (value == null)
                {
                    tags.AddElement(tag);
                }
                else
                {
                    var element = tags.AddObject();
                    element.AddElement(tag, value);
                }
            }
        }
    }
}
