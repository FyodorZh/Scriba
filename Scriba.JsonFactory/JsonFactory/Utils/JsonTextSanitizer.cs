using System.IO;
using System.Text;

namespace Scriba.JsonFactory.Utils
{
    /// <summary>
    /// A <see cref="TextWriter"/> that transforms characters written to it before forwarding
    /// them to a core writer, producing text-safe output from JSON data:
    /// <list type="bullet">
    ///   <item>Control characters (U+0000–U+001F) are backslash-escaped.</item>
    ///   <item>Double quotes (<c>"</c>) are replaced with single quotes (<c>'</c>).</item>
    ///   <item>Backslashes (<c>\</c>) are replaced with forward slashes (<c>/</c>).</item>
    /// </list>
    /// Designed for zero-allocation reuse via object pooling. The fast path in
    /// <see cref="Write(string)"/> forwards entirely safe strings with a single call
    /// to the core writer, avoiding per-character virtual dispatch when no escaping
    /// is needed.
    /// </summary>
    internal sealed class JsonTextSanitizer : TextWriter
    {
        private TextWriter? _coreWriter;

        public void Init(TextWriter? core)
        {
            _coreWriter = core;
        }

        public override Encoding Encoding => _coreWriter?.Encoding ?? Encoding.UTF8;

        public override void Write(char value)
        {
            var writer = _coreWriter;
            if (writer == null) return;

            if (value < 0x20)
            {
                writer.Write('\\');
                writer.Write(value);
            }
            else if (value == '"')
            {
                writer.Write('\'');
            }
            else if (value == '\\')
            {
                writer.Write('/');
            }
            else
            {
                writer.Write(value);
            }
        }

        public override void Write(string value)
        {
            var writer = _coreWriter;
            if (writer == null || value == null) return;

            int length = value.Length;

            int i = 0;
            for (; i < length; i++)
            {
                char c = value[i];
                if (c < 0x20 || c == '"' || c == '\\')
                    break;
            }

            if (i == length)
            {
                writer.Write(value);
                return;
            }

            for (int j = 0; j < i; j++)
                writer.Write(value[j]);

            for (; i < length; i++)
            {
                char c = value[i];
                if (c < 0x20)
                {
                    writer.Write('\\');
                    writer.Write(c);
                }
                else if (c == '"')
                {
                    writer.Write('\'');
                }
                else if (c == '\\')
                {
                    writer.Write('/');
                }
                else
                {
                    writer.Write(c);
                }
            }
        }
    }
}
