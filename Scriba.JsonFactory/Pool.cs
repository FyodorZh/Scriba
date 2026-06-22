using System;
using System.Collections.Generic;

namespace Scriba.JsonFactory
{
    /// <summary>
    /// Provides a two-tier object pool (thread-local + global) for reusable
    /// <typeparamref name="TObject"/> instances, reducing allocation pressure.
    /// </summary>
    /// <typeparam name="TObject">The type of objects to pool. Must be a reference type with a parameterless constructor.</typeparam>
    internal static class Pool<TObject>
        where TObject : class, new()
    {
        private const int MaxGlobalPopulationSize = 1000;
        private const int HalfPopulationSize = 50;

        private static readonly Stack<TObject> _globalStack = new ();

        [ThreadStatic]
        private static Stack<TObject>? _stack;

        public static TObject New()
        {
            var stack = _stack;
            if (stack == null)
            {
                stack = new Stack<TObject>();
                _stack = stack;
            }

            if (stack.Count == 0)
            {
                Populate(stack);
            }

            return stack.Pop();
        }

        public static void Free(TObject obj)
        {
            var stack = _stack;
            if (stack == null)
            {
                stack = new Stack<TObject>();
                _stack = stack;
            }

            stack.Push(obj);
            if (stack.Count >= HalfPopulationSize * 2)
            {
                Depopulate(stack);
            }
        }

        private static void Populate(Stack<TObject> dst)
        {
            int fromGlobal;
            lock (_globalStack)
            {
                fromGlobal = Math.Min(HalfPopulationSize, _globalStack.Count);
                for (int i = 0; i < fromGlobal; ++i)
                    dst.Push(_globalStack.Pop());
            }
            for (int i = fromGlobal; i < HalfPopulationSize; ++i)
                dst.Push(new TObject());
        }

        private static void Depopulate(Stack<TObject> src)
        {
            lock (_globalStack)
            {
                int count = Math.Min(HalfPopulationSize, src.Count);
                for (int i = 0; i < count; ++i)
                {
                    if (_globalStack.Count < MaxGlobalPopulationSize)
                    {
                        _globalStack.Push(src.Pop());
                    }
                }
            }
        }
    }
}
