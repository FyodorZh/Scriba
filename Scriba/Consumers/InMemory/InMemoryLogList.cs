using System;

namespace Scriba.Consumers
{
    public class InMemoryLogList
    {
        private int _capacity;
        private LogEntity[] _queue;

        private readonly System.IO.StringWriter _buffer = new ();

        private int mPosition;
        private int mCount;

        public InMemoryLogList()
            : this(0)
        {
        }

        public InMemoryLogList(int capacity)
        {
            _queue = Array.Empty<LogEntity>(); // tmp
            Reinit(capacity);
        }

        public void Reinit(int capacity)
        {
            _capacity = capacity;
            _queue = new LogEntity[capacity];
            mPosition = 0;
            mCount = 0;
        }

        public int Count => mCount;

        public void Append(MessageData logMessage)
        {
            if (mCount == _capacity)
            {
                Dequeue();
            }

            string message;
            string[] stack;

            lock (_buffer)
            {
                logMessage.WriteMessageTo(_buffer);
                message = _buffer.ToString();
                _buffer.GetStringBuilder().Length = 0;

                int stackDepth = logMessage.StackTraceDepth;
                stack = new string[stackDepth];
                for (int i = 0; i < stackDepth; ++i)
                {
                    logMessage.WriteStackFrame(i, "", _buffer);
                    stack[i] = _buffer.ToString();
                    _buffer.GetStringBuilder().Length = 0;
                }
            }
            Enqueue(new LogEntity(Now(), logMessage.Severity, message, stack));
        }

        public LogEntity At(int id)
        {
            return _queue[(mPosition + id) % _capacity];
        }

        private void Dequeue()
        {
            mPosition += 1;
            mCount -= 1;
        }

        private void Enqueue(LogEntity entity)
        {
            _queue[(mPosition + mCount) % _capacity] = entity;
            mCount += 1;
        }

        protected virtual DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }
}