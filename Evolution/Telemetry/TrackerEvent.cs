using Newtonsoft.Json;

namespace Telemetry
{
    public class TrackerEvent
    {
        [JsonProperty(Order = -2)]
        public EventType Type { get; private set; }

        [JsonProperty(Order = -2)]
        public long TimeStamp { get; private set; }

        [JsonProperty(Order = -2)]
        public string SessionID { get; private set; }

        // Aniade la coma por comodidad, el unico evento que no la tiene es el SessionEnd
        public virtual string ToJSON() { return $"{JsonConvert.SerializeObject(this)},"; }

        protected TrackerEvent(EventType type, long timeStamp, string sessionID)
        {
            Type = type;
            TimeStamp = timeStamp;
            SessionID = sessionID;
        }

        protected TrackerEvent(EventType type, long timeStamp)
        {
            Type = type;
            TimeStamp = timeStamp;
            SessionID = Tracker.Instance.SessionID;
        }

        protected TrackerEvent(EventType type)
        {
            Type = type;
            TimeStamp = Tracker.Instance.GetSessionTimestamp();
            SessionID = Tracker.Instance.SessionID;
        }
    }
}
