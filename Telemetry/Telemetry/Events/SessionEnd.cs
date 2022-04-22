using Newtonsoft.Json;


namespace Telemetry.Events
{
    public class SessionEnd : TrackerEvent
    {
        public SessionEnd() : base(EventType.SessionEnd) {}
        public override string ToJSON() 
        {
            string aux = base.ToJSON();
            return $"{aux.Remove(aux.Length - 1)}\n]"; 
        }

    }
}
