namespace Telemetry
{
    public class JSONSerializer : ISerializer
    {
        public string Serialize(TrackerEvent tEvent)
        {
            return tEvent.ToJSON();
        }
    }
}
