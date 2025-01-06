namespace RpgGame
{
    public class Event
    {
        public string Message { get; }
        public DateTime Timestamp { get; }

        public Event(string message)
        {
            Message = message;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Message}";
        }
    }
}
