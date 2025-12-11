using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Scriba
{
    public static class Log
    {
        private static readonly Object mLocker = new Object();
        private static readonly Context mGlobalContext = new Context();

        [ThreadStatic] private static List<Context>? mContextStack;

        public static IContext PushContext(IContext context)
        {
            Context ctx = context as Context ?? new Context();
            mContextStack ??= new List<Context>();
            mContextStack.Add(ctx);

            return ctx;
        }

        public static IContext? PopContext()
        {
            List<Context>? stack = mContextStack;
            if (stack != null && stack.Count > 0)
            {
                Context ctx = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                return ctx;
            }

            return null;
        }

        private static Context? StackHead()
        {
            List<Context>? stack = mContextStack;
            if (stack != null && stack.Count > 0)
            {
                return stack[stack.Count - 1];
            }

            return null;
        }

        private static Context ActiveContext => StackHead() ?? mGlobalContext;

        public static void AddConsumer(ILogConsumer logConsumer)
        {
            Context? context = StackHead();
            if (context != null)
            {
                context.AddConsumer(logConsumer);
            }
            else
            {
                lock (mLocker)
                {
                    mGlobalContext.AddConsumer(logConsumer);
                }
            }
        }

        public static void RemoveConsumer(ILogConsumer logConsumer)
        {
            Context? context = StackHead();
            if (context != null)
            {
                context.RemoveConsumer(logConsumer);
            }
            else
            {
                lock (mLocker)
                {
                    mGlobalContext.RemoveConsumer(logConsumer);
                }
            }
        }

        public static void RemoveConsumerByType(Type type)
        {
            Context? context = StackHead();
            if (context != null)
            {
                context.RemoveConsumerByType(type);
            }
            else
            {
                lock (mLocker)
                {
                    mGlobalContext.RemoveConsumerByType(type);
                }
            }
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

        public static TagList Tags => ActiveContext.Tags;

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
            Context? context = StackHead();
            if (context != null)
            {
                context.Message(message);
            }
            else
            {
                lock (mLocker)
                {
                    mGlobalContext.Message(message);
                }
            }
        }

        private static void Message(Severity severity, string format, params object[] args)
        {
            Context? context = StackHead();
            if (context != null && severity <= context.LogFor)
            {
                context.Message(LogMessageBuilder.Build(severity, context.IgnoreStackFor, format, args));
            }
            else if (severity <= mGlobalContext.LogFor)
            {
                var message = LogMessageBuilder.Build(severity, mGlobalContext.IgnoreStackFor, format, args);

                lock (mLocker)
                {
                    mGlobalContext.Message(message);
                }
            }
        }
    }
}