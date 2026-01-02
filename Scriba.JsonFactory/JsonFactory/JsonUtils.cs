using System;

namespace Scriba.JsonFactory
{
    public static class JsonUtils
    {
        public static bool AddMultiElement(this IJsonObject self, string name, string format, params object[] list)
        {
            if (self.AddElement(name, format, list))
            {
                const int InText = 0;
                const int InBracket = 1;
                int state = InText;

                int openBracketPos = 0;
                int globalParamId = 0;
                bool hasAt = false;
                for (int i = 0; i < format.Length; ++i)
                {
                    char ch = format[i];
                    switch (ch)
                    {
                        case '{':
                            if (state == InText)
                            {
                                state = InBracket;
                                openBracketPos = i;
                                hasAt = false;
                            }
                            else
                            {
                                return false;
                            }
                            break;
                        case '@':
                            if (state == InBracket && openBracketPos == i - 1)
                            {
                                hasAt = true;
                            }
                            break;
                        case '}':
                            if (state == InBracket)
                            {
                                string paramName = format.Substring(openBracketPos + 1, i - (openBracketPos + 1));

                                if (!int.TryParse(paramName, out var paramId))
                                {
                                    paramId = globalParamId++;
                                }

                                bool res = false;
                                if (paramId < list.Length)
                                {
                                    if (hasAt)
                                    {
                                        if (list[paramId] is IExternalJson nestedJson)
                                        {
                                            res = self.AddElement(paramName, nestedJson);
                                        }
                                        else
                                        {
                                            //throw new InvalidOperationException("Wrong param#" + paramId + " type. Must be IJson");
                                            res = self.AddElement(paramName, "Null JSON");
                                        }
                                    }
                                    else
                                    {
                                        self.AddElement(paramName, ToString(list[paramId], null));
                                        res = true; // не интересеует результат добавления атрибута из-за варианта "{0} {0}"
                                    }
                                }

                                if (!res)
                                {
                                    return false;
                                }
                                state = InText;
                            }
                            else
                            {
                                return false;
                            }
                            break;
                        default:
                            break;
                    }
                }
                return true;
            }
            return false;
        }

        public static bool Serialize(this IJsonObject self, System.IO.TextWriter output)
        {
            output.Write("{");
            int count = self.Count;
            for (int i = 0; i < count; ++i)
            {
                var kv = self[i];
                if (i != 0)
                {
                    output.Write(", ");
                }
                output.Write('"');
                output.Write(kv.Name);
                output.Write("\": ");
                if (!kv.Filed.WriteTo(output))
                {
                    return false;
                }
            }
            output.Write("}");
            return true;
        }

        public static bool Serialize(this IJsonObject self, out string output)
        {
            var sw = Pool<System.IO.StringWriter>.New();
            output = self.Serialize(sw) ? sw.ToString() : "";
            sw.GetStringBuilder().Length = 0;
            Pool<System.IO.StringWriter>.Free(sw);
            return output != "";
        }

        public static bool WriteTo(this JsonElement element, System.IO.TextWriter output, bool escape = true)
        {
            switch (element.Type)
            {
                case ElementType.Unknown:
                    output.Write("<ERROR>");
                    return false;
                case ElementType.String:
                    {
                        if (!element.TryGet(out string value))
                        {
                            return false;
                        }
                        output.Write('"');
                        if (escape)
                        {
                            WriteString(value, output);
                        }
                        else
                        {
                            output.Write(value);
                        }
                        output.Write('"');
                    }
                    return true;
                case ElementType.StringFormat:
                    {
                        if (!element.TryGet(out var format, out var list))
                        {
                            return false;
                        }
                        output.Write('"');
                        if (!StringFormatTo(format, list, output, escape))
                        {
                            return false;
                        }
                        output.Write('"');
                    }
                    return true;
                case ElementType.Number:
                    {
                        if (!element.TryGet(out double value))
                        {
                            return false;
                        }
                        output.Write(value);
                    }
                    return true;
                case ElementType.Bool:
                    {
                        if (!element.TryGet(out bool value))
                        {
                            return false;
                        }
                        output.Write(value ? "true" : "false");
                    }
                    return true;
                case ElementType.Long:
                    {
                        if (!element.TryGet(out long value))
                        {
                            return false;
                        }
                        output.Write(value);
                    }
                    return true;
                case ElementType.Json:
                    {
                        if (!element.TryGet(out IExternalJson? value))
                        {
                            return false;
                        }
                        value.WriteTo(output);
                    }
                    return true;
                case ElementType.Object:
                    {
                        if (!element.TryGet(out IJsonObject? value))
                        {
                            return false;
                        }
                        if (!value.Serialize(output))
                        {
                            return false;
                        }
                    }
                    return true;
                case ElementType.Array:
                {
                    if (!element.TryGet(out IJsonArray? value))
                    {
                        return false;
                    }

                    output.Write('[');
                    for (int j = 0; j < value.Count; ++j)
                    {
                        if (j != 0)
                        {
                            output.Write(", ");
                        }

                        if (!value[j].WriteTo(output))
                        {
                            return false;
                        }
                    }
                    output.Write(']');
                    return true;
                }
            }
            return false;
        }

