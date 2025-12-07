using System;

namespace Scriba
{
    public static class SeveritySerializer
    {
        private static readonly string[] mSeverityMap;

        static SeveritySerializer()
        {
            Type eType = typeof(Severity);
            var fields = eType.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            mSeverityMap = new string[fields.Length];
            for (int i = 0; i < fields.Length; ++i)
            {
                mSeverityMap[i] = fields[i].GetValue(null).ToString();
            }
        }

        public static string Serialize(this Severity severity)
        {
            int id = (int)severity;
            if (id < mSeverityMap.Length)
            {
                return mSeverityMap[id];
            }
            return "UNKNOWN";
        }
    }
}