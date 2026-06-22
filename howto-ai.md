# Scriba — AI Developer Guide

## Overview

Scriba is a **zero-allocation structured logging framework for .NET** (`netstandard2.0` / `netstandard2.1`). It is designed for high-performance scenarios (games, servers, embedded) where GC pressure must be minimised.

Key design decisions:
- Log messages are built as **JSON objects** (using `Scriba.JsonFactory`, an allocation-free JSON builder with object pooling)
- Messages flow through a **consumer pipeline** — each `ILogConsumer` receives every message
- The **format string syntax** supports named parameters (e.g. `"Hello {name}"`) and auto-extracts them as JSON fields
- Tags are per-logger key-value metadata that get embedded into every message

---

## Table of Content

1. [Scriba (Core)](#1-scriba-core)
2. [Consumer.Syslog](#2-consumersyslog)
3. [Consumer.Logcat](#3-consumerlogcat)

---

# 1. Scriba (Core)

**Package:** `Scriba` · `netstandard2.0` · NuGet

The core logging library. It defines the logger interface, severity levels, log consumers, formatting, tags, and message data structures.

---

## `Severity` enum

```csharp
namespace Scriba;

public enum Severity : byte
{
    UNKNOWN = 0,
    FATAL   = 1,
    ERROR   = 2,
    WARN    = 3,
    INFO    = 4,
    DEBUG   = 5,
}
```

Use the extension method `.Serialize()` to convert a `Severity` value to its string name:

```csharp
Severity.DEBUG.Serialize(); // => "DEBUG"
```

---

## `ILogger` interface

```csharp
namespace Scriba;

public interface ILogger : IDisposable
{
    Severity LogFor          { get; set; }
    Severity IgnoreStackFor  { get; set; }
    string?  AppId           { get; set; }
    string?  MachineName     { get; set; }
    bool     LogTime         { get; set; }

    ITagList Tags { get; }

    void AddConsumer(ILogConsumer logConsumer);
    void RemoveConsumer(ILogConsumer logConsumer);
    void RemoveConsumerByType(Type type);

    void d(string format, params object[] args);
    void i(string format, params object[] args);
    void w(string format, params object[] args);
    void e(string format, params object[] args);

    void wtf(string message, Exception exception);
    void wtf(Exception exception);

    void json(JsonFactory.IJsonObject message);
}

public interface ILoggerExt : ILogger
{
    void Publish(MessageData message);
}
```

**Logging shortcut methods** (`d`, `i`, `w`, `e`):
- Accept a format string with `{0}`, `{1}` positional or `{name}` named placeholders
- Named placeholders become JSON field names in the log message
- The `msg` field in JSON contains the fully formatted message

```csharp
logger.e("Connection failed: {reason}", "timeout");
// JSON: {"severity": 2, "msg": "Connection failed: timeout", "reason": "timeout"}

logger.e("User {user} at {ip}", userName, clientIp);
// JSON: {"severity": 2, "msg": "User alice at 10.0.0.1", "0": userName, "1": clientIp}
```

---

## `ILogConsumer` interface

```csharp
namespace Scriba;

public interface ILogConsumer
{
    void Message(MessageData logMessage);
    void AddRef();
    void Release();
}
```

Implement this to route log messages anywhere (file, network, console, etc.).

---

## `MessageData` (readonly struct)

```csharp
namespace Scriba;

public readonly struct MessageData
{
    IJsonObject Data { get; }

    Severity Severity  { get; }
    DateTime?  Time    { get; }
    int StackTraceDepth { get; }

    bool WriteMessageTo(TextWriter output);
    bool WriteTagsTo(TextWriter output, Predicate<string>? tagsSelector = null);
    bool WriteStackTrace(string prefix, TextWriter output);
    bool WriteStackFrame(int frameId, string prefix, TextWriter output);
}
```

Has an internal constructor — you receive it from the logging pipeline, you do not create it.

---

## `ILogFormatter` interface

```csharp
namespace Scriba;

public interface ILogFormatter
{
    void Format(MessageData logMessage, TextWriter dst);
}
```

`SynchronizedLogFormatter` wraps a `Action<MessageData, TextWriter>` delegate:

```csharp
ILogFormatter formatter = new SynchronizedLogFormatter((msg, dst) =>
{
    dst.Write(msg.Severity);
    dst.Write(": ");
    msg.WriteMessageTo(dst);
});
```

---

## `ITagList` interface

```csharp
namespace Scriba;

public interface ITagList
{
    bool IsEmpty { get; }
    void Set(string tag, string? value = null);
    void Set(string tag, Func<string> valueFactory);
    bool Remove(string tag);
    void WriteTo(JsonFactory.IJsonArray tags);
}
```

Tags without a value are serialised as bare strings; tags with a value become `{"tag": "value"}` objects.

---

## `Logger` class

```csharp
namespace Scriba;

public class Logger : ILoggerExt
{
    public Logger();
    public Logger(IEnumerable<ILogConsumer> consumers);

    void AddConsumer(ILogConsumer);
    void RemoveConsumer(ILogConsumer);
    void RemoveConsumerByType(Type type);
    void Publish(MessageData);
    void Dispose();
}
```

---

## `LoggerWrapper` class

```csharp
namespace Scriba;

public class LoggerWrapper : Logger, ITagList
{
    // Created via extension method on ILogger:
    //   logger.Wrap()
    // Publishes to BOTH the wrapped logger AND the wrapper's base consumers.
    // Tags from both loggers are merged (wrapped first, then wrapper).
}

public static class LoggerWrapper_Ext
{
    public static ILogger Wrap(this ILogger logger);
    // Requires ILoggerExt — throws InvalidOperationException otherwise.
}
```

Usage:
```csharp
var baseLogger = new Logger();
baseLogger.AddConsumer(consoleConsumer);
var wrapper = baseLogger.Wrap();

wrapper.Tags.Set("module", "http");
wrapper.i("Request received");
```

---

## `StaticLogger` class

```csharp
namespace Scriba;

public class StaticLogger : ILoggerExt
{
    public static readonly StaticLogger Instance = new StaticLogger();
    // All members delegate to the static Log class.
}
```

---

## `VoidLogger` class

```csharp
namespace Scriba;

public class VoidLogger : ILogger
{
    public static readonly VoidLogger Instance = new VoidLogger();
    // All methods are no-ops. json() disposes the message object.
}
```

---

## `Log` static class

```csharp
namespace Scriba;

public static class Log
{
    static ILogger  PushThreadContextLogger(ILogger logger);
    static ILogger? PopThreadContextLogger();

    static void AddConsumer(ILogConsumer);
    static void RemoveConsumer(ILogConsumer);
    static void RemoveConsumerByType(Type type);
    static Severity LogFor { get; set; }
    static Severity IgnoreStackFor { get; set; }
    static string? AppId { get; set; }
    static string? MachineName { get; set; }
    static bool LogTime { get; set; }
    static ITagList Tags { get; }
    static void d(string format, params object[] args);
    static void i(string format, params object[] args);
    static void w(string format, params object[] args);
    static void e(string format, params object[] args);
    static void wtf(string message, Exception exception);
    static void wtf(Exception exception);
    static void json(JsonFactory.IJsonObject message);
}
```

Usage patterns:
```csharp
Log.i("Server started on port {port}", 8080);
Log.wtf(exception);

var ctxLogger = new Logger();
ctxLogger.Tags.Set("requestId", Guid.NewGuid().ToString());
Log.PushThreadContextLogger(ctxLogger);
try
{
    Log.i("Processing request");
}
finally
{
    Log.PopThreadContextLogger();
}

Log.AddConsumer(new ConsoleConsumer());
```

---

## Built-in Consumers

### `ConsoleConsumer` (`Scriba.Consumers.ConsoleConsumer`)

```csharp
namespace Scriba.Consumers;

public class ConsoleConsumer : MultiRefLogConsumer
{
    ILogFormatter Formatter { get; set; }
}
```

### `SimpleFileConsumer`

```csharp
namespace Scriba;

public class SimpleFileConsumer : MultiRefLogConsumer
{
    ILogFormatter Formatter { get; set; }

    public SimpleFileConsumer(string fileName);
}
```

### `MultiRefLogConsumer` (abstract base)

```csharp
namespace Scriba;

public abstract class MultiRefLogConsumer : ILogConsumer
{
    void AddRef();
    void Release();
    protected virtual void Dispose();
    public abstract void Message(MessageData logMessage);
}
```

### `InMemoryLogConsumer` (test helper, `Scriba.Consumers`)

```csharp
namespace Scriba.Consumers;

public class InMemoryLogConsumer : ILogConsumer
{
    string[] TakeAll();
}
```

---

## Example: Full setup

```csharp
using Scriba;
using Scriba.Consumers;
using Scriba.JsonFactory;

var logger = new Logger
{
    LogFor = Severity.DEBUG,
    IgnoreStackFor = Severity.ERROR,
    AppId = "MyApp",
    MachineName = Environment.MachineName,
    LogTime = true
};

logger.AddConsumer(new ConsoleConsumer());
logger.AddConsumer(new SimpleFileConsumer("app.log"));

logger.Tags.Set("env", "production");
logger.Tags.Set("version", () => GetVersion());

logger.i("User {user} logged in from {ip}", "alice", "10.0.0.1");
logger.wtf(new InvalidOperationException("Something went wrong"));

logger.Dispose();
```

---

# 2. Consumer.Syslog

**Package:** `Scriba.Consumer.Syslog` · `netstandard2.0` · NuGet

---

## `SyslogConsumer` class

```csharp
namespace Scriba.Consumers.Syslog;

public class SyslogConsumer : ILogConsumer
{
    public SyslogConsumer(string ip, int port);

    bool IsOptional { get; }

    void Message(MessageData logMessage);
    void AddRef();
    void Release();
}
```

```csharp
var logger = new Logger();
logger.AddConsumer(new SyslogConsumer("192.168.1.100", 514));
logger.i("Server started");
```

---

# 3. Consumer.Logcat

**Package:** `Scriba.Consumer.Logcat` · `netstandard2.1` · NuGet

---

## `LogcatConsumer` class

```csharp
namespace Scriba.Consumers.Logcat;

public class LogcatConsumer : MultiRefLogConsumer
{
    public LogcatConsumer();            // Tag = "App"
    public LogcatConsumer(string tag);  // Custom tag

    public override void Message(MessageData logMessage);
}
```

```csharp
var logger = new Logger();
logger.AddConsumer(new LogcatConsumer("MyApp"));
logger.i("Hello from Scriba on Android!");
```

---

# Common Patterns

## Named parameter format syntax

```csharp
logger.e("Error {0} at {1}", errorCode, location);
logger.e("Error {code} at {location}", code, location);
logger.e("Error {code} at {1}", code, location);
```

## Custom consumer

```csharp
public class MyConsumer : MultiRefLogConsumer
{
    public override void Message(MessageData logMessage)
    {
        logMessage.WriteMessageTo(Console.Out);
        Console.Out.WriteLine();
    }
}
```

## Custom formatter

```csharp
var formatter = new SynchronizedLogFormatter((msg, dst) =>
{
    dst.Write("[");
    dst.Write(msg.Severity.Serialize());
    dst.Write("] ");
    msg.WriteMessageTo(dst);
});
```

## JSON logging with IJsonObject

```csharp
IJsonObject payload = JsonObject.Construct();
payload.AddElement("event", "purchase");
payload.AddElement("amount", 29.99);
payload.AddElement("currency", "USD");

logger.json(payload);
```
