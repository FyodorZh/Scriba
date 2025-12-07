namespace Scriba.Consumers
{
    public struct LogEntity
    {
        public System.DateTime Time { get; }
        public string Severity { get; }
        public string Text { get; }
        public string[] Stack { get; }

        public LogEntity(System.DateTime time, string severity, string text, string[] stack)
        {
            Time = time;
            Severity = severity;
            Text = text;
            Stack = stack;
        }
    }
}