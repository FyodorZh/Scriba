using System.IO;
using System.Text;

namespace Scriba.JsonFactory.Utils
{
    internal sealed class EscapeCharTextFilter : TextWriter
    {
        private TextWriter? _coreWriter;

        public void Init(TextWriter? core)
        {
            _coreWriter = core;
        }

        public override Encoding Encoding => _coreWriter?.Encoding ?? Encoding.UTF8;

        public override void Write(char value)
        {
            if (char.IsControl(value))
            {
                EscapeChar(value);
            }
            else
            {
                switch (value)
                {
                    case '"':
                        _coreWriter?.Write('\'');
                        break;

                    case '\\':
                        _coreWriter?.Write('/');
                        break;

                    default:
                        _coreWriter?.Write(value);
                        break;
                }
            }
        }

        public override void Write(string value)
        {
            foreach (var ch in value)
            {
                Write(ch);
            }
        }

        private void EscapeChar(char charToEscape)
        {
            _coreWriter?.Write('\\');
            _coreWriter?.Write(charToEscape);
        }
    }
}
