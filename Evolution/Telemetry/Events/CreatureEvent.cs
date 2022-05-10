using Newtonsoft.Json;

namespace Telemetry.Events
{
    public abstract class CreatureEvent : TrackerEvent
    {
        public int Tick { get; private set; }
        public int CreatureID { get; private set; }
        public string Species { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public CreatureEvent(EventType type, int tick, int id, string species, int x, int y) : base(type)
        {
            Tick = tick; CreatureID = id; Species = species; X = x; Y = y;
        }
    }
}
