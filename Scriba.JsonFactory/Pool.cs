using System.Collections.Generic;

namespace Scriba.JsonFactory
{
    internal static class Pool<TObject>
        where TObject : class, new()
    {
        [System.ThreadStatic]
        private static List<TObject>? _list;

        public static TObject New()
        {
            var list = _list;
            if (list == null)
            {
                list = new List<TObject>();
                _list = list;
            }

            if (list.Count > 0)
            {
                TObject res = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return res;
            }
            return new TObject();
        }

        public static void Free(TObject obj)
        {
            var list = _list;
            if (list == null)
            {
                list = new List<TObject>();
                _list = list;
            }

            if (list.Count < 1000)
            {
                list.Add(obj);
            }
        }
    }
}
