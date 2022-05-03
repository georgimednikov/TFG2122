using Newtonsoft.Json;

namespace Telemetry.Events
{
    public abstract class CreatureEvent : TrackerEvent
    {
        public int Tick { get; private set; }
        public int CreatureID { get; private set; }
        public string Species { get; private set; }
        public CreatureEvent(EventType type, int tick, int id, string species) : base(type)
        {
            Tick = tick; CreatureID = id; Species = species;
        }
    }
}
