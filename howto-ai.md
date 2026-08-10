{
  "library": "Scriba",
  "meta": {
    "target": "netstandard2.0",
    "lang": 9,
    "nullable": true
  },
  "philosophy": "Scriba is a zero-allocation structured logging framework for .NET; it builds every log message as a JSON object using the bundled Scriba.JsonFactory pooled JSON model and delivers it to a pipeline of reference-counted ILogConsumer instances. The design ethos is minimal GC pressure: JsonObject and JsonArray instances are recycled through internal pools, formatted strings are composed lazily at serialisation time, and message construction is skipped entirely when the severity is above LogFor. Naming is terse and Android-inspired: d/i/w/e for debug/info/warn/error, wtf for exceptions, json for structured payloads, and a static Log facade for ambient logging.",
  "quickPick": [
    {
      "need": "Attach key-value metadata to every message from a logger",
      "use": "TagList"
    },
    {
      "need": "Build a structured JSON payload to attach to a log message",
      "use": "JsonObject"
    },
    {
      "need": "Create a logger that duplicates messages to another logger",
      "use": "LoggerWrapper"
    },
    {
      "need": "Disable logging without touching call sites",
      "use": "VoidLogger"
    },
    {
      "need": "Log a simple text message at any level",
      "use": "Logger"
    },
    {
      "need": "Log from anywhere without an instance",
      "use": "Log"
    },
    {
      "need": "Provide raw JSON that serialises into a log message",
      "use": "IExternalJson"
    },
    {
      "need": "Send log output to a file",
      "use": "SimpleFileConsumer"
    },
    {
      "need": "Send log output to the console",
      "use": "ConsoleConsumer"
    },
    {
      "need": "Use a DI singleton that delegates to the static Log class",
      "use": "StaticLogger"
    }
  ],
  "decisionTree": [
    [
      "How do you obtain a logger?",
      "static ambient access",
      "Log"
    ],
    [
      "How do you obtain a logger?",
      "configure a Logger instance",
      "Logger"
    ],
    [
      "How do you obtain a logger?",
      "DI singleton around static Log",
      "StaticLogger"
    ],
    [
      "How do you obtain a logger?",
      "no-op placeholder",
      "VoidLogger"
    ],
    [
      "What payload type do you log?",
      "external JSON provider",
      "IExternalJson"
    ],
    [
      "What payload type do you log?",
      "plain text",
      "ILogger"
    ],
    [
      "What payload type do you log?",
      "structured JSON",
      "JsonObject"
    ],
    [
      "Where do messages go?",
      "console",
      "ConsoleConsumer"
    ],
    [
      "Where do messages go?",
      "file",
      "SimpleFileConsumer"
    ]
  ],
  "interfaces": [
    {
      "name": "IExternalJson",
      "extends": [],
      "members": [
        {
          "sig": "Release()",
          "desc": "Notifies the object that it is no longer used so it can return pooled resources to their pools; not thread-safe; O(1)",
          "preconditions": [
            "called exactly once after the owning element is freed"
          ],
          "postconditions": [
            "the object must not be written afterwards"
          ]
        },
        {
          "sig": "WriteTo(TextWriter output)",
          "desc": "Writes the raw JSON representation of this object to output; not thread-safe; O(n)",
          "preconditions": [
            "output not null"
          ],
          "example": "external.WriteTo(Console.Out);"
        },
        {
          "sig": "WriteToAsText(TextWriter output)",
          "desc": "Writes the JSON representation with quotes escaped so the result is safe to embed inside a JSON string value; not thread-safe; O(n)",
          "preconditions": [
            "output not null"
          ],
          "remarks": "double quotes become single quotes, backslashes become forward slashes, control characters are backslash-escaped"
        }
      ]
    },
    {
      "name": "IJsonArray",
      "extends": [
        "IDisposable"
      ],
      "members": [
        {
          "sig": "AddArray():IJsonArray",
          "desc": "Adds a nested array to the end of this array and returns it; caller takes ownership; amortised O(1)",
          "postconditions": [
            "Count increases by 1"
          ]
        },
        {
          "sig": "AddElement(bool value)",
          "desc": "Adds a boolean element to the end of this array; amortised O(1)",
          "postconditions": [
            "Count increases by 1"
          ]
        },
        {
          "sig": "AddElement(double value)",
          "desc": "Adds a number element to the end of this array; amortised O(1)",
          "postconditions": [
            "Count increases by 1"
          ]
        },
        {
          "sig": "AddElement(long value)",
          "desc": "Adds an integer element to the end of this array; amortised O(1)",
          "postconditions": [
            "Count increases by 1"
          ]
        },
        {
          "sig": "AddElement(string value)",
          "desc": "Adds a string element to the end of this array; amortised O(1)",
          "postconditions": [
            "Count increases by 1"
          ]
        },
        {
          "sig": "AddObject():IJsonObject",
          "desc": "Adds a nested object to the end of this array and returns it; caller takes ownership; amortised O(1)",
          "postconditions": [
            "Count increases by 1"
          ]
        },
        {
          "sig": "Count:int { get; }",
          "desc": "Returns the number of elements in this array; O(1)"
        },
        {
          "sig": "Dispose()",
          "desc": "Frees all contained elements and returns this array to the internal pool; not thread-safe; O(n)",
          "preconditions": [
            "the array must not be used afterwards"
          ]
        },
        {
          "sig": "this[int id]:JsonElement { get; }",
          "desc": "Returns the element at the given index; throws ArgumentOutOfRangeException if id is out of range; O(1)"
        }
      ]
    },
    {
      "name": "IJsonObject",
      "extends": [
        "IDisposable"
      ],
      "members": [
        {
          "sig": "AddArray(string name):IJsonArray?",
          "desc": "Adds a nested array field named name and returns it; returns null if name already exists or contains a double quote; caller takes ownership; O(n)",
          "preconditions": [
            "name not null"
          ]
        },
        {
          "sig": "AddElement(string name, IExternalJson value):bool",
          "desc": "Adds an external JSON field named name; serialised at write time via IExternalJson; returns false if name already exists or contains a double quote; holds reference only; O(n)",
          "preconditions": [
            "name not null",
            "value not null"
          ]
        },
        {
          "sig": "AddElement(string name, bool value):bool",
          "desc": "Adds a boolean field named name; returns false if name already exists or contains a double quote; O(n)",
          "preconditions": [
            "name not null"
          ]
        },
        {
          "sig": "AddElement(string name, double value):bool",
          "desc": "Adds a number field named name; returns false if name already exists or contains a double quote; O(n)",
          "preconditions": [
            "name not null"
          ]
        },
        {
          "sig": "AddElement(string name, long value):bool",
          "desc": "Adds an integer field named name; returns false if name already exists or contains a double quote; O(n)",
          "preconditions": [
            "name not null"
          ]
        },
        {
          "sig": "AddElement(string name, string format, params object[] list):bool",
          "desc": "Adds a deferred format field named name whose text is composed lazily at serialisation time; returns false if name already exists or contains a double quote; O(n)",
          "preconditions": [
            "name not null"
          ]
        },
        {
          "sig": "AddElement(string name, string value):bool",
          "desc": "Adds a string field named name; returns false if name already exists or contains a double quote; O(n)",
          "preconditions": [
            "name not null"
          ]
        },
        {
          "sig": "AddObject(string name):IJsonObject?",
          "desc": "Adds a nested object field named name and returns it; returns null if name already exists or contains a double quote; caller takes ownership; O(n)",
          "preconditions": [
            "name not null"
          ]
        },
        {
          "sig": "Count:int { get; }",
          "desc": "Returns the number of fields in this object; O(1)"
        },
        {
          "sig": "Dispose()",
          "desc": "Frees all contained fields and returns this object to the internal pool; not thread-safe; O(n)",
          "preconditions": [
            "the object must not be used afterwards"
          ]
        },
        {
          "sig": "TryGet(string name, out JsonElement filed):bool",
          "desc": "Returns true and outputs the field value if a field named name exists, otherwise returns false; O(n)",
          "preconditions": [
            "name not null"
          ],
          "example": "if (obj.TryGet(\"age\", out JsonElement field)) { ... }"
        },
        {
          "sig": "this[int id]:(string Name, JsonElement Filed) { get; }",
          "desc": "Returns the name/value pair at the given index; throws ArgumentOutOfRangeException if id is out of range; O(1)"
        }
      ]
    },
    {
      "name": "ILogConsumer",
      "extends": [],
      "members": [
        {
          "sig": "AddRef()",
          "desc": "Increments the reference count; lock-free; O(1)",
          "postconditions": [
            "caller must balance every AddRef with a Release"
          ]
        },
        {
          "sig": "Message(MessageData logMessage)",
          "desc": "Processes one log message; the publisher swallows consumer exceptions so implementers should report failures themselves; not thread-safe; O(1)",
          "preconditions": [
            "logMessage.Data not null"
          ]
        },
        {
          "sig": "Release()",
          "desc": "Decrements the reference count and disposes the consumer when it reaches zero; lock-free; O(1)",
          "preconditions": [
            "must be balanced with a prior AddRef or the initial ownership"
          ]
        }
      ]
    },
    {
      "name": "ILogExternalJsonFactory",
      "extends": [],
      "members": [
        {
          "sig": "Create(object value):IExternalJson",
          "desc": "Creates an IExternalJson wrapper for the given value; not thread-safe",
          "preconditions": [
            "value not null"
          ],
          "postconditions": [
            "caller takes ownership of the returned wrapper"
          ]
        }
      ]
    },
    {
      "name": "ILogFormatter",
      "extends": [],
      "members": [
        {
          "sig": "Format(MessageData logMessage, TextWriter dst)",
          "desc": "Writes the formatted representation of logMessage to dst; not thread-safe; O(n)",
          "preconditions": [
            "dst not null",
            "logMessage.Data not null"
          ]
        }
      ]
    },
    {
      "name": "ILogger",
      "extends": [
        "IDisposable"
      ],
      "members": [
        {
          "sig": "AddConsumer(ILogConsumer logConsumer)",
          "desc": "Adds a consumer that receives every published message; thread-safe (lock); O(1)",
          "preconditions": [
            "logConsumer not null"
          ],
          "postconditions": [
            "logConsumer starts receiving subsequent messages"
          ]
        },
        {
          "sig": "AppId:string? { get; set; }",
          "desc": "Gets or sets the application identifier embedded as the app_id field in every message; not thread-safe",
          "example": "logger.AppId = \"MyApp\";"
        },
        {
          "sig": "Dispose()",
          "desc": "Releases every consumer and frees logger resources; not thread-safe",
          "postconditions": [
            "the logger must not be used afterwards"
          ]
        },
        {
          "sig": "IgnoreStackFor:Severity { get; set; }",
          "desc": "Gets or sets the stack-trace threshold; stack traces are captured only for messages with severity below IgnoreStackFor; not thread-safe",
          "example": "logger.IgnoreStackFor = Severity.ERROR;"
        },
        {
          "sig": "LogFor:Severity { get; set; }",
          "desc": "Gets or sets the maximum severity that is logged; messages with severity at or below LogFor are logged; not thread-safe",
          "example": "logger.LogFor = Severity.WARN;"
        },
        {
          "sig": "LogTime:bool { get; set; }",
          "desc": "Gets or sets whether a time field (UTC DateTime.ToBinary) is embedded in every message; not thread-safe"
        },
        {
          "sig": "MachineName:string? { get; set; }",
          "desc": "Gets or sets the machine identifier embedded as the machine field in every message; not thread-safe"
        },
        {
          "sig": "RemoveConsumer(ILogConsumer logConsumer)",
          "desc": "Removes a consumer and releases one reference on it; thread-safe (lock); O(n)",
          "preconditions": [
            "logConsumer not null"
          ]
        },
        {
          "sig": "RemoveConsumerByType(Type type)",
          "desc": "Removes all consumers whose exact runtime type equals type and releases one reference on each; thread-safe (lock); O(n)",
          "preconditions": [
            "type not null"
          ],
          "remarks": "matches the exact type via GetType, not assignable types"
        },
        {
          "sig": "Tags:ITagList { get; }",
          "desc": "Returns the tag list whose entries are embedded into every message; not thread-safe"
        },
        {
          "sig": "d(string format, params object[] args)",
          "desc": "Logs a debug message; does nothing if DEBUG is above LogFor; not thread-safe"
        },
        {
          "sig": "e(string format, params object[] args)",
          "desc": "Logs an error message; does nothing if ERROR is above LogFor; not thread-safe"
        },
        {
          "sig": "i(string format, params object[] args)",
          "desc": "Logs an info message; does nothing if INFO is above LogFor; not thread-safe"
        },
        {
          "sig": "json(IJsonObject message)",
          "desc": "Logs a raw JSON object, adds the app_id, machine and tags fields, then disposes message; not thread-safe",
          "preconditions": [
            "message not null"
          ],
          "postconditions": [
            "message is disposed and must not be reused"
          ]
        },
        {
          "sig": "w(string format, params object[] args)",
          "desc": "Logs a warning message; does nothing if WARN is above LogFor; not thread-safe"
        },
        {
          "sig": "wtf(Exception exception)",
          "desc": "Logs an error message with the exception text; does nothing if ERROR is above LogFor; not thread-safe"
        },
        {
          "sig": "wtf(string message, Exception exception)",
          "desc": "Logs an error message combining the message and the exception text; does nothing if ERROR is above LogFor; not thread-safe"
        }
      ]
    },
    {
      "name": "ILoggerExt",
      "extends": [
        "ILogger"
      ],
      "members": [
        {
          "sig": "Publish(MessageData message)",
          "desc": "Publishes a pre-built message to all consumers; used by wrappers to duplicate messages; not thread-safe",
          "preconditions": [
            "message.Data not null"
          ]
        }
      ]
    },
    {
      "name": "ITagList",
      "extends": [],
      "members": [
        {
          "sig": "IsEmpty:bool { get; }",
          "desc": "Returns true if no tags are set; O(1)"
        },
        {
          "sig": "Remove(string tag):bool",
          "desc": "Removes a tag and returns true if it existed; thread-safe (lock); O(n)",
          "preconditions": [
            "tag not null"
          ]
        },
        {
          "sig": "Set(string tag, Func<string> valueFactory)",
          "desc": "Sets or replaces a tag whose value is evaluated lazily by valueFactory on each message; thread-safe (lock); O(n)",
          "preconditions": [
            "tag not null",
            "valueFactory not null"
          ]
        },
        {
          "sig": "Set(string tag, string? value = null)",
          "desc": "Sets or replaces a tag; a null value produces a bare tag and a non-null value produces a key-value tag; thread-safe (lock); O(n)",
          "preconditions": [
            "tag not null"
          ],
          "example": "logger.Tags.Set(\"env\", \"production\");"
        },
        {
          "sig": "WriteTo(IJsonArray tags)",
          "desc": "Appends all tags to the given JSON array; thread-safe (lock); O(n)",
          "preconditions": [
            "tags not null"
          ]
        }
      ]
    }
  ],
  "types": [
    {
      "name": "ConsoleConsumer",
      "kind": "class",
      "category": "consumer",
      "base": "MultiRefLogConsumer",
      "implements": [
        "ILogConsumer"
      ],
      "isAuxiliary": false,
      "desc": "Log consumer that writes every message to Console.Out using its Formatter.",
      "threadSafety": "not thread-safe",
      "limitations": "Console output is not a production sink; use SimpleFileConsumer or a custom consumer for files and networks.",
      "properties": [
        {
          "sig": "Formatter:ILogFormatter { get; set; }",
          "desc": "Gets or sets the formatter used to render messages to the console; not thread-safe",
          "remarks": "default is a SynchronizedLogFormatter producing SEVERITY: msg"
        }
      ],
      "methods": [
        {
          "sig": "Message(MessageData logMessage)",
          "desc": "Formats logMessage with Formatter and writes it to Console.Out; not thread-safe; O(n)",
          "preconditions": [
            "logMessage.Data not null"
          ]
        }
      ]
    },
    {
      "name": "ElementType",
      "kind": "enum",
      "category": "enumeration",
      "isAuxiliary": true,
      "desc": "Declares the runtime kind of a JsonElement; Unknown=0, String, StringFormat, Bool, Long, Number, Json, Object, Array",
      "threadSafety": "not thread-safe",
      "limitations": "StringFormat elements must be read with the format/params TryGet overload."
    },
    {
      "name": "JsonArray",
      "kind": "class",
      "category": "array builder",
      "implements": [
        "IJsonArray"
      ],
      "isAuxiliary": false,
      "desc": "Pooled mutable JSON array; obtain instances via IJsonObject.AddArray or IJsonArray.AddArray and return them with Free or Dispose.",
      "threadSafety": "not thread-safe",
      "limitations": "Prefers pooled instances; avoid new JsonArray for hot paths.",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "Creates an empty JsonArray outside the pool; prefer pooled instances for zero-allocation logging",
          "remarks": "the parameterless constructor is public but bypasses pooling"
        }
      ],
      "methods": [
        {
          "sig": "Free()",
          "desc": "Frees all contained elements and returns this array to the internal pool; not thread-safe; O(n)",
          "preconditions": [
            "the array must not be used afterwards"
          ]
        }
      ]
    },
    {
      "name": "JsonElement",
      "kind": "struct",
      "category": "value holder",
      "isAuxiliary": false,
      "desc": "Typed union-backed holder for one JSON value; created by constructors and read through Type-matched TryGet overloads.",
      "threadSafety": "not thread-safe",
      "limitations": "the out value is undefined when a TryGet returns false; always check the returned bool",
      "constructors": [
        {
          "sig": "ctor(IExternalJson value)",
          "desc": "Creates an element of type Json wrapping an external provider; holds reference only; O(1)",
          "preconditions": [
            "value not null"
          ]
        },
        {
          "sig": "ctor(JsonArray value)",
          "desc": "Creates an element of type Array; caller takes ownership; O(1)"
        },
        {
          "sig": "ctor(JsonObject value)",
          "desc": "Creates an element of type Object; caller takes ownership; O(1)"
        },
        {
          "sig": "ctor(bool value)",
          "desc": "Creates an element of type Bool; O(1)"
        },
        {
          "sig": "ctor(double value)",
          "desc": "Creates an element of type Number; O(1)"
        },
        {
          "sig": "ctor(long value)",
          "desc": "Creates an element of type Long; O(1)"
        },
        {
          "sig": "ctor(string format, object[] list)",
          "desc": "Creates an element of type StringFormat with deferred formatting parameters; O(1)"
        },
        {
          "sig": "ctor(string value)",
          "desc": "Creates an element of type String; O(1)"
        }
      ],
      "properties": [
        {
          "sig": "Type:ElementType { get; }",
          "desc": "Returns the stored element type; the matching TryGet overload must be used; O(1)"
        }
      ],
      "methods": [
        {
          "sig": "Free()",
          "desc": "Releases the underlying sub-object, sub-array or external JSON and resets the element; not thread-safe; O(n)",
          "postconditions": [
            "Type becomes Unknown"
          ]
        },
        {
          "sig": "TryGet(out IExternalJson):bool",
          "desc": "Returns true and outputs the external provider if Type is Json, otherwise returns false; O(1)"
        },
        {
          "sig": "TryGet(out IJsonArray):bool",
          "desc": "Returns true and outputs the nested array if Type is Array, otherwise returns false; O(1)"
        },
        {
          "sig": "TryGet(out IJsonObject):bool",
          "desc": "Returns true and outputs the nested object if Type is Object, otherwise returns false; O(1)"
        },
        {
          "sig": "TryGet(out bool):bool",
          "desc": "Returns true and outputs the boolean if Type is Bool, otherwise returns false; O(1)"
        },
        {
          "sig": "TryGet(out double):bool",
          "desc": "Returns true and outputs the number if Type is Number, otherwise returns false; O(1)"
        },
        {
          "sig": "TryGet(out long):bool",
          "desc": "Returns true and outputs the integer if Type is Long, otherwise returns false; O(1)"
        },
        {
          "sig": "TryGet(out string format, out object[] substrings):bool",
          "desc": "Returns true and outputs the format and parameters if Type is StringFormat, otherwise returns false; O(1)"
        },
        {
          "sig": "TryGet(out string):bool",
          "desc": "Returns true and outputs the string if Type is String, otherwise returns false; O(1)"
        }
      ]
    },
    {
      "name": "JsonObject",
      "kind": "class",
      "category": "object builder",
      "implements": [
        "IJsonObject"
      ],
      "isAuxiliary": false,
      "desc": "Pooled mutable JSON object; obtain instances via JsonObject.Construct and return them with Free or Dispose.",
      "threadSafety": "not thread-safe",
      "limitations": "Prefers pooled instances; avoid new JsonObject for hot paths.",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "Creates an empty JsonObject outside the pool; prefer JsonObject.Construct to reuse pooled instances",
          "remarks": "the parameterless constructor is public but bypasses pooling"
        }
      ],
      "methods": [
        {
          "sig": "Construct():IJsonObject",
          "desc": "Returns a pooled JsonObject; caller takes ownership and must Dispose or call Free when done; thread-safe (lock); O(1) amortised",
          "example": "var obj = JsonObject.Construct();",
          "remarks": "the returned instance is an IJsonObject; cast to JsonObject if Free must be called on the concrete type"
        },
        {
          "sig": "Free()",
          "desc": "Frees all fields and returns this object to the internal pool; not thread-safe; O(n)",
          "preconditions": [
            "the object must not be used afterwards"
          ]
        }
      ]
    },
    {
      "name": "JsonObjectAsExternalJson",
      "kind": "class",
      "category": "adapter",
      "base": "SimpleExternalJson",
      "implements": [
        "IExternalJson"
      ],
      "isAuxiliary": false,
      "desc": "IExternalJson adapter over a pooled JsonObject so a structured payload can be embedded as a sub-object of another message.",
      "threadSafety": "not thread-safe",
      "limitations": "WriteTo produces no output if Root has been released.",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "Creates a wrapper whose Root is a fresh pooled JsonObject; O(1)",
          "postconditions": [
            "Root is non-null until Release"
          ]
        }
      ],
      "properties": [
        {
          "sig": "Root:IJsonObject? { get; }",
          "desc": "Returns the underlying pooled JsonObject, or null after Release; not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "Reinit():bool",
          "desc": "Reinitialises Root if it is null and returns true, otherwise returns false; not thread-safe; O(1)",
          "remarks": "use after Release to reuse the wrapper"
        },
        {
          "sig": "Release()",
          "desc": "Frees Root back to the pool and sets it to null; not thread-safe; O(n)",
          "postconditions": [
            "Root is null"
          ]
        },
        {
          "sig": "WriteTo(TextWriter output)",
          "desc": "Serialises Root to output; not thread-safe; O(n)",
          "preconditions": [
            "output not null"
          ]
        }
      ]
    },
    {
      "name": "Log",
      "kind": "staticClass",
      "category": "facade",
      "isAuxiliary": false,
      "desc": "Static ambient facade that routes all calls to the active logger: the head of the calling thread's logger stack, or a process-global Logger when the stack is empty.",
      "threadSafety": "thread-safe (lock)",
      "limitations": "Static state is process-global except for the thread-local stack; prefer an injected Logger for testable code.",
      "properties": [
        {
          "sig": "AppId:string? { get; set; }",
          "desc": "Gets or sets AppId on the active logger; not thread-safe"
        },
        {
          "sig": "IgnoreStackFor:Severity { get; set; }",
          "desc": "Gets or sets IgnoreStackFor on the active logger; not thread-safe"
        },
        {
          "sig": "LogFor:Severity { get; set; }",
          "desc": "Gets or sets LogFor on the active logger; not thread-safe"
        },
        {
          "sig": "LogTime:bool { get; set; }",
          "desc": "Gets or sets LogTime on the active logger; not thread-safe"
        },
        {
          "sig": "MachineName:string? { get; set; }",
          "desc": "Gets or sets MachineName on the active logger; not thread-safe"
        },
        {
          "sig": "Tags:ITagList { get; }",
          "desc": "Returns Tags from the active logger; not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "AddConsumer(ILogConsumer logConsumer)",
          "desc": "Adds a consumer to the active logger; thread-safe (lock); O(1)",
          "preconditions": [
            "logConsumer not null"
          ]
        },
        {
          "sig": "PopThreadContextLogger():ILogger?",
          "desc": "Pops and returns the top thread-local logger, or null if the stack is empty; not thread-safe; O(1)"
        },
        {
          "sig": "PushThreadContextLogger(ILogger logger):ILogger",
          "desc": "Pushes logger onto the calling thread's logger stack and returns it; subsequent Log calls on this thread use it; not thread-safe; O(1)",
          "preconditions": [
            "logger not null"
          ],
          "postconditions": [
            "must be balanced by PopThreadContextLogger"
          ],
          "example": "Log.PushThreadContextLogger(ctx);"
        },
        {
          "sig": "RemoveConsumer(ILogConsumer logConsumer)",
          "desc": "Removes a consumer from the active logger; thread-safe (lock); O(n)",
          "preconditions": [
            "logConsumer not null"
          ]
        },
        {
          "sig": "RemoveConsumerByType(Type type)",
          "desc": "Removes consumers of the exact type from the active logger; thread-safe (lock); O(n)",
          "preconditions": [
            "type not null"
          ]
        },
        {
          "sig": "d(string format, params object[] args)",
          "desc": "Logs a debug message via the active logger; does nothing if DEBUG is above LogFor"
        },
        {
          "sig": "e(string format, params object[] args)",
          "desc": "Logs an error message via the active logger; does nothing if ERROR is above LogFor"
        },
        {
          "sig": "i(string format, params object[] args)",
          "desc": "Logs an info message via the active logger; does nothing if INFO is above LogFor"
        },
        {
          "sig": "json(IJsonObject message)",
          "desc": "Logs a raw JSON object via the active logger, which disposes it; not thread-safe",
          "postconditions": [
            "message is disposed and must not be reused"
          ]
        },
        {
          "sig": "w(string format, params object[] args)",
          "desc": "Logs a warning message via the active logger; does nothing if WARN is above LogFor"
        },
        {
          "sig": "wtf(Exception exception)",
          "desc": "Logs an error message with the exception text via the active logger; does nothing if ERROR is above LogFor"
        },
        {
          "sig": "wtf(string message, Exception exception)",
          "desc": "Logs an error message combining message and exception text via the active logger; does nothing if ERROR is above LogFor"
        }
      ]
    },
    {
      "name": "Logger",
      "kind": "class",
      "category": "logger",
      "implements": [
        "ILoggerExt"
      ],
      "isAuxiliary": false,
      "desc": "Concrete logger that stores consumers and tags, filters by LogFor, builds JSON messages and publishes them to its consumers.",
      "threadSafety": "not thread-safe",
      "limitations": "Consumer add/remove and publish are internally locked, but configuration properties are not synchronized.",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "Creates a logger with LogFor=DEBUG, IgnoreStackFor=ERROR, LogTime=true and no consumers",
          "postconditions": [
            "Tags is empty"
          ]
        },
        {
          "sig": "ctor(IEnumerable<ILogConsumer> consumers)",
          "desc": "Creates a logger and adds the given non-null consumers, skipping null entries; not thread-safe",
          "preconditions": [
            "consumers not null"
          ],
          "example": "var logger = new Logger(new[] { consoleConsumer });"
        }
      ],
      "properties": [
        {
          "sig": "AppId:string? { get; set; }",
          "desc": "Gets or sets the application identifier embedded as the app_id field; not thread-safe"
        },
        {
          "sig": "IgnoreStackFor:Severity { get; set; }",
          "desc": "Gets or sets the stack-trace threshold; stack traces are captured only for messages with severity below IgnoreStackFor; not thread-safe"
        },
        {
          "sig": "LogFor:Severity { get; set; }",
          "desc": "Gets or sets the maximum severity that is logged; messages with severity at or below LogFor are logged; not thread-safe"
        },
        {
          "sig": "LogTime:bool { get; set; }",
          "desc": "Gets or sets whether a time field (UTC DateTime.ToBinary) is embedded in every message; not thread-safe"
        },
        {
          "sig": "MachineName:string? { get; set; }",
          "desc": "Gets or sets the machine identifier embedded as the machine field; not thread-safe"
        },
        {
          "sig": "Tags:ITagList { get; }",
          "desc": "Returns the tag list embedded into every message; thread-safe (lock); O(1)"
        }
      ],
      "methods": [
        {
          "sig": "AddConsumer(ILogConsumer logConsumer)",
          "desc": "Adds a consumer that receives every published message; thread-safe (lock); O(1)",
          "preconditions": [
            "logConsumer not null"
          ]
        },
        {
          "sig": "Dispose()",
          "desc": "Releases every consumer, clears the consumer list and disposes the internal lock; not thread-safe",
          "postconditions": [
            "the logger must not be used afterwards"
          ]
        },
        {
          "sig": "Publish(MessageData message)",
          "desc": "Publishes message to all consumers in reverse order, swallowing each consumer exception; thread-safe (lock); O(n)",
          "preconditions": [
            "message.Data not null"
          ]
        },
        {
          "sig": "RemoveConsumer(ILogConsumer logConsumer)",
          "desc": "Removes a consumer and calls Release on it; thread-safe (lock); O(n)",
          "preconditions": [
            "logConsumer not null"
          ]
        },
        {
          "sig": "RemoveConsumerByType(Type type)",
          "desc": "Removes all consumers whose exact runtime type equals type and calls Release on each; thread-safe (lock); O(n)",
          "preconditions": [
            "type not null"
          ]
        },
        {
          "sig": "d(string format, params object[] args)",
          "desc": "Logs a debug message; does nothing if DEBUG is above LogFor; not thread-safe"
        },
        {
          "sig": "e(string format, params object[] args)",
          "desc": "Logs an error message; does nothing if ERROR is above LogFor; not thread-safe"
        },
        {
          "sig": "i(string format, params object[] args)",
          "desc": "Logs an info message; does nothing if INFO is above LogFor; not thread-safe"
        },
        {
          "sig": "json(IJsonObject message)",
          "desc": "Logs a raw JSON object, adds the app_id, machine and tags fields, publishes it, then disposes message; not thread-safe",
          "preconditions": [
            "message not null"
          ],
          "postconditions": [
            "message is disposed and must not be reused"
          ]
        },
        {
          "sig": "w(string format, params object[] args)",
          "desc": "Logs a warning message; does nothing if WARN is above LogFor; not thread-safe"
        },
        {
          "sig": "wtf(Exception exception)",
          "desc": "Logs an error message with the exception text; does nothing if ERROR is above LogFor; not thread-safe"
        },
        {
          "sig": "wtf(string message, Exception exception)",
          "desc": "Logs an error message combining message and exception text; does nothing if ERROR is above LogFor; not thread-safe"
        }
      ]
    },
    {
      "name": "LoggerWrapper",
      "kind": "class",
      "category": "adapter",
      "base": "Logger",
      "implements": [
        "ITagList"
      ],
      "isAuxiliary": false,
      "desc": "Logger that duplicates every published message to a wrapped logger and merges the wrapped logger's tags with its own tags; created via LoggerWrapper_Ext.Wrap.",
      "threadSafety": "not thread-safe",
      "limitations": "Requires the wrapped logger to implement ILoggerExt.",
      "properties": [
        {
          "sig": "Tags:ITagList { get; }",
          "desc": "Returns this wrapper as an ITagList that merges the wrapped logger's tags with the wrapper's own tags; not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "Publish(MessageData message)",
          "desc": "Publishes message to the wrapped logger and then to this wrapper's own consumers; not thread-safe; O(n)",
          "preconditions": [
            "message.Data not null"
          ]
        }
      ]
    },
    {
      "name": "LoggerWrapper_Ext",
      "kind": "staticClass",
      "category": "extension",
      "isAuxiliary": false,
      "desc": "Extension-method holder that adds the Wrap factory for ILogger.",
      "threadSafety": "not thread-safe",
      "methods": [
        {
          "sig": "Wrap(this ILogger logger):ILogger",
          "desc": "Returns a LoggerWrapper that duplicates messages to logger; throws InvalidOperationException if logger does not implement ILoggerExt; not thread-safe",
          "preconditions": [
            "logger not null"
          ],
          "postconditions": [
            "the wrapper's tags merge with the wrapped logger's tags"
          ],
          "example": "var wrapper = baseLogger.Wrap();"
        }
      ]
    },
    {
      "name": "MessageData",
      "kind": "struct",
      "category": "message",
      "isAuxiliary": false,
      "desc": "Read-only view of one log message backed by its JSON object; received by consumers through ILogConsumer.Message.",
      "threadSafety": "not thread-safe",
      "limitations": "Cannot be constructed by consumers (internal constructor); a default(MessageData) has a null Data and throws NullReferenceException on member access.",
      "properties": [
        {
          "sig": "Data:IJsonObject { get; }",
          "desc": "Returns the underlying JSON object of this message; O(1)"
        },
        {
          "sig": "Severity:Severity { get; }",
          "desc": "Returns the message severity parsed from the data, or UNKNOWN if absent; O(1)"
        },
        {
          "sig": "StackTraceDepth:int { get; }",
          "desc": "Returns the number of captured stack frames, or 0 if absent; O(1)"
        },
        {
          "sig": "Time:DateTime? { get; }",
          "desc": "Returns the message time parsed from the data, or null if absent; O(1)"
        }
      ],
      "methods": [
        {
          "sig": "WriteMessageTo(TextWriter output):bool",
          "desc": "Writes the message text to output and returns true; returns false if the msg field is absent; not thread-safe; O(n)",
          "preconditions": [
            "output not null"
          ],
          "example": "logMessage.WriteMessageTo(Console.Out);"
        },
        {
          "sig": "WriteStackFrame(int frameId, string prefix, TextWriter output):bool",
          "desc": "Writes a single stack frame at frameId prefixed by prefix to output and returns true; returns false if the frame is missing or malformed; not thread-safe; O(n)",
          "preconditions": [
            "output not null"
          ]
        },
        {
          "sig": "WriteStackTrace(string prefix, TextWriter output):bool",
          "desc": "Writes every stack frame prefixed by prefix to output and returns true; returns false if no stack field exists; not thread-safe; O(n)",
          "preconditions": [
            "output not null"
          ]
        },
        {
          "sig": "WriteTagsTo(TextWriter output, Predicate<string>? tagsSelector = null):bool",
          "desc": "Writes tag entries matching tagsSelector (or all tags) as name=value; entries to output and returns true; returns false if no tags field exists; not thread-safe; O(n)",
          "preconditions": [
            "output not null"
          ]
        }
      ]
    },
    {
      "name": "MultiRefLogConsumer",
      "kind": "class",
      "category": "base consumer",
      "implements": [
        "ILogConsumer"
      ],
      "isAuxiliary": false,
      "desc": "Abstract reference-counted ILogConsumer; subclasses override Message and may override protected virtual Dispose for cleanup when the reference count reaches zero.",
      "threadSafety": "lock-free",
      "limitations": "Derived classes must call AddRef and Release in balance or the instance is disposed prematurely.",
      "methods": [
        {
          "sig": "AddRef()",
          "desc": "Increments the reference count; lock-free; O(1)"
        },
        {
          "sig": "Message(MessageData logMessage)",
          "desc": "Abstract; processes one log message; not thread-safe",
          "preconditions": [
            "logMessage.Data not null"
          ]
        },
        {
          "sig": "Release()",
          "desc": "Decrements the reference count and calls Dispose when it reaches zero; lock-free; O(1)",
          "postconditions": [
            "when the count reaches zero the instance is disposed"
          ]
        }
      ]
    },
    {
      "name": "Severity",
      "kind": "enum",
      "category": "enumeration",
      "isAuxiliary": true,
      "desc": "UNKNOWN=0, FATAL=1, ERROR=2, WARN=3, INFO=4, DEBUG=5; lower numeric value means higher importance",
      "threadSafety": "not thread-safe",
      "limitations": "numeric values are embedded in the severity JSON field, so consumers see numbers, not names"
    },
    {
      "name": "SeveritySerializer",
      "kind": "staticClass",
      "category": "extension",
      "isAuxiliary": false,
      "desc": "Extension-method holder that converts a Severity value to its enum member name.",
      "threadSafety": "not thread-safe",
      "methods": [
        {
          "sig": "Serialize(this Severity):string",
          "desc": "Returns the name of the severity value (for example DEBUG); returns UNKNOWN if the value is outside the enum; O(1)",
          "example": "var name = Severity.DEBUG.Serialize();"
        }
      ]
    },
    {
      "name": "SimpleExternalJson",
      "kind": "class",
      "category": "base adapter",
      "implements": [
        "IExternalJson"
      ],
      "isAuxiliary": false,
      "desc": "Abstract base for IExternalJson implementations; implements WriteToAsText on top of the abstract WriteTo.",
      "threadSafety": "not thread-safe",
      "methods": [
        {
          "sig": "Release()",
          "desc": "Abstract; releases pooled resources; not thread-safe; O(1)"
        },
        {
          "sig": "WriteTo(TextWriter output)",
          "desc": "Abstract; writes the raw JSON representation to output; not thread-safe; O(n)",
          "preconditions": [
            "output not null"
          ]
        },
        {
          "sig": "WriteToAsText(TextWriter output)",
          "desc": "Writes the JSON representation with quotes escaped so it is safe to embed in a JSON string value; not thread-safe; O(n)",
          "preconditions": [
            "output not null"
          ]
        }
      ]
    },
    {
      "name": "SimpleFileConsumer",
      "kind": "class",
      "category": "consumer",
      "base": "MultiRefLogConsumer",
      "implements": [
        "ILogConsumer"
      ],
      "isAuxiliary": false,
      "desc": "Log consumer that appends every message to a text file using its Formatter and flushes after each message.",
      "threadSafety": "not thread-safe",
      "limitations": "Flushes on every message; for high-throughput use a buffered file consumer instead.",
      "constructors": [
        {
          "sig": "ctor(string fileName)",
          "desc": "Opens fileName in append mode and creates the consumer; throws IOException if the file cannot be opened",
          "preconditions": [
            "fileName not null or empty"
          ],
          "example": "var consumer = new SimpleFileConsumer(\"app.log\");"
        }
      ],
      "properties": [
        {
          "sig": "Formatter:ILogFormatter { get; set; }",
          "desc": "Gets or sets the formatter used to render messages to the file; not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "Message(MessageData logMessage)",
          "desc": "Formats logMessage with Formatter, writes it to the file and flushes; not thread-safe; O(n)",
          "preconditions": [
            "logMessage.Data not null"
          ]
        }
      ]
    },
    {
      "name": "StaticLogger",
      "kind": "class",
      "category": "facade",
      "implements": [
        "ILoggerExt"
      ],
      "isAuxiliary": false,
      "desc": "Singleton ILoggerExt whose members delegate to the static Log class; use as a DI-friendly facade around ambient logging.",
      "threadSafety": "thread-safe (lock)",
      "limitations": "Configuration changes affect the global active logger.",
      "properties": [
        {
          "sig": "AppId:string? { get; set; }",
          "desc": "Gets or sets AppId via the static Log class; not thread-safe"
        },
        {
          "sig": "IgnoreStackFor:Severity { get; set; }",
          "desc": "Gets or sets IgnoreStackFor via the static Log class; not thread-safe"
        },
        {
          "sig": "Instance:StaticLogger { get; }",
          "desc": "Returns the singleton instance; thread-safe; O(1)",
          "remarks": "static readonly singleton field"
        },
        {
          "sig": "LogFor:Severity { get; set; }",
          "desc": "Gets or sets LogFor via the static Log class; not thread-safe"
        },
        {
          "sig": "LogTime:bool { get; set; }",
          "desc": "Gets or sets LogTime via the static Log class; not thread-safe"
        },
        {
          "sig": "MachineName:string? { get; set; }",
          "desc": "Gets or sets MachineName via the static Log class; not thread-safe"
        },
        {
          "sig": "Tags:ITagList { get; }",
          "desc": "Returns Tags from the static Log class; not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "AddConsumer(ILogConsumer logConsumer)",
          "desc": "Adds a consumer via the static Log class; thread-safe (lock)",
          "preconditions": [
            "logConsumer not null"
          ]
        },
        {
          "sig": "Dispose()",
          "desc": "Does nothing; provided to satisfy ILogger",
          "remarks": "the singleton is not disposed"
        },
        {
          "sig": "Publish(MessageData message)",
          "desc": "Publishes message via the static Log class; not thread-safe",
          "preconditions": [
            "message.Data not null"
          ]
        },
        {
          "sig": "RemoveConsumer(ILogConsumer logConsumer)",
          "desc": "Removes a consumer via the static Log class; thread-safe (lock)",
          "preconditions": [
            "logConsumer not null"
          ]
        },
        {
          "sig": "RemoveConsumerByType(Type type)",
          "desc": "Removes consumers of the exact type via the static Log class; thread-safe (lock)",
          "preconditions": [
            "type not null"
          ]
        },
        {
          "sig": "d(string format, params object[] args)",
          "desc": "Logs a debug message via the static Log class; does nothing if DEBUG is above LogFor"
        },
        {
          "sig": "e(string format, params object[] args)",
          "desc": "Logs an error message via the static Log class; does nothing if ERROR is above LogFor"
        },
        {
          "sig": "i(string format, params object[] args)",
          "desc": "Logs an info message via the static Log class; does nothing if INFO is above LogFor"
        },
        {
          "sig": "json(IJsonObject message)",
          "desc": "Logs a raw JSON object via the static Log class, which disposes it",
          "postconditions": [
            "message is disposed and must not be reused"
          ]
        },
        {
          "sig": "w(string format, params object[] args)",
          "desc": "Logs a warning message via the static Log class; does nothing if WARN is above LogFor"
        },
        {
          "sig": "wtf(Exception exception)",
          "desc": "Logs an error message with the exception text via the static Log class; does nothing if ERROR is above LogFor"
        },
        {
          "sig": "wtf(string message, Exception exception)",
          "desc": "Logs an error message combining message and exception text via the static Log class; does nothing if ERROR is above LogFor"
        }
      ]
    },
    {
      "name": "SynchronizedLogFormatter",
      "kind": "class",
      "category": "adapter",
      "implements": [
        "ILogFormatter"
      ],
      "isAuxiliary": false,
      "desc": "Thread-safe ILogFormatter that renders each message through a wrapped delegate and writes the buffered result to the destination.",
      "threadSafety": "thread-safe (lock)",
      "limitations": "The wrapped delegate must not call back into the destination writer.",
      "constructors": [
        {
          "sig": "ctor(Action<MessageData, TextWriter> formatter)",
          "desc": "Creates a formatter that calls formatter for each message under a shared lock; thread-safe (lock)",
          "preconditions": [
            "formatter not null"
          ],
          "example": "var f = new SynchronizedLogFormatter((msg, dst) => { dst.Write(msg.Severity); });"
        }
      ],
      "methods": [
        {
          "sig": "Format(MessageData logMessage, TextWriter dst)",
          "desc": "Renders logMessage via the wrapped delegate and writes the result to dst; thread-safe (lock); O(n)",
          "preconditions": [
            "dst not null",
            "logMessage.Data not null"
          ]
        }
      ]
    },
    {
      "name": "TagList",
      "kind": "class",
      "category": "tag collection",
      "implements": [
        "ITagList"
      ],
      "isAuxiliary": false,
      "desc": "Thread-safe collection of tags that serialise into a JSON array as bare strings or key-value objects.",
      "threadSafety": "thread-safe (lock)",
      "limitations": "Tags are global to the logger, not per message; use a per-message payload for one-off context.",
      "properties": [
        {
          "sig": "IsEmpty:bool { get; }",
          "desc": "Returns true if no tags are set; O(1)"
        }
      ],
      "methods": [
        {
          "sig": "Remove(string tag):bool",
          "desc": "Removes a tag and returns true if it existed; thread-safe (lock); O(n)",
          "preconditions": [
            "tag not null"
          ]
        },
        {
          "sig": "Set(string tag, Func<string> valueFactory)",
          "desc": "Sets or replaces a tag whose value is evaluated lazily by valueFactory on each message; thread-safe (lock); O(n)",
          "preconditions": [
            "tag not null",
            "valueFactory not null"
          ],
          "example": "tags.Set(\"version\", () => version);"
        },
        {
          "sig": "Set(string tag, string? value = null)",
          "desc": "Sets or replaces a tag; a null value produces a bare tag and a non-null value produces a key-value tag; thread-safe (lock); O(n)",
          "preconditions": [
            "tag not null"
          ]
        },
        {
          "sig": "WriteTo(IJsonArray tags)",
          "desc": "Appends all tags to the given JSON array; thread-safe (lock); O(n)",
          "preconditions": [
            "tags not null"
          ]
        }
      ]
    },
    {
      "name": "Union",
      "kind": "struct",
      "category": "storage",
      "isAuxiliary": true,
      "desc": "Explicit-layout union that stores a bool, long or double at the same memory offset; reading a different field than the one written yields unspecified data.",
      "threadSafety": "not thread-safe",
      "limitations": "Used internally by JsonElement; consumers should prefer the Type-matched TryGet overloads.",
      "properties": [
        {
          "sig": "BoolValue:bool { get; set; }",
          "desc": "Reads or writes the boolean interpretation of the shared storage; O(1)"
        },
        {
          "sig": "DoubleValue:double { get; set; }",
          "desc": "Reads or writes the double interpretation of the shared storage; O(1)"
        },
        {
          "sig": "LongValue:long { get; set; }",
          "desc": "Reads or writes the long interpretation of the shared storage; O(1)"
        }
      ]
    },
    {
      "name": "VoidLogger",
      "kind": "class",
      "category": "null object",
      "implements": [
        "ILogger"
      ],
      "isAuxiliary": false,
      "desc": "Singleton ILogger whose methods are no-ops; json disposes the passed message; use as a null-object to disable logging without changing call sites.",
      "threadSafety": "thread-safe",
      "limitations": "No-op logger hides all logging; it still disposes messages passed to json.",
      "properties": [
        {
          "sig": "AppId:string? { get; set; }",
          "desc": "Setter stores nothing; getter returns null; not thread-safe"
        },
        {
          "sig": "IgnoreStackFor:Severity { get; set; }",
          "desc": "Setter stores nothing; getter returns the default; not thread-safe"
        },
        {
          "sig": "Instance:VoidLogger { get; }",
          "desc": "Returns the singleton instance; thread-safe; O(1)",
          "remarks": "static readonly singleton field"
        },
        {
          "sig": "IsActive:bool { get; }",
          "desc": "Returns false, indicating this logger performs no output; O(1)"
        },
        {
          "sig": "LogFor:Severity { get; set; }",
          "desc": "Setter stores nothing; getter returns the default; not thread-safe"
        },
        {
          "sig": "LogTime:bool { get; set; }",
          "desc": "Setter stores nothing; getter returns the default; not thread-safe"
        },
        {
          "sig": "MachineName:string? { get; set; }",
          "desc": "Setter stores nothing; getter returns null; not thread-safe"
        },
        {
          "sig": "Tags:ITagList { get; }",
          "desc": "Returns VoidTagList.Instance; O(1)"
        }
      ],
      "methods": [
        {
          "sig": "AddConsumer(ILogConsumer logConsumer)",
          "desc": "Does nothing; provided to satisfy ILogger"
        },
        {
          "sig": "Dispose()",
          "desc": "Does nothing; provided to satisfy ILogger"
        },
        {
          "sig": "RemoveConsumer(ILogConsumer logConsumer)",
          "desc": "Does nothing; provided to satisfy ILogger"
        },
        {
          "sig": "RemoveConsumerByType(Type type)",
          "desc": "Does nothing; provided to satisfy ILogger"
        },
        {
          "sig": "d(string format, params object[] args)",
          "desc": "Does nothing"
        },
        {
          "sig": "e(string format, params object[] args)",
          "desc": "Does nothing"
        },
        {
          "sig": "i(string format, params object[] args)",
          "desc": "Does nothing"
        },
        {
          "sig": "json(IJsonObject message)",
          "desc": "Disposes message and does nothing else; not thread-safe",
          "postconditions": [
            "message is disposed and must not be reused"
          ]
        },
        {
          "sig": "w(string format, params object[] args)",
          "desc": "Does nothing"
        },
        {
          "sig": "wtf(Exception exception)",
          "desc": "Does nothing"
        },
        {
          "sig": "wtf(string message, Exception exception)",
          "desc": "Does nothing"
        }
      ]
    },
    {
      "name": "VoidTagList",
      "kind": "class",
      "category": "null object",
      "implements": [
        "ITagList"
      ],
      "isAuxiliary": false,
      "desc": "Singleton ITagList whose methods are no-ops; used by VoidLogger.",
      "threadSafety": "thread-safe",
      "limitations": "Set and Remove have no effect; WriteTo writes nothing.",
      "properties": [
        {
          "sig": "Instance:VoidTagList { get; }",
          "desc": "Returns the singleton instance; thread-safe; O(1)",
          "remarks": "static readonly singleton field"
        },
        {
          "sig": "IsEmpty:bool { get; }",
          "desc": "Returns true; O(1)"
        }
      ],
      "methods": [
        {
          "sig": "Remove(string tag):bool",
          "desc": "Returns false and does nothing; O(1)"
        },
        {
          "sig": "Set(string tag, Func<string> valueFactory)",
          "desc": "Does nothing; O(1)"
        },
        {
          "sig": "Set(string tag, string? value = null)",
          "desc": "Does nothing; O(1)"
        },
        {
          "sig": "WriteTo(IJsonArray tags)",
          "desc": "Does nothing; O(1)"
        }
      ]
    }
  ],
  "extensions": [
    {
      "for": "IJsonObject",
      "members": [
        {
          "sig": "AddMultiElement(this IJsonObject self, string name, string format, params object[] list):bool",
          "desc": "Adds a string field named name whose text is composed from format, then extracts every {name} placeholder (and positional {0}, {1}, ... placeholders) into sibling fields; a @{name} placeholder requires an IExternalJson argument and is embedded raw, otherwise the argument is stringified; returns false if any placeholder cannot be resolved or the field already exists; not thread-safe; O(n*m)",
          "preconditions": [
            "self not null",
            "name not null"
          ],
          "example": "obj.AddMultiElement(\"msg\", \"Hello {name}\", \"world\");"
        },
        {
          "sig": "Serialize(this IJsonObject self, TextWriter output):bool",
          "desc": "Writes the object as compact JSON to output and returns true; returns false if an element cannot be written; not thread-safe; O(n)",
          "preconditions": [
            "self not null",
            "output not null"
          ]
        },
        {
          "sig": "Serialize(this IJsonObject self, out string output):bool",
          "desc": "Serialises the object to a string and returns true; returns false with output set to an empty string if serialisation fails; not thread-safe; O(n)",
          "preconditions": [
            "self not null"
          ],
          "example": "obj.Serialize(out string json);"
        }
      ]
    },
    {
      "for": "JsonElement",
      "members": [
        {
          "sig": "WriteTo(this JsonElement element, TextWriter output, bool escape = true):bool",
          "desc": "Writes the element as JSON to output and returns true; returns false for elements of type Unknown or when a write fails; when escape is true double quotes become single quotes, backslashes are doubled and control characters are escaped; not thread-safe; O(n)",
          "preconditions": [
            "output not null"
          ]
        }
      ]
    },
    {
      "for": "Severity",
      "members": [
        {
          "sig": "Serialize(this Severity):string",
          "desc": "Returns the name of the severity value (for example DEBUG); returns UNKNOWN if the value is outside the enum; O(1)",
          "example": "var name = Severity.WARN.Serialize();"
        }
      ]
    }
  ],
  "gotchas": [
    "A default(MessageData) has a null Data; accessing any member on it throws NullReferenceException, so never create one.",
    "AddElement returns false rather than throwing when a field name already exists or contains a double quote; callers that ignore the return value silently lose fields.",
    "JsonElement stores its value in a union; you must call the TryGet overload that matches Type, otherwise it returns false and the out value is default.",
    "JsonObject and JsonArray are pooled; construct with JsonObject.Construct (or via AddObject and AddArray) and Dispose when done to enable reuse; new JsonObject bypasses the pool.",
    "LogFor is inverted compared to most loggers: messages with severity at or below LogFor are logged, so to reduce verbosity set LogFor to a lower severity value; the default DEBUG logs everything.",
    "RemoveConsumerByType matches the exact runtime type (GetType == type), not assignable types.",
    "Stack traces are captured only when the message severity is below IgnoreStackFor; the default IgnoreStackFor=ERROR means only UNKNOWN and FATAL messages carry a stack.",
    "The Log thread-local logger stack is per thread; PushThreadContextLogger affects only the calling thread.",
    "The Severity enum numbers are embedded as the severity JSON field, so consumers see numeric values rather than names.",
    "logger.json(message) disposes the passed IJsonObject after publishing, so the caller must never reuse it."
  ],
  "commonMistakes": [
    "Adding a field whose name already exists and ignoring the false return value.",
    "Assuming RemoveConsumerByType also removes consumers of derived types.",
    "Assuming a higher LogFor value logs more; the condition is severity <= LogFor, so raising LogFor actually disables lower-severity messages.",
    "Calling a mismatched TryGet overload on JsonElement and trusting the out value.",
    "Forgetting to Dispose a JsonObject or JsonArray after use keeps the object out of the pool and wastes the zero-allocation design.",
    "Reusing an IJsonObject after passing it to logger.json(); it has already been disposed.",
    "Using new JsonObject() or new JsonArray() instead of pooled construction defeats the pooling design.",
    "Wrapping an ILogger that does not implement ILoggerExt with Wrap() and crashing on the InvalidOperationException."
  ],
  "patterns": [
    {
      "goal": "Attach a lazy tag evaluated per message",
      "code": "logger.Tags.Set(\"session\", () => session.Id.ToString());"
    },
    {
      "goal": "Build and serialize a JsonObject to a string",
      "code": "var obj = JsonObject.Construct();obj.AddElement(\"name\", \"Alice\");obj.AddElement(\"age\", 30);obj.Serialize(out string json);obj.Dispose();"
    },
    {
      "goal": "Configure a logger with console and file consumers",
      "code": "var logger = new Logger();logger.AddConsumer(new ConsoleConsumer());logger.AddConsumer(new SimpleFileConsumer(\"app.log\"));logger.i(\"Server started on {port}\", 8080);logger.Dispose();"
    },
    {
      "goal": "Custom IExternalJson for a sub-object",
      "code": "var external = new JsonObjectAsExternalJson();external.Root!.AddElement(\"nested\", \"value\");var obj = JsonObject.Construct();obj.AddElement(\"data\", external);obj.Serialize(out string json);obj.Dispose();external.Release();"
    },
    {
      "goal": "Gracefully degrade when adding a duplicate field fails",
      "code": "var obj = JsonObject.Construct();if (!obj.AddElement(\"id\", 1)) { obj.AddElement(\"id_dup\", 1); }obj.Dispose();"
    },
    {
      "goal": "Log a structured JSON payload",
      "code": "var payload = JsonObject.Construct();payload.AddElement(\"event\", \"purchase\");payload.AddElement(\"amount\", 29.99);logger.json(payload);"
    },
    {
      "goal": "Log from anywhere via the static Log class",
      "code": "Log.AddConsumer(new ConsoleConsumer());Log.i(\"request {id} handled\", requestId);"
    },
    {
      "goal": "Push a thread-local logger context with tags",
      "code": "var ctx = new Logger();ctx.Tags.Set(\"requestId\", Guid.NewGuid().ToString());Log.PushThreadContextLogger(ctx);try { Log.i(\"processing\"); } finally { Log.PopThreadContextLogger(); }"
    },
    {
      "goal": "Read a field only when present",
      "code": "if (obj.TryGet(\"name\", out JsonElement field) && field.TryGet(out string name)) { Console.WriteLine(name); }"
    },
    {
      "goal": "Wrap a logger to merge tags from both",
      "code": "var wrapper = baseLogger.Wrap();wrapper.Tags.Set(\"module\", \"http\");wrapper.i(\"Request received\");"
    }
  ],
  "relationships": [
    {
      "from": "ConsoleConsumer",
      "to": "MultiRefLogConsumer",
      "rel": "extends"
    },
    {
      "from": "IJsonObject",
      "to": "IJsonArray",
      "rel": "creates via AddArray"
    },
    {
      "from": "IJsonObject",
      "to": "IJsonObject",
      "rel": "creates via AddObject"
    },
    {
      "from": "ILogConsumer",
      "to": "MessageData",
      "rel": "receives via Message"
    },
    {
      "from": "JsonArray",
      "to": "IJsonArray",
      "rel": "implements"
    },
    {
      "from": "JsonObject",
      "to": "IJsonObject",
      "rel": "implements"
    },
    {
      "from": "JsonObjectAsExternalJson",
      "to": "SimpleExternalJson",
      "rel": "extends"
    },
    {
      "from": "Log",
      "to": "ILoggerExt",
      "rel": "delegates to the active logger"
    },
    {
      "from": "Logger",
      "to": "IJsonObject",
      "rel": "disposes after json()"
    },
    {
      "from": "Logger",
      "to": "ILogConsumer",
      "rel": "publishes messages to"
    },
    {
      "from": "LoggerWrapper",
      "to": "ILogger",
      "rel": "wraps and duplicates messages to"
    },
    {
      "from": "LoggerWrapper",
      "to": "Logger",
      "rel": "extends"
    },
    {
      "from": "LoggerWrapper_Ext",
      "to": "ILogger",
      "rel": "adds the Wrap() extension"
    },
    {
      "from": "MessageData",
      "to": "IJsonObject",
      "rel": "wraps as Data"
    },
    {
      "from": "MultiRefLogConsumer",
      "to": "ILogConsumer",
      "rel": "implements"
    },
    {
      "from": "SeveritySerializer",
      "to": "Severity",
      "rel": "adds the Serialize() extension"
    },
    {
      "from": "SimpleExternalJson",
      "to": "IExternalJson",
      "rel": "implements"
    },
    {
      "from": "SimpleFileConsumer",
      "to": "MultiRefLogConsumer",
      "rel": "extends"
    },
    {
      "from": "StaticLogger",
      "to": "Log",
      "rel": "delegates to"
    },
    {
      "from": "SynchronizedLogFormatter",
      "to": "ILogFormatter",
      "rel": "implements"
    },
    {
      "from": "TagList",
      "to": "ITagList",
      "rel": "implements"
    },
    {
      "from": "VoidLogger",
      "to": "ILogger",
      "rel": "implements"
    },
    {
      "from": "VoidTagList",
      "to": "ITagList",
      "rel": "implements"
    }
  ]
}
