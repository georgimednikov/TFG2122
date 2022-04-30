using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Telemetry.Events
{
    public class CreatureDeath : CreatureEvent
    {  
        [JsonConverter(typeof(StringEnumConverter))]
        public DeathType DeathType { get; private set; }
        public CreatureDeath(int tick, int creatureID, string speciesName, DeathType deathType) : base(EventType.CreatureDeath, tick, creatureID, speciesName) { DeathType = deathType; }
        public override string ToJSON()
        {
            string aux = base.ToJSON();
            return $"{aux.Remove(aux.Length - 1)}\n]";
        }
    }
}
