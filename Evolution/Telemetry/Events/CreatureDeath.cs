using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Telemetry.Events
{
    public class CreatureDeath : CreatureEvent
    {  
        [JsonConverter(typeof(StringEnumConverter))]
        public DeathType DeathType { get; private set; }
        public int killerID { get; private set; }
        public double killerDmg { get; private set; }
        public CreatureDeath(int tick, int creatureID, string speciesName, DeathType deathType, int kID, double kDmg, int x, int y) : base(EventType.CreatureDeath, tick, creatureID, speciesName, x, y) 
        { DeathType = deathType; killerID = kID; killerDmg = kDmg; }
        public override string ToJSON()
        {
            string aux = base.ToJSON();
            return $"{aux.Remove(aux.Length - 1)}\n]";
        }
    }
}
