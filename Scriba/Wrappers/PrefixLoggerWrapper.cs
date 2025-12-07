using JsonFactory;

namespace Scriba
{
    internal class PrefixLoggerWrapper : LoggerWrapper
    {
        private readonly string mTagValue;
        private readonly string mTagName;

        public PrefixLoggerWrapper(ILogger log, string tagName, string tagValue)
            : base(log)
        {
            mTagValue = tagValue;
            mTagName = tagName;
        }

        protected override void AppendTags(IJsonArray tags)
        {
            var tag = tags.AddObject();
            tag.AddElement(mTagName, mTagValue);
        }
    }

    public static class PrefixLoggerWrapper_Ext
    {
        public static ILogger Wrap(this ILogger logger, string tagName, string tagValue)
        {
            return new PrefixLoggerWrapper(logger, tagName, tagValue);
        }
    }
}