        private static bool StringFormatTo(string format, object[] list, System.IO.TextWriter output, bool escape)
        {
            const int InText = 0;
            const int InBracket = 1;
            const int InBracketFormat = 2;

            int state = InText;
            int defaultElementId = 0;

            int idEndPos = 0;

            for (int i = 0; i < format.Length; ++i)
            {
                char ch = format[i];
                switch (ch)
                {
                    case '{':
                        if (state == InText)
                        {
                            state = InBracket;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case ':':
                        if (state == InBracket)
                        {
                            state = InBracketFormat;
                            idEndPos = i - 1;
                        }
                        else
                        {
                            output.Write(ch);
                        }
                        break;
                    case '}':
                        if (state == InBracket || state == InBracketFormat)
                        {
                            string? elementFormat;
                            if (state == InBracket)
                            {
                                idEndPos = i - 1;
                                elementFormat = null;
                            }
                            else
                            {
                                elementFormat = format.Substring(idEndPos + 2, i - (idEndPos + 2));
                            }

                            int elementId = 0;

                            int power = 1;
                            for (int pos = idEndPos; pos >= 0; --pos)
                            {
                                char digit = format[pos];
                                if (digit == '{')
                                {
                                    if (power == 1)
                                    {
                                        elementId = -1;
                                    }
                                    break;
                                }
                                if (digit >= '0' && digit <= '9' && power < 1e6)
                                {
                                    elementId += power * (digit - '0');
                                    power *= 10;
                                }
                                else
                                {
                                    elementId = -1;
                                    break;
                                }
                            }

                            if (elementId == -1)
                            {
                                elementId = defaultElementId++;
                            }

                            if (elementId >= list.Length)
                            {
                                return false;
                            }

                            if (list[elementId] is IExternalJson subJson)
                            {
                                if (escape)
                                {
                                    subJson.WriteToAsText(output);
                                }
                                else
                                {
                                    subJson.WriteTo(output);
                                }
                            }
                            else
                            {
                                var str = ToString(list[elementId], elementFormat);
                                if (escape)
                                {
                                    WriteString(str, output);
                                }
                                else
                                {
                                    output.Write(str);
                                }
                            }

                            state = InText;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    default:
                        if (state == InText)
                        {
                            if (escape)
                            {
                                WriteChar(ch, output);
                            }
                            else
                            {
                                output.Write(ch);
                            }
                        }
                        break;
                }
            }

            return true;
        }

        private static string ToString(object? obj, string? format)
        {
            if (obj == null)
            {
                return "null";
            }

            if (format == null)
            {
                return obj.ToString();
            }

            if (obj is bool boolValue)
            {
                return boolValue.ToString();
            }

            if (obj is char charValue)
            {
                return charValue.ToString();
            }

            if (obj is sbyte sByteValue)
            {
                return sByteValue.ToString(format);
            }

            if (obj is byte byteValue)
            {
                return byteValue.ToString(format);
            }

            if (obj is Int16 shortValue)
            {
                return shortValue.ToString(format);
            }

            if (obj is UInt16 uShortValue)
            {
                return uShortValue.ToString(format);
            }

            if (obj is Int32 intValue)
            {
                return intValue.ToString(format);
            }

            if (obj is UInt32 uIntValue)
            {
                return uIntValue.ToString(format);
            }

            if (obj is Int64 longValue)
            {
                return longValue.ToString(format);
            }

            if (obj is UInt64 uLongValue)
            {
                return uLongValue.ToString(format);
            }

            if (obj is float floatValue)
            {
                return floatValue.ToString(format);
            }

            if (obj is double doubleValue)
            {
                return doubleValue.ToString(format);
            }

            if (obj is Decimal decimalValue)
            {
                return decimalValue.ToString(format);
            }

            if (obj is DateTime dateTime)
            {
                return dateTime.ToString(format);
            }

            return obj.ToString();
        }

        private static void WriteChar(char ch, System.IO.TextWriter dst)
        {
            switch (ch)
            {
                case '"':
                    dst.Write('\'');
                    break;
                case '\\':
                    dst.Write("\\\\");
                    break;
                case '\t':
                    dst.Write("\\t");
                    break;
                case '\r':
                    break;
                case '\n':
                    dst.Write("\\n");
                    break;
                default:
                    dst.Write(ch);
                    break;
            }
        }

        private static void WriteString(string str, System.IO.TextWriter dst)
        {
            int len = str.Length;
            for (int i = 0; i < len; ++i)
            {
                WriteChar(str[i], dst);
            }
        }
    }
}