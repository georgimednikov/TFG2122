using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Telemetry.Events
{
    public class Die : TrackerEvent
    {
        public Die(int tick, int creatureID, DeathType deathType, string speciesName) : base(EventType.Die) { 
            Tick = tick; CreatureID = creatureID; DeathType = deathType; SpeciesName = speciesName;
        }
        public int Tick { get; private set; }
        public int CreatureID { get; private set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DeathType DeathType { get; private set; }
        public string SpeciesName { get; private set; }
    }
}
