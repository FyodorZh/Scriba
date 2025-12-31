using System;
using System.Collections.Generic;
using System.Threading;

namespace Scriba
{
    public class TagList : ITagList
    {
        private readonly List<TagElement> _tags = new List<TagElement>();
        private readonly ReaderWriterLockSlim _locker = new ();

        public bool IsEmpty { get; private set; } = true;

        private void Set(TagElement tagElement)
        {
            _locker.EnterWriteLock();
            try
            {
                for (int i = 0; i < _tags.Count; ++i)
                {
                    if (_tags[i].Tag == tagElement.Tag)
                    {
                        _tags[i] = tagElement;
                        return;
                    }
                }
                _tags.Add(tagElement);
                IsEmpty = false;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void Set(string tag, string? value)
        {
            Set(value != null ? new TagElement(tag, value) : new TagElement(tag));
        }

        public void Set(string tag, Func<string> valueFactory)
        {
            Set(new TagElement(tag, valueFactory));
        }

        public bool Remove(string tag)
        {
            _locker.EnterWriteLock();
            try
            {
                bool res = _tags.RemoveAll(el => el.Tag == tag) > 0;
                IsEmpty = _tags.Count == 0;
                return res;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void WriteTo(JsonFactory.IJsonArray tags)
        {
            _locker.EnterReadLock();
            try
            {
                int count = _tags.Count;
                for (int i = 0; i < count; ++i)
                {
                    var tagElement = _tags[i];
                    string tag = tagElement.Tag;
                    string? value = tagElement.Value ?? tagElement.ValueFactory?.Invoke();

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
