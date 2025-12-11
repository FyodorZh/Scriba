using System.IO;
using Scriba.JsonFactory.Utils;

namespace Scriba.JsonFactory.ExternalJsons
{
    public abstract class SimpleExternalJson : IExternalJson
    {
        public abstract void Release();

        public abstract void WriteTo(TextWriter output);

        public void WriteToAsText(TextWriter output)
        {
            var writer = Pool<EscapeCharTextFilter>.New();
            writer.Init(output);
            try
            {
                WriteTo(writer);
            }
            finally
            {
                writer.Init(null);
                Pool<EscapeCharTextFilter>.Free(writer);
            }
        }
    }
}
