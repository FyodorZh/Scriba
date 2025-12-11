using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Scriba.JsonFactory
{
    public interface IJsonObject : IDisposable
    {
        bool AddElement(string name, string value);
        bool AddElement(string name, string format, params object[] list);
        bool AddElement(string name, double value);
        bool AddElement(string name, bool value);
        bool AddElement(string name, long value);
        bool AddElement(string name, IExternalJson value);
        IJsonObject AddObject(string name);
        IJsonArray AddArray(string name);

        int Count { get; }
        (string Name, JsonElement Filed) this[int id] { get; }
        bool TryGet(string name, out JsonElement filed);
    }

    public sealed class JsonObject : IJsonObject
    {
        public static IJsonObject Construct()
        {
            return Pool<JsonObject>.New();
        }

        private readonly List<(string Name, JsonElement Filed)> _elements = new ();

        public void Free()
        {
            for (int i = 0; i < _elements.Count; ++i)
            {
                _elements[i].Filed.Free();
            }
            _elements.Clear();
            Pool<JsonObject>.Free(this);
        }

        IJsonArray IJsonObject.AddArray(string name)
        {
            if (CheckName(name) && TryFind(name) < 0)
            {
                JsonArray array = Pool<JsonArray>.New();
                _elements.Add((name, new JsonElement(array)));
                return array;
            }
            throw new InvalidOperationException(name);
        }

        bool IJsonObject.AddElement(string name, string value)
        {
            if (CheckName(name) && TryFind(name) < 0)
            {
                _elements.Add((name, new JsonElement(value)));
                return true;
            }
            return false;
        }

        bool IJsonObject.AddElement(string name, string format, params object[] list)
        {
            if (CheckName(name) && TryFind(name) < 0)
            {
                _elements.Add((name, new JsonElement(format, list)));
                return true;
            }
            return false;
        }

        bool IJsonObject.AddElement(string name, double value)
        {
            if (CheckName(name) && TryFind(name) < 0)
            {
                _elements.Add((name, new JsonElement(value)));
                return true;
            }
            return false;
        }

        bool IJsonObject.AddElement(string name, bool value)
        {
            if (CheckName(name) && TryFind(name) < 0)
            {
                _elements.Add((name, new JsonElement(value)));
                return true;
            }
            return false;
        }

        bool IJsonObject.AddElement(string name, long value)
        {
            if (CheckName(name) && TryFind(name) < 0)
            {
                _elements.Add((name, new JsonElement(value)));
                return true;
            }
            return false;
        }

        bool IJsonObject.AddElement(string name, IExternalJson value)
        {
            if (CheckName(name) && TryFind(name) < 0)
            {
                _elements.Add((name, new JsonElement(value)));
                return true;
            }
            return false;
        }

        IJsonObject IJsonObject.AddObject(string name)
        {
            if (CheckName(name) && TryFind(name) < 0)
            {
                JsonObject obj = Pool<JsonObject>.New();
                _elements.Add((name, new JsonElement(obj)));
                return obj;
            }
            throw new InvalidOperationException(name);
        }

        void IDisposable.Dispose()
        {
            Free();
        }

        int IJsonObject.Count => _elements.Count;

        (string Name, JsonElement Filed) IJsonObject.this[int id] => _elements[id];

        bool IJsonObject.TryGet(string name, out JsonElement field)
        {
            int id = TryFind(name);
            if (id < 0)
            {
                field = default;
                return false;
            }
            field = _elements[id].Filed;
            return true;
        }

        private bool CheckName(string name)
        {
            return !name.Contains("\"");
        }

        private int TryFind(string name)
        {
            for (int i = 0; i < _elements.Count; ++i)
            {
                if (_elements[i].Name == name)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
