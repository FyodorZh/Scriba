using System.IO;
using System.Text;

namespace Scriba.JsonFactory.Utils
{
    internal sealed class EscapeCharTextFilter : TextWriter
    {
        private TextWriter mWriter;

        public void Init(TextWriter core)
        {
            mWriter = core;
        }

        public override Encoding Encoding => mWriter.Encoding;

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
                        mWriter.Write('\'');
                        break;

                    case '\\':
                        mWriter.Write('/');
                        break;

                    default:
                        mWriter.Write(value);
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
            mWriter.Write('\\');
            mWriter.Write(charToEscape);
        }
    }
}
