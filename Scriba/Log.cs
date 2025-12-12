using System;
using System.Collections.Generic;

namespace Scriba
{
    public static class Log
    {
        private static readonly IContext _globalContext = new Context();

        [ThreadStatic] private static List<IContext>? _contextStack;

        public static ILoggerContext PushContext(ILoggerContext context)
        {
            IContext ctx = context as IContext ?? new Context();
            _contextStack ??= new List<IContext>();
            _contextStack.Add(ctx);

            return ctx;
        }

        public static ILoggerContext? PopContext()
        {
            List<IContext>? stack = _contextStack;
            if (stack != null && stack.Count > 0)
            {
                IContext ctx = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                return ctx;
            }

            return null;
        }

        private static IContext? StackHead()
        {
            List<IContext>? stack = _contextStack;
            if (stack != null && stack.Count > 0)
            {
                return stack[stack.Count - 1];
            }

            return null;
        }

        private static IContext ActiveContext => StackHead() ?? _globalContext;

        public static void AddConsumer(ILogConsumer logConsumer)
        {
            IContext context = ActiveContext;
            context.AddConsumer(logConsumer);
        }

        public static void RemoveConsumer(ILogConsumer logConsumer)
        {
            IContext context = ActiveContext;
            context.RemoveConsumer(logConsumer);
        }

        public static void RemoveConsumerByType(Type type)
        {
            IContext context = ActiveContext;
            context.RemoveConsumerByType(type);
        }

        public static Severity LogFor
        {
            get => ActiveContext.LogFor;
            set => ActiveContext.LogFor = value;
        }

        /// <summary>
        /// Не логировать колстеки для мессаджей с таким или меньшим приоритетом
        /// </summary>
        public static Severity IgnoreStackFor
        {
            get => ActiveContext.IgnoreStackFor;
            set => ActiveContext.IgnoreStackFor = value;
        }

        public static string AppId
        {
            get => ActiveContext.AppId;
            set => ActiveContext.AppId = value;
        }

        public static string MachineName
        {
            get => ActiveContext.MachineName;
            set => ActiveContext.MachineName = value;
        }

        public static ITagList Tags => ActiveContext.Tags;

        public static void d(string format, params object[] args)
        {
            Message(Severity.DEBUG, format, args);
        }

        public static void i(string format, params object[] args)
        {
            Message(Severity.INFO, format, args);
        }

        public static void w(string format, params object[] args)
        {
            Message(Severity.WARN, format, args);
        }

        public static void e(string format, params object[] args)
        {
            Message(Severity.ERROR, format, args);
        }

        public static void wtf(string message, Exception exception)
        {
            Message(Severity.ERROR, "Exception ({text}): {exception}", message, exception.ToString());
        }

        public static void wtf(Exception exception)
        {
            Message(Severity.ERROR, "Exception : {exception}", exception.ToString());
        }

        public static void json(JsonFactory.IJsonObject message)
        {
            IContext context = ActiveContext;
            context.Message(message);
        }

        private static void Message(Severity severity, string format, params object[] args)
        {
            IContext context = ActiveContext;
            context.Message(LogMessageBuilder.Build(severity, context.IgnoreStackFor, format, args));
        }
    }
}