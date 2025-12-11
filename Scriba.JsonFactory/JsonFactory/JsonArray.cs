using System;
using System.Collections.Generic;

namespace Scriba.JsonFactory
{
    public interface IJsonArray : IDisposable
    {
        void AddElement(string value);
        void AddElement(double value);
        void AddElement(bool value);
        void AddElement(long value);
        IJsonObject AddObject();
        IJsonArray AddArray();

        int Count { get; }
        JsonElement this[int id] { get; }
    }

    public sealed class JsonArray : IJsonArray
    {
        private readonly List<JsonElement> _elements = new List<JsonElement>();

        public void Free()
        {
            for (int i = 0; i < _elements.Count; ++i)
            {
                _elements[i].Free();
            }
            _elements.Clear();
            Pool<JsonArray>.Free(this);
        }

        IJsonArray IJsonArray.AddArray()
        {
            JsonArray array = Pool<JsonArray>.New();
            _elements.Add(new JsonElement(array));
            return array;
        }

        void IJsonArray.AddElement(string value)
        {
            _elements.Add(new JsonElement(value));
        }

        void IJsonArray.AddElement(double value)
        {
            _elements.Add(new JsonElement(value));
        }

        void IJsonArray.AddElement(bool value)
        {
            _elements.Add(new JsonElement(value));
        }

        void IJsonArray.AddElement(long value)
        {
            _elements.Add(new JsonElement(value));
        }

        IJsonObject IJsonArray.AddObject()
        {
            JsonObject obj = Pool<JsonObject>.New();
            _elements.Add(new JsonElement(obj));
            return obj;
        }

        void IDisposable.Dispose()
        {
            Free();
        }

        int IJsonArray.Count
        {
            get { return _elements.Count; }
        }

        JsonElement IJsonArray.this[int id]
        {
            get { return _elements[id]; }
        }
    }
}
