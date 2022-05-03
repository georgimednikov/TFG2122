using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Telemetry.Events
{
    public class CreatureApplyPoison : CreatureEvent
    {
        public int victimID { get; private set; }
        public string victimSpecies { get; private set; }
        public float damage { get; private set; }
        public int duration { get; private set; }
        public CreatureApplyPoison(int tick, int creatureID, string speciesName, int vID, string vSp, float dmg, int dur) : base(EventType.CreatureApplyPoison, tick, creatureID, speciesName) 
        { victimID = vID; victimSpecies = vSp; damage = dmg; duration = dur; }
    }
}
