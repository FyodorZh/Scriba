using System.Collections.Generic;
using System.Threading;

namespace Scriba
{
    public class TagList : ITagList
    {
        private readonly List<TagElement> _tags = new List<TagElement>();
        private readonly ReaderWriterLockSlim _locker = new ();

        public bool Add(string tag, string? value = null)
        {
            _locker.EnterWriteLock();
            try
            {
                if (_tags.FindIndex(el => el.Tag == tag) >= 0)
                {
                    return false;
                }

                _tags.Add(new TagElement(tag, value));
                return true;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public bool Remove(string tag)
        {
            _locker.EnterWriteLock();
            try
            {
                return _tags.RemoveAll(el => el.Tag == tag) > 0;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        internal void WriteTo(JsonFactory.IJsonArray tags)
        {
            _locker.EnterReadLock();
            try
            {
                int count = _tags.Count;
                for (int i = 0; i < count; ++i)
                {
                    var tag = _tags[i].Tag;
                    var value = _tags[i].Value;

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
            finally
            {
                _locker.ExitReadLock();
            }
        }
    }
}
