using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Scriba.JsonFactory
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Union
    {
        [FieldOffset(0)]
        public bool BoolValue;

        [FieldOffset(0)]
        public long LongValue;

        [FieldOffset(0)]
        public double DoubleValue;
    }

    /// <summary>
    /// Уберструктура для хранения элемента json
    /// </summary>
    public struct JsonElement
    {
        private Union _valuesUnion;
        private object? _objectValue;
        private object[]? _substrings;

        public ElementType Type { get; private set; }


        private bool BoolValue
        {
            get => _valuesUnion.BoolValue;
            set => _valuesUnion.BoolValue = value;
        }

        private long LongValue
        {
            get => _valuesUnion.LongValue;
            set => _valuesUnion.LongValue = value;
        }

        private double DoubleValue
        {
            get => _valuesUnion.DoubleValue;
            set => _valuesUnion.DoubleValue = value;
        }

        private string? StringValue
        {
            get => _objectValue as string;
            set => _objectValue = value;
        }

        private IExternalJson? SubJson
        {
            get => _objectValue as IExternalJson;
            set => _objectValue = value;
        }

        private JsonObject? ObjectValue
        {
            get => _objectValue as JsonObject;
            set => _objectValue = value;
        }

        private JsonArray? ArrayValue
        {
            get => _objectValue as JsonArray;
            set => _objectValue = value;
        }
        
        public JsonElement(string value)
            : this()
        {
            Type = ElementType.String;
            StringValue = value;
        }

        public JsonElement(string format, object[] list)
            : this()
        {
            Type = ElementType.StringFormat;
            StringValue = format;
            _substrings = list;
        }

        public JsonElement(double value)
            : this()
        {
            Type = ElementType.Number;
            DoubleValue = value;
        }

        public JsonElement(bool value)
            : this()
        {
            Type = ElementType.Bool;
            BoolValue = value;
        }

        public JsonElement(long value)
            : this()
        {
            Type = ElementType.Long;
            LongValue = value;
        }

        public JsonElement(IExternalJson value)
            : this()
        {
            Type = ElementType.Json;
            SubJson = value;
        }

        public JsonElement(JsonObject value)
            : this()
        {
            Type = ElementType.Object;
            ObjectValue = value;
        }

        public JsonElement(JsonArray value)
            : this()
        {
            Type = ElementType.Array;
            ArrayValue = value;
        }

        public void Free()
        {
            SubJson?.Release();
            ObjectValue?.Free();
            ArrayValue?.Free();

            Type = ElementType.Unknown;
            _valuesUnion = new Union();
            _objectValue = null;
            _substrings = null;
        }

        public bool TryGet(out string value)
        {
            if (Type == ElementType.String)
            {
                value = StringValue ?? throw new Exception();
                return true;
            }
            value = "";
            return false;
        }

        public bool TryGet([MaybeNullWhen(false)] out string format, [MaybeNullWhen(false)] out object[] substrings)
        {
            if (Type == ElementType.StringFormat)
            {
                format = StringValue ?? throw new Exception();
                substrings = _substrings ?? throw new Exception();
                return true;
            }
            format = "";
            substrings = null;
            return false;
        }

        public bool TryGet(out double value)
        {
            if (Type == ElementType.Number)
            {
                value = DoubleValue;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out bool value)
        {
            if (Type == ElementType.Bool)
            {
                value = BoolValue;
                return true;
            }
            value = false;
            return false;
        }

        public bool TryGet(out long value)
        {
            if (Type == ElementType.Long)
            {
                value = LongValue;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet([MaybeNullWhen(false)] out IExternalJson value)
        {
            if (Type == ElementType.Json)
            {
                value = SubJson ?? throw new Exception();
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGet([MaybeNullWhen(false)] out IJsonObject value)
        {
            if (Type == ElementType.Object)
            {
                value = ObjectValue ?? throw new Exception();
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGet([MaybeNullWhen(false)] out IJsonArray value)
        {
            if (Type == ElementType.Array)
            {
                value = ArrayValue ?? throw new Exception();
                return true;
            }
            value = null;
            return false;
        }
    }
}
