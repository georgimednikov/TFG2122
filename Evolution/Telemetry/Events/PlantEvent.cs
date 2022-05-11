using Newtonsoft.Json;

namespace Telemetry.Events
{
    public abstract class PlantEvent : TrackerEvent
    {
        public int Tick { get; private set; }
        public int PlantID { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public PlantEvent(EventType type, int tick, int id, int x, int y) : base(type)
        {
            Tick = tick; PlantID = id; X = x; Y = y;
        }
    }
}
