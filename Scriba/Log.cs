using System;
using System.Collections.Generic;

namespace Scriba
{
    public static class Log
    {
        private static readonly ILoggerExt _globalLogger = new Logger();

        [ThreadStatic] private static List<ILoggerExt>? _stack;

        public static ILogger PushThreadContextLogger(ILogger logger)
        {
            ILoggerExt ctx = (logger as ILoggerExt) ?? new Logger();
            _stack ??= new List<ILoggerExt>();
            _stack.Add(ctx);

            return ctx;
        }

        public static ILogger? PopThreadContextLogger()
        {
            List<ILoggerExt>? stack = _stack;
            if (stack != null && stack.Count > 0)
            {
                ILogger logger = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                return logger;
            }

            return null;
        }

        private static ILoggerExt? StackHead()
        {
            List<ILoggerExt>? stack = _stack;
            if (stack != null && stack.Count > 0)
            {
                return stack[stack.Count - 1];
            }

            return null;
        }

        private static ILoggerExt ActiveLogger => StackHead() ?? _globalLogger;

        public static void AddConsumer(ILogConsumer logConsumer)
        {
            ActiveLogger.AddConsumer(logConsumer);
        }

        public static void RemoveConsumer(ILogConsumer logConsumer)
        {
            ActiveLogger.RemoveConsumer(logConsumer);
        }

        public static void RemoveConsumerByType(Type type)
        {
            ActiveLogger.RemoveConsumerByType(type);
        }

        public static Severity LogFor
        {
            get => ActiveLogger.LogFor;
            set => ActiveLogger.LogFor = value;
        }

        /// <summary>
        /// Не логировать колстеки для мессаджей с таким или меньшим приоритетом
        /// </summary>
        public static Severity IgnoreStackFor
        {
            get => ActiveLogger.IgnoreStackFor;
            set => ActiveLogger.IgnoreStackFor = value;
        }

        public static string? AppId
        {
            get => ActiveLogger.AppId;
            set => ActiveLogger.AppId = value;
        }

        public static string? MachineName
        {
            get => ActiveLogger.MachineName;
            set => ActiveLogger.MachineName = value;
        }

        public static bool LogTime
        {
            get => ActiveLogger.LogTime;
            set => ActiveLogger.LogTime = value;
        }

        public static ITagList Tags => ActiveLogger.Tags;

        public static void d(string format, params object[] args)
        {
            ActiveLogger.d(format, args);
        }

        public static void i(string format, params object[] args)
        {
            ActiveLogger.i(format, args);
        }

        public static void w(string format, params object[] args)
        {
            ActiveLogger.w(format, args);
        }

        public static void e(string format, params object[] args)
        {
            ActiveLogger.e(format, args);
        }

        public static void wtf(string message, Exception exception)
        {
            ActiveLogger.wtf(message, exception);
        }

        public static void wtf(Exception exception)
        {
            ActiveLogger.wtf(exception);
        }

        public static void json(JsonFactory.IJsonObject message)
        {
            ActiveLogger.json(message);
        }

        internal static void Publish(MessageData message)
        {
            ActiveLogger.Publish(message);
        }
    }
}