using System;
using Actuarius.Collections;
using Actuarius.Memory;
using Operarius;
using Scriba.Consumers.ToFile;

namespace LogConsumers
{
    public class FileLogConsumer : BaseFileLogConsumer
    {
        public static readonly ConcurrentPool<BytesBuffer> DefaultBytesPool =
            new ConcurrentPool<BytesBuffer>(new LambdaConstructor<BytesBuffer>(() => new BytesBuffer(new FileLogFormatter())), new TinyConcurrentQueue<BytesBuffer>());

        public FileLogConsumer(IPeriodicLogicRunner logicRunner, DeltaTime flushInterval, string fileName, IPool<BytesBuffer> bytesBufferPool)
            : base(logicRunner, flushInterval, fileName, bytesBufferPool)
        {
        }

        public FileLogConsumer(IPeriodicLogicRunner logicRunner,
            DeltaTime flushInterval,
            string fileName,
            long maxSize,
            bool splitByDate,
            bool splitByRun,
            IPool<BytesBuffer> bytesBufferPool,
            Action<Exception> onError = null)
            : base(logicRunner, flushInterval, fileName, maxSize, splitByDate, splitByRun, bytesBufferPool, onError)
        {
        }
    }
}