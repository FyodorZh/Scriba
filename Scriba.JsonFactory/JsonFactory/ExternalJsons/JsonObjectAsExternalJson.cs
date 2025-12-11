using System.IO;

namespace Scriba.JsonFactory.ExternalJsons
{
    public class JsonObjectAsExternalJson : SimpleExternalJson
    {
        private JsonObject? _object;

        public IJsonObject? Root => _object;

        public JsonObjectAsExternalJson()
        {
            Reinit();
        }

        public bool Reinit()
        {
            if (_object == null)
            {
                _object = Pool<JsonObject>.New();
                return true;
            }
            return false;
        }

        public override void Release()
        {
            if (_object != null)
            {
                _object.Free();
                _object = null;
            }
        }

        public override void WriteTo(TextWriter output)
        {
            _object?.Serialize(output);
        }
    }
}